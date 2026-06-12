using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IMilestoneRepository _milestoneRepository;
    private readonly IAllocationRepository _allocationRepository;
    private readonly ITimesheetRepository _timesheetRepository;

    public MaintenanceService(
        IEmployeeRepository employeeRepository,
        IProjectRepository projectRepository,
        IMilestoneRepository milestoneRepository,
        IAllocationRepository allocationRepository,
        ITimesheetRepository timesheetRepository)
    {
        _employeeRepository = employeeRepository;
        _projectRepository = projectRepository;
        _milestoneRepository = milestoneRepository;
        _allocationRepository = allocationRepository;
        _timesheetRepository = timesheetRepository;
    }

    public async Task RunMaintenanceAsync()
    {
        var today = DateTime.UtcNow.Date;

        // 1. Employee Status Calculation
        var employees = await _employeeRepository.GetAllAsync();
        var allocations = await _allocationRepository.GetAllAsync();

        foreach (var employee in employees)
        {
            var hasActiveAllocation = allocations.Any(a => 
                a.EmployeeId == employee.Id && 
                today >= a.FromDate.Date && 
                today <= a.ToDate.Date);

            var newStatus = hasActiveAllocation ? EmployeeStatus.Allocated : EmployeeStatus.Bench;
            
            if (employee.Status != newStatus)
            {
                employee.Status = newStatus;
                _employeeRepository.Update(employee);
            }
        }

        await _employeeRepository.SaveChangesAsync();

        // 2. Project Health Update
        var projects = await _projectRepository.GetAllAsync();
        foreach (var project in projects.Where(p => p.Status == ProjectStatus.Active))
        {
            var projectMilestones = await _milestoneRepository.GetByProjectIdAsync(project.Id);
            var overdueMilestones = projectMilestones.Count(m => m.Status != MilestoneStatus.Done && m.DueDate.Date < today);

            if (overdueMilestones > 0)
            {
                project.HealthStatus = ProjectHealthStatus.AtRisk;
            }
            else if (projectMilestones.Any(m => m.Status != MilestoneStatus.Done && (m.DueDate.Date - today).TotalDays <= 7))
            {
                // Milestones due within a week
                project.HealthStatus = ProjectHealthStatus.Attention;
            }
            else
            {
                project.HealthStatus = ProjectHealthStatus.OnTrack;
            }

            _projectRepository.Update(project);
        }
        await _projectRepository.SaveChangesAsync();

        // 3. Timesheet Flagging
        // Find the start date of the previous week (e.g., previous Monday)
        var daysSinceMonday = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
        if (daysSinceMonday < 0) daysSinceMonday += 7;
        var currentWeekStart = today.AddDays(-daysSinceMonday);
        var previousWeekStart = currentWeekStart.AddDays(-7);
        var previousWeekEnd = currentWeekStart.AddDays(-1);

        foreach (var employee in employees)
        {
            // Only care if they had allocations last week? Or everyone needs a timesheet?
            // "Identify employees who did not submit a timesheet for the previous week"
            var timesheets = await _timesheetRepository.GetByEmployeeIdAsync(employee.Id);
            
            var hasTimesheetForLastWeek = timesheets.Any(t => 
                t.WeekStartDate.Date == previousWeekStart.Date);

            if (!hasTimesheetForLastWeek && employee.Status == EmployeeStatus.Allocated)
            {
                // Create missed timesheet
                var missedTimesheet = new Timesheet
                {
                    EmployeeId = employee.Id,
                    WeekStartDate = previousWeekStart.Date,
                    Status = TimesheetStatus.Missed,
                    TotalHours = 0
                };
                await _timesheetRepository.AddAsync(missedTimesheet);
            }
        }
        await _timesheetRepository.SaveChangesAsync();
    }
}
