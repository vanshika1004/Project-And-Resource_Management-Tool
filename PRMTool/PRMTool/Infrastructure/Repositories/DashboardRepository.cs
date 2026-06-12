using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public DashboardRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // ── Existing summary count methods ──

    public async Task<int> GetTotalEmployeesAsync()
    {
        return await _context.Employees.CountAsync();
    }

    public async Task<int> GetAllocatedEmployeesAsync()
    {
        return await _context.Employees
            .CountAsync(e =>
                e.Status == EmployeeStatus.Allocated);
    }

    public async Task<int> GetBenchEmployeesAsync()
    {
        return await _context.Employees
            .CountAsync(e =>
                e.Status == EmployeeStatus.Bench);
    }

    public async Task<int> GetActiveProjectsAsync()
    {
        return await _context.Projects
            .CountAsync(p =>
                p.Status == ProjectStatus.Active);
    }

    public async Task<int> GetSubmittedTimesheetsAsync()
    {
        return await _context.Timesheets
            .CountAsync(t =>
                t.Status == TimesheetStatus.Submitted);
    }

    public async Task<int> GetMissedTimesheetsAsync()
    {
        return await _context.Timesheets
            .CountAsync(t =>
                t.Status == TimesheetStatus.Missed);
    }

    // ── Resource Dashboard ──

    public async Task<List<Employee>> GetBenchEmployeesWithSkillsAsync()
    {
        return await _context.Employees
            .Where(e => e.IsActive
                && e.Status == EmployeeStatus.Bench)
            .Include(e => e.EmployeeSkills)
                .ThenInclude(es => es.Skill)
            .ToListAsync();
    }

    public async Task<List<Employee>> GetAllocatedEmployeesWithAllocationsAsync()
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Employees
            .Where(e => e.IsActive
                && e.Status == EmployeeStatus.Allocated)
            .Include(e => e.Allocations
                .Where(a => a.ToDate >= today))
            .ToListAsync();
    }

    public async Task<Employee?> GetEmployeeDetailAsync(int employeeId)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Employees
            .Where(e => e.Id == employeeId && e.IsActive)
            .Include(e => e.EmployeeSkills)
                .ThenInclude(es => es.Skill)
            .Include(e => e.Allocations
                .Where(a => a.ToDate >= today))
                .ThenInclude(a => a.Project)
            .FirstOrDefaultAsync();
    }

    public async Task<List<string>> GetRecentActivityTagsAsync(
        int employeeId, int weeks)
    {
        var cutoffDate = DateTime.UtcNow.Date
            .AddDays(-7 * weeks);

        return await _context.TimesheetEntries
            .Where(te => te.Timesheet.EmployeeId == employeeId
                && te.Timesheet.WeekStartDate >= cutoffDate)
            .SelectMany(te => te.ActivityTags)
            .Select(teat => teat.ActivityTag.TagName)
            .Distinct()
            .ToListAsync();
    }

    // ── Utilization Report ──

    public async Task<List<Employee>> GetAllEmployeesWithAllocationsAsync()
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Employees
            .Where(e => e.IsActive)
            .Include(e => e.Allocations
                .Where(a => a.ToDate >= today))
            .ToListAsync();
    }

    // ── Team Timesheets ──

    public async Task<List<Timesheet>> GetTimesheetsByWeekAsync(
        DateTime weekStartDate)
    {
        return await _context.Timesheets
            .Where(t => t.WeekStartDate == weekStartDate)
            .Include(t => t.Employee)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .ToListAsync();
    }

    // ── Skill Matrix ──

    public async Task<List<Employee>> GetAllEmployeesWithSkillsAsync()
    {
        return await _context.Employees
            .Where(e => e.IsActive)
            .Include(e => e.EmployeeSkills)
                .ThenInclude(es => es.Skill)
            .ToListAsync();
    }

    // ── Project Dashboard ──

    public async Task<List<Project>> GetAllProjectsWithDetailsAsync()
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Projects
            .Include(p => p.Manager)
            .Include(p => p.Milestones)
            .Include(p => p.Allocations
                .Where(a => a.ToDate >= today))
            .ToListAsync();
    }
}