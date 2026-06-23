using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRM.Core.Entities;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Data;

namespace PRM.Infrastructure.Repositories;

public class TimesheetRepository : ITimesheetRepository
{
    private readonly PrmDbContext _context;

    public TimesheetRepository(PrmDbContext context)
    {
        _context = context;
    }

    public async Task<Timesheet?> GetByResourceAndWeekAsync(int resourceId, DateTime weekStartDate)
    {
        return await _context.Timesheets
            .Include(t => t.Entries)
            .FirstOrDefaultAsync(t => t.UserId == resourceId && t.WeekStartDate == weekStartDate);
    }

    public async Task<IEnumerable<Timesheet>> GetByResourceIdAsync(int resourceId)
    {
        return await _context.Timesheets
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Where(t => t.UserId == resourceId)
            .OrderByDescending(t => t.WeekStartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Timesheet>> GetTeamTimesheetsAsync(int managerId, DateTime weekStartDate)
    {
        return await _context.Timesheets
            .Include(t => t.User)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Where(t => t.User!.ManagerId == managerId && t.WeekStartDate == weekStartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Timesheet>> GetRecentByProjectAsync(int projectId, int weeks = 4)
    {
        var cutoff = DateTime.UtcNow.Date.AddDays(-7 * weeks);
        return await _context.Timesheets
            .Include(t => t.Entries)
            .Where(t => t.WeekStartDate >= cutoff && t.Entries.Any(e => e.ProjectId == projectId))
            .ToListAsync();
    }

    public async Task AddAsync(Timesheet timesheet)
    {
        await _context.Timesheets.AddAsync(timesheet);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Timesheet timesheet)
    {
        _context.Timesheets.Update(timesheet);
        await _context.SaveChangesAsync();
    }
}
