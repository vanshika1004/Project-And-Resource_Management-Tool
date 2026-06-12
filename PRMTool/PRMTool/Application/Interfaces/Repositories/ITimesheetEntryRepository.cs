using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ITimesheetEntryRepository
{
    Task<List<TimesheetEntry>> GetByTimesheetIdAsync(int timesheetId);

    Task<TimesheetEntry?> GetByIdAsync(int id);

    Task AddAsync(TimesheetEntry entry);

    Task SaveChangesAsync();
}