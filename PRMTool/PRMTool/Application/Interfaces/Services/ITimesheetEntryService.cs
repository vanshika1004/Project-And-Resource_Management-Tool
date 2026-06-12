using Application.DTOs.Timesheet;

namespace Application.Interfaces.Services;

public interface ITimesheetEntryService
{
    Task<int> CreateEntryAsync(CreateTimesheetEntryRequestDto request);

    Task<List<TimesheetEntryDto>> GetByTimesheetIdAsync(int timesheetId);
}