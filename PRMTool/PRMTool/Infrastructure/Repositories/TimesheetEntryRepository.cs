using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TimesheetEntryRepository : ITimesheetEntryRepository
{
    private readonly ApplicationDbContext _context;

    public TimesheetEntryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TimesheetEntry>> GetByTimesheetIdAsync(int timesheetId)
    {
        return await _context.TimesheetEntries
            .Include(e => e.Project)
            .Where(e => e.TimesheetId == timesheetId)
            .ToListAsync();
    }

    public async Task<TimesheetEntry?> GetByIdAsync(int id)
    {
        return await _context.TimesheetEntries
            .Include(e => e.Project)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task AddAsync(TimesheetEntry entry)
    {
        await _context.TimesheetEntries.AddAsync(entry);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}