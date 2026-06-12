using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    // Existing summary counts
    Task<int> GetTotalEmployeesAsync();

    Task<int> GetAllocatedEmployeesAsync();

    Task<int> GetBenchEmployeesAsync();

    Task<int> GetActiveProjectsAsync();

    Task<int> GetSubmittedTimesheetsAsync();

    Task<int> GetMissedTimesheetsAsync();

    // Resource Dashboard
    Task<List<Employee>> GetBenchEmployeesWithSkillsAsync();

    Task<List<Employee>> GetAllocatedEmployeesWithAllocationsAsync();

    Task<Employee?> GetEmployeeDetailAsync(int employeeId);

    Task<List<string>> GetRecentActivityTagsAsync(
        int employeeId, int weeks);

    // Utilization Report
    Task<List<Employee>> GetAllEmployeesWithAllocationsAsync();

    // Team Timesheets
    Task<List<Timesheet>> GetTimesheetsByWeekAsync(
        DateTime weekStartDate);

    // Skill Matrix
    Task<List<Employee>> GetAllEmployeesWithSkillsAsync();

    // Project Dashboard
    Task<List<Project>> GetAllProjectsWithDetailsAsync();
}