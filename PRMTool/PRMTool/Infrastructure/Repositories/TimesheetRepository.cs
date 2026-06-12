using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TimesheetRepository
    : ITimesheetRepository
{
    private readonly ApplicationDbContext _context;

    public TimesheetRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Timesheet>> GetAllAsync()
    {
        return await _context.Timesheets
            .Include(t => t.Employee)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Include(t => t.Entries)
                .ThenInclude(e => e.ActivityTags)
                    .ThenInclude(at => at.ActivityTag)
            .ToListAsync();
    }

    public async Task<Timesheet?> GetByIdAsync(int id)
    {
        return await _context.Timesheets
            .Include(t => t.Employee)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Include(t => t.Entries)
                .ThenInclude(e => e.ActivityTags)
                    .ThenInclude(at => at.ActivityTag)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Timesheet>>
        GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.Timesheets
            .Where(t => t.EmployeeId == employeeId)
            .Include(t => t.Employee)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Include(t => t.Entries)
                .ThenInclude(e => e.ActivityTags)
                    .ThenInclude(at => at.ActivityTag)
            .ToListAsync();
    }

    public async Task AddAsync(Timesheet timesheet)
    {
        await _context.Timesheets
            .AddAsync(timesheet);
    }

    public void Update(Timesheet timesheet)
    {
        _context.Timesheets.Update(timesheet);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<ActivityTag>> GetOrCreateActivityTagsAsync(IEnumerable<string> tagNames)
    {
        var tags = new List<ActivityTag>();
        foreach (var name in tagNames)
        {
            var trimmedName = name.Trim();
            if (string.IsNullOrEmpty(trimmedName)) continue;

            var existingTag = await _context.ActivityTags.FirstOrDefaultAsync(t => t.TagName == trimmedName);
            if (existingTag != null)
            {
                tags.Add(existingTag);
            }
            else
            {
                var newTag = new ActivityTag { TagName = trimmedName };
                _context.ActivityTags.Add(newTag);
                tags.Add(newTag);
            }
        }
        return tags;
    }
}