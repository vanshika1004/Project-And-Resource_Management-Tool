using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ITimesheetRepository
{
    Task<List<Timesheet>> GetAllAsync();

    Task<Timesheet?> GetByIdAsync(int id);

    Task<List<Timesheet>>
        GetByEmployeeIdAsync(int employeeId);

    Task AddAsync(Timesheet timesheet);

    void Update(Timesheet timesheet);

    Task SaveChangesAsync();

    Task<List<ActivityTag>> GetOrCreateActivityTagsAsync(IEnumerable<string> tagNames);
}