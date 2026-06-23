using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PRM.Application.Exceptions;
using PRM.Application.Interfaces;
using PRM.Core.Interfaces;

namespace PRM.Application.Services;

public class AiService : IAiService
{
    private readonly ILlmProvider _llmProvider;
    private readonly IProjectRepository _projectRepo;
    private readonly IUserRepository _userRepo;
    private readonly IAllocationRepository _allocationRepo;
    private readonly ITimesheetRepository _timesheetRepo;
    private readonly ISystemConfigRepository _configRepo;

    public AiService(
        ILlmProvider llmProvider, 
        IProjectRepository projectRepo, 
        IUserRepository userRepo,
        IAllocationRepository allocationRepo,
        ITimesheetRepository timesheetRepo,
        ISystemConfigRepository configRepo)
    {
        _llmProvider = llmProvider;
        _projectRepo = projectRepo;
        _userRepo = userRepo;
        _allocationRepo = allocationRepo;
        _timesheetRepo = timesheetRepo;
        _configRepo = configRepo;
    }

    public async Task<string> GetSkillMatchRecommendationAsync(string requirement, int managerId)
    {
        // Simple heuristic to extract hours per week
        int requiredHours = 0;
        var match = Regex.Match(requirement, @"(\d+)\s*h(ou)?rs?");
        if (match.Success)
        {
            requiredHours = int.Parse(match.Groups[1].Value);
        }

        var maxHoursConfigValue = await _configRepo.GetValueAsync("MaxWeeklyHours");
        int maxWeeklyHours = int.TryParse(maxHoursConfigValue, out int maxh) ? maxh : 40;

        int requiredUtilisation = requiredHours > 0 ? (int)Math.Ceiling((double)requiredHours / maxWeeklyHours * 100) : 0;

        var users = await _userRepo.GetAllAsync();
        var activeAllocations = await _allocationRepo.GetActiveByResourceIdsAsync(users.Select(u => u.Id));

        // Fetch recent timesheets (last 4 weeks) for all users to get activity tags
        var recentTimesheets = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>>();
        foreach (var user in users.Where(u => u.Role?.Name == "Resource"))
        {
            var timesheets = await _timesheetRepo.GetByResourceIdAsync(user.Id);
            var recentTags = timesheets
                .Where(t => t.WeekStartDate >= DateTime.UtcNow.Date.AddDays(-28))
                .SelectMany(t => t.Entries)
                .Where(e => !string.IsNullOrWhiteSpace(e.ActivityTags))
                .SelectMany(e => e.ActivityTags!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Distinct()
                .ToList();
            recentTimesheets[user.Id] = recentTags;
        }

        var validCandidates = users.Where(u => u.Role?.Name == "Resource").Select(u => 
        {
            var userAllocs = activeAllocations.Where(a => a.UserId == u.Id).ToList();
            int currentUtil = userAllocs.Sum(a => a.UtilisationPercent);
            int freeHoursPerWeek = (int)Math.Floor((double)(100 - currentUtil) / 100 * maxWeeklyHours);
            var activityTags = recentTimesheets.ContainsKey(u.Id) ? recentTimesheets[u.Id] : new System.Collections.Generic.List<string>();
            return new 
            {
                u.Id,
                u.FullName,
                ProfileSkills = u.UserSkills.Select(us => $"{us.Skill?.Name} ({us.ProficiencyLevel})").ToList(),
                FreeCapacityPercent = 100 - currentUtil,
                FreeHoursPerWeek = freeHoursPerWeek,
                Status = currentUtil == 0 ? "ON BENCH" : $"ALLOCATED ({currentUtil}%)",
                RecentActivityTags = activityTags,
                RecentProjects = userAllocs.Select(a => a.Project?.Name).Where(n => n != null).Distinct().ToList()
            };
        })
        .Where(c => c.FreeCapacityPercent >= requiredUtilisation)
        .ToList();

        if (!validCandidates.Any())
        {
            throw new BusinessRuleViolationException($"No candidates found with at least {requiredUtilisation}% free capacity.");
        }

        return await _llmProvider.GetSkillMatchAsync(requirement, validCandidates);
    }

    public async Task<string> GetProjectRiskSummaryAsync(int projectId, int managerId)
    {
        var project = await _projectRepo.GetByIdWithMilestonesAsync(projectId);
        if (project == null) throw new NotFoundException("Project", projectId);

        if (project.ManagerId != managerId)
            throw new BusinessRuleViolationException("You can only analyze risks for your own assigned projects.");

        var recentTimesheets = await _timesheetRepo.GetRecentByProjectAsync(projectId, 4);

        var projectData = new {
            project.Name,
            project.Status,
            project.StartDate,
            project.EndDate,
            project.TotalStoryPoints,
            AllocationsCount = project.Allocations.Count,
            Milestones = project.Milestones.Select(m => new { m.Title, m.Status, m.DueDate, m.StoryPoints }),
            RecentTimesheets = recentTimesheets.SelectMany(t => t.Entries.Where(e => e.ProjectId == projectId)).Select(e => new {
                e.HoursWorked,
                e.ActivityTags
            })
        };

        return await _llmProvider.GetRiskSummaryAsync(projectData);
    }

    public async Task<PRM.Application.DTOs.TeamBuildResultDto> BuildTeamAsync(PRM.Application.DTOs.TeamBuildRequest request, int managerId)
    {
        var users = await _userRepo.GetAllAsync();
        var activeAllocations = await _allocationRepo.GetActiveByResourceIdsAsync(users.Select(u => u.Id));

        var candidates = users.Where(u => u.Role?.Name == "Resource" || u.Role?.Name == "Employee").ToList();
        
        var takenUserIds = new System.Collections.Generic.HashSet<int>();
        var matches = new System.Collections.Generic.List<PRM.Application.DTOs.TeamBuildRoleResultDto>();

        foreach (var role in request.Roles)
        {
            int? assignedUserId = null;
            string? assignedUserName = null;
            string reason = "";

            var qualifiedCandidates = candidates.Where(c => 
                role.RequiredSkills.All(rs => 
                    c.UserSkills.Any(us => 
                        us.Skill != null && 
                        us.Skill.Name.Equals(rs, StringComparison.OrdinalIgnoreCase) && 
                        us.ProficiencyLevel >= role.MinimumProficiency)
                )
            ).ToList();

            if (!qualifiedCandidates.Any())
            {
                reason = "Nobody in the organization has the required skills at the specified proficiency level.";
            }
            else
            {
                var availableQualified = qualifiedCandidates.Where(c => !takenUserIds.Contains(c.Id)).ToList();
                
                var benchQualified = availableQualified.Where(c => 
                {
                    var userAllocs = activeAllocations.Where(a => a.UserId == c.Id).ToList();
                    int currentUtil = userAllocs.Sum(a => a.UtilisationPercent);
                    return currentUtil == 0;
                }).ToList();

                if (benchQualified.Any())
                {
                    var selected = benchQualified.First();
                    assignedUserId = selected.Id;
                    assignedUserName = selected.FullName;
                    reason = $"Perfect match. Candidate is on the bench and meets all skill requirements.";
                    takenUserIds.Add(selected.Id);
                }
                else
                {
                    var occupiedCandidate = availableQualified.FirstOrDefault();
                    if (occupiedCandidate != null)
                    {
                        var alloc = activeAllocations.Where(a => a.UserId == occupiedCandidate.Id).OrderByDescending(a => a.ToDate).FirstOrDefault();
                        if (alloc != null)
                        {
                            reason = $"Skill exists in the organization, but candidate is allocated until {alloc.ToDate:dd-MMM-yyyy}.";
                        }
                        else
                        {
                            reason = "Skill exists, but candidate was assigned to another role in this team.";
                        }
                    }
                    else
                    {
                        reason = "Skill exists, but candidate was assigned to another role in this team.";
                    }
                }
            }

            matches.Add(new PRM.Application.DTOs.TeamBuildRoleResultDto(role.Title, assignedUserId, assignedUserName, reason));
        }

        string summary = $"Team builder processed {request.Roles.Count} roles. {matches.Count(m => m.AssignedUserId.HasValue)} filled, {matches.Count(m => !m.AssignedUserId.HasValue)} unfilled.";
        
        return new PRM.Application.DTOs.TeamBuildResultDto(matches, summary);
    }
}
