using Application.DTOs.Dashboard;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;

namespace Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository
        _dashboardRepository;

    public DashboardService(
        IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardSummaryDto>
        GetDashboardSummaryAsync()
    {
        return new DashboardSummaryDto
        {
            TotalEmployees =
                await _dashboardRepository
                    .GetTotalEmployeesAsync(),

            AllocatedEmployees =
                await _dashboardRepository
                    .GetAllocatedEmployeesAsync(),

            BenchEmployees =
                await _dashboardRepository
                    .GetBenchEmployeesAsync(),

            ActiveProjects =
                await _dashboardRepository
                    .GetActiveProjectsAsync(),

            SubmittedTimesheets =
                await _dashboardRepository
                    .GetSubmittedTimesheetsAsync(),

            MissedTimesheets =
                await _dashboardRepository
                    .GetMissedTimesheetsAsync()
        };
    }

    public async Task<ResourceDashboardDto> GetResourceDashboardAsync(int? managerId = null)
    {
        var benchEmployeesList = await _dashboardRepository
            .GetBenchEmployeesWithSkillsAsync();

        var allocatedEmployeesList = await _dashboardRepository
            .GetAllocatedEmployeesWithAllocationsAsync();

        if (managerId.HasValue)
        {
            benchEmployeesList = benchEmployeesList.Where(e => e.ManagerId == managerId.Value).ToList();
            allocatedEmployeesList = allocatedEmployeesList.Where(e => e.ManagerId == managerId.Value).ToList();
        }

        var benchDtos = benchEmployeesList.Select(e =>
            new BenchEmployeeDto
            {
                Id = e.Id,
                FullName = e.FullName,
                Department = e.Department,
                Skills = e.EmployeeSkills
                    .Select(es => es.Skill.SkillName)
                    .ToList()
            }).ToList();

        var activeDtos = allocatedEmployeesList.Select(e =>
        {
            var totalPercent = e.Allocations
                .Sum(a => a.UtilizationPercent);

            return new ActiveEmployeeDto
            {
                Id = e.Id,
                FullName = e.FullName,
                TotalAllocationPercent = totalPercent,
                Availability = totalPercent >= 100
                    ? "FULL"
                    : $"{100 - totalPercent}% free"
            };
        }).ToList();

        var partialCount = activeDtos
            .Count(a => a.TotalAllocationPercent < 100);

        return new ResourceDashboardDto
        {
            BenchEmployees = benchDtos,
            ActiveEmployees = activeDtos,
            BenchCount = benchDtos.Count,
            PartiallyAllocatedCount = partialCount
        };
    }

    public async Task<EmployeeDetailDto?>
        GetEmployeeDetailAsync(int employeeId)
    {
        var employee = await _dashboardRepository
            .GetEmployeeDetailAsync(employeeId);

        if (employee == null)
            return null;

        var recentTags = await _dashboardRepository
            .GetRecentActivityTagsAsync(employeeId, 4);

        var totalPercent = employee.Allocations
            .Sum(a => a.UtilizationPercent);

        return new EmployeeDetailDto
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Department = employee.Department,
            Status = $"{employee.Status} ({totalPercent}%)",
            TotalAllocationPercent = totalPercent,
            ProfileSkills = employee.EmployeeSkills
                .Select(es => es.Skill.SkillName)
                .ToList(),
            ActiveAllocations = employee.Allocations
                .Select(a => new EmployeeAllocationDto
                {
                    ProjectName = a.Project.ProjectName,
                    UtilizationPercent = a.UtilizationPercent,
                    FromDate = a.FromDate,
                    ToDate = a.ToDate
                }).ToList(),
            RecentActivityTags = recentTags
        };
    }

    public async Task<UtilizationReportDto>
        GetUtilizationReportAsync()
    {
        var employees = await _dashboardRepository
            .GetAllEmployeesWithAllocationsAsync();

        var employeeDtos = employees.Select(e =>
        {
            var totalPercent = e.Allocations
                .Sum(a => a.UtilizationPercent);

            var status = totalPercent == 0
                ? "Bench"
                : totalPercent >= 100
                    ? "Full"
                    : "Partial";

            return new EmployeeUtilizationDto
            {
                EmployeeId = e.Id,
                FullName = e.FullName,
                Department = e.Department,
                UtilizationPercent = totalPercent,
                AllocationStatus = status
            };
        }).ToList();

        return new UtilizationReportDto
        {
            Employees = employeeDtos
        };
    }

    public async Task<TeamTimesheetReportDto>
        GetTeamTimesheetReportAsync(DateTime? weekStartDate)
    {
        var effectiveDate = weekStartDate
            ?? GetLastMonday(DateTime.UtcNow);

        var timesheets = await _dashboardRepository
            .GetTimesheetsByWeekAsync(effectiveDate);

        var entries = timesheets.SelectMany(t =>
            t.Entries.Select(entry =>
                new TeamTimesheetEntryDto
                {
                    EmployeeId = t.EmployeeId,
                    EmployeeName = t.Employee.FullName,
                    ProjectName = entry.Project.ProjectName,
                    HoursWorked = entry.HoursWorked,
                    Status = t.Status.ToString()
                        .ToUpperInvariant()
                })).ToList();

        return new TeamTimesheetReportDto
        {
            WeekStartDate = effectiveDate,
            Entries = entries
        };
    }

    public async Task<SkillMatrixReportDto>
        GetSkillMatrixReportAsync()
    {
        var employees = await _dashboardRepository
            .GetAllEmployeesWithSkillsAsync();

        var employeeDtos = employees.Select(e =>
            new EmployeeSkillMatrixDto
            {
                EmployeeId = e.Id,
                FullName = e.FullName,
                Department = e.Department,
                Skills = e.EmployeeSkills
                    .Select(es => new SkillDetailDto
                    {
                        SkillName = es.Skill.SkillName,
                        Category = es.Skill.Category,
                        ProficiencyLevel = es.ProficiencyLevel
                            .ToString()
                    }).ToList()
            }).ToList();

        return new SkillMatrixReportDto
        {
            Employees = employeeDtos
        };
    }

    public async Task<ProjectDashboardDto>
        GetProjectDashboardAsync()
    {
        var projects = await _dashboardRepository
            .GetAllProjectsWithDetailsAsync();

        var today = DateTime.UtcNow.Date;

        var projectDtos = projects.Select(p =>
        {
            var completedSp = p.Milestones
                .Where(m => m.Status == MilestoneStatus.Done)
                .Sum(m => m.StoryPoints);

            var progressPct = p.TotalStoryPoints > 0
                ? Math.Round(
                    (decimal)completedSp
                        / p.TotalStoryPoints * 100, 2)
                : 0m;

            return new ProjectDashboardItemDto
            {
                ProjectId = p.Id,
                ProjectName = p.ProjectName,
                ManagerName = p.Manager.FullName,
                EndDate = p.EndDate,
                Status = p.Status.ToString(),
                HealthStatus = p.HealthStatus.ToString(),
                TotalStoryPoints = p.TotalStoryPoints,
                CompletedStoryPoints = completedSp,
                ProgressPercentage = progressPct,
                ResourceCount = p.Allocations.Count,
                Milestones = p.Milestones
                    .Select(m => new ProjectMilestoneSummaryDto
                    {
                        Title = m.Title,
                        DueDate = m.DueDate,
                        StoryPoints = m.StoryPoints,
                        Status = m.Status.ToString(),
                        IsOverdue = m.Status
                            != MilestoneStatus.Done
                            && m.DueDate < today
                    }).ToList()
            };
        }).ToList();

        return new ProjectDashboardDto
        {
            Projects = projectDtos
        };
    }

    private static DateTime GetLastMonday(DateTime date)
    {
        var daysBack = ((int)date.DayOfWeek
            - (int)DayOfWeek.Monday + 7) % 7;

        return date.Date.AddDays(-daysBack);
    }
}