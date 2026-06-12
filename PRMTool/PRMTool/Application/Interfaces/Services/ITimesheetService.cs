using Application.DTOs.Timesheet;

namespace Application.Interfaces.Services;

public interface ITimesheetService
{
    Task<int> CreateTimesheetAsync(CreateTimesheetRequestDto request);

    Task<List<TimesheetDto>> GetAllAsync();

    Task<TimesheetDto?> GetByIdAsync(int id);
    
    Task<List<TimesheetDto>> GetByEmployeeIdAsync(int employeeId);
    Task<List<TimesheetDto>> GetTeamTimesheetsAsync(int managerId);
}