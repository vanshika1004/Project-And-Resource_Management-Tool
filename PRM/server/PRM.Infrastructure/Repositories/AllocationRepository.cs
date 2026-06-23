using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRM.Core.Entities;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Data;

namespace PRM.Infrastructure.Repositories;

public class AllocationRepository : IAllocationRepository
{
    private readonly PrmDbContext _context;

    public AllocationRepository(PrmDbContext context)
    {
        _context = context;
    }

    public async Task<Allocation?> GetByIdAsync(int id)
    {
        return await _context.Allocations
            .Include(a => a.Project)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Allocation>> GetAllAsync()
    {
        return await _context.Allocations
            .Include(a => a.Project)
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Allocation>> GetByResourceIdAsync(int resourceId)
    {
        return await _context.Allocations
            .Include(a => a.Project)
            .Include(a => a.User)
            .Where(a => a.UserId == resourceId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Allocation>> GetByProjectIdAsync(int projectId)
    {
        return await _context.Allocations
            .Include(a => a.User)
            .Where(a => a.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Allocation>> GetOverlappingAsync(int resourceId, DateTime fromDate, DateTime toDate)
    {
        return await _context.Allocations
            .Where(a => a.UserId == resourceId &&
                        a.FromDate <= toDate &&
                        a.ToDate >= fromDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Allocation>> GetActiveForWeekAsync(int resourceId, DateTime weekStart)
    {
        var weekEnd = weekStart.AddDays(6);
        return await _context.Allocations
            .Include(a => a.Project)
            .Where(a => a.UserId == resourceId &&
                        a.FromDate <= weekEnd &&
                        a.ToDate >= weekStart)
            .ToListAsync();
    }

    public async Task<IEnumerable<Allocation>> GetActiveByResourceIdsAsync(IEnumerable<int> resourceIds)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Allocations
            .Include(a => a.Project)
            .Where(a => resourceIds.Contains(a.UserId) &&
                        a.FromDate <= today &&
                        a.ToDate >= today)
            .ToListAsync();
    }

    public async Task AddAsync(Allocation allocation)
    {
        await _context.Allocations.AddAsync(allocation);
        await _context.SaveChangesAsync();
    }

    public async Task EndAllocationAsync(int id, DateTime endDate)
    {
        var allocation = await _context.Allocations.FindAsync(id);
        if (allocation != null)
        {
            allocation.ToDate = endDate;
            _context.Allocations.Update(allocation);
            await _context.SaveChangesAsync();
        }
    }
}
