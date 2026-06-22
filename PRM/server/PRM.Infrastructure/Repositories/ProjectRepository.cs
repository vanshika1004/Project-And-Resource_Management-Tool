using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRM.Core.Entities;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Data;

namespace PRM.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly PrmDbContext _context;

    public ProjectRepository(PrmDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.Manager)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project?> GetByIdWithMilestonesAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.Milestones)
            .Include(p => p.Allocations)
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project?> GetByIdWithDetailsForRiskAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.Milestones)
            .Include(p => p.Allocations)
                .ThenInclude(a => a.User)
            .Include(p => p.TimesheetEntries)
                .ThenInclude(t => t.Timesheet)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _context.Projects
            .Include(p => p.Manager)
            .Include(p => p.Milestones)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByManagerIdAsync(int managerId)
    {
        return await _context.Projects
            .Include(p => p.Milestones)
            .Where(p => p.ManagerId == managerId)
            .ToListAsync();
    }

    public async Task AddAsync(Project project)
    {
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }
}
