using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRM.Application.DTOs;
using PRM.Application.Exceptions;
using PRM.Application.Interfaces;
using PRM.Core.Entities;
using PRM.Core.Interfaces;

namespace PRM.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepo;

    public ProjectService(IProjectRepository projectRepo)
    {
        _projectRepo = projectRepo;
    }

    public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
    {
        var projects = await _projectRepo.GetAllAsync();
        return projects.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectDto>> GetMyProjectsAsync(int managerId)
    {
        var projects = await _projectRepo.GetByManagerIdAsync(managerId);
        return projects.Select(MapToDto);
    }

    public async Task<ProjectDto> GetProjectByIdAsync(int id)
    {
        var project = await _projectRepo.GetByIdWithMilestonesAsync(id);
        if (project == null) throw new NotFoundException("Project", id);
        return MapToDto(project);
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request)
    {
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ManagerId = request.ManagerId,
            TotalStoryPoints = request.TotalStoryPoints
        };

        await _projectRepo.AddAsync(project);
        return await GetProjectByIdAsync(project.Id);
    }

    public async Task UpdateProjectAsync(UpdateProjectRequest request)
    {
        var project = await _projectRepo.GetByIdAsync(request.Id);
        if (project == null) throw new NotFoundException("Project", request.Id);

        project.Name = request.Name;
        project.Description = request.Description;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.Status = request.Status;
        project.ManagerId = request.ManagerId;
        project.TotalStoryPoints = request.TotalStoryPoints;
        project.HealthStatus = request.HealthStatus;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepo.UpdateAsync(project);
    }

    public async Task<MilestoneDto> AddMilestoneAsync(int projectId, CreateMilestoneRequest request)
    {
        var project = await _projectRepo.GetByIdWithMilestonesAsync(projectId);
        if (project == null) throw new NotFoundException("Project", projectId);

        var milestone = new Milestone
        {
            Title = request.Title,
            DueDate = request.DueDate,
            StoryPoints = request.StoryPoints,
            Status = PRM.Core.Enums.MilestoneStatus.NotStarted
        };

        project.Milestones.Add(milestone);
        await _projectRepo.UpdateAsync(project);

        return new MilestoneDto(milestone.Id, project.Id, milestone.Title, milestone.DueDate, milestone.StoryPoints, milestone.Status);
    }

    public async Task UpdateMilestoneStatusAsync(int projectId, int milestoneId, PRM.Core.Enums.MilestoneStatus status)
    {
        var project = await _projectRepo.GetByIdWithMilestonesAsync(projectId);
        if (project == null) throw new NotFoundException("Project", projectId);

        var milestone = project.Milestones.FirstOrDefault(m => m.Id == milestoneId);
        if (milestone == null) throw new NotFoundException("Milestone", milestoneId);

        milestone.Status = status;
        await _projectRepo.UpdateAsync(project);
    }

    private static ProjectDto MapToDto(Project p)
    {
        var milestones = p.Milestones?.Select(m => new MilestoneDto(m.Id, m.ProjectId, m.Title, m.DueDate, m.StoryPoints, m.Status)).ToList() ?? new List<MilestoneDto>();
        var allocations = p.Allocations?.Select(a => new AllocationDto(a.Id, a.UserId, a.User?.FullName ?? "Unknown", a.ProjectId, p.Name, a.UtilisationPercent, a.FromDate, a.ToDate)).ToList();
        return new ProjectDto(
            p.Id, p.Name, p.Description, p.StartDate, p.EndDate, 
            p.Status, p.ManagerId, p.TotalStoryPoints, p.HealthStatus, milestones, allocations);
    }

    public async Task<List<string>> GetProjectRiskFlagsAsync(int projectId)
    {
        var project = await _projectRepo.GetByIdWithDetailsForRiskAsync(projectId);
        if (project == null) throw new NotFoundException("Project", projectId);

        var flags = new List<string>();

        // 1. Check for overdue milestones
        var overdueMilestones = project.Milestones
            .Where(m => m.DueDate < DateTime.Today && m.Status != PRM.Core.Enums.MilestoneStatus.Done)
            .ToList();
        
        foreach (var om in overdueMilestones)
        {
            var daysOverdue = (int)(DateTime.Today - om.DueDate).TotalDays;
            flags.Add($"✗ {om.Title} milestone is {daysOverdue} days overdue");
        }

        // 2. Check for low logged hours vs expected for recent weeks (e.g. last week)
        DateTime today = DateTime.Today;
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        DateTime lastMonday = today.AddDays(-diff).AddDays(-7).Date;

        var maxWeeklyHoursStr = Environment.GetEnvironmentVariable("MAX_WEEKLY_HOURS") ?? "40"; // Or fetch from DB. Keep it simple.
        int maxWeeklyHours = int.TryParse(maxWeeklyHoursStr, out int mwh) ? mwh : 40;

        foreach (var alloc in project.Allocations.Where(a => a.FromDate <= lastMonday.AddDays(6) && a.ToDate >= lastMonday))
        {
            var expectedHours = (alloc.UtilisationPercent / 100m) * maxWeeklyHours;
            
            // Sum hours logged by this user for this project last week
            var loggedHours = project.TimesheetEntries
                .Where(te => te.Timesheet != null && te.Timesheet.UserId == alloc.UserId && te.Timesheet.WeekStartDate.Date == lastMonday)
                .Sum(te => te.HoursWorked);

            if (loggedHours < expectedHours * 0.5m) // Arbitrary threshold for "low"
            {
                flags.Add($"✗ {alloc.User?.FullName} logged only {loggedHours} hrs last week (expected {expectedHours:0.#} hrs)");
            }
        }

        if (flags.Count == 0)
        {
            flags.Add("✓ Resources are correctly allocated");
            flags.Add("✓ Project is on track");
        }

        return flags;
    }
}
