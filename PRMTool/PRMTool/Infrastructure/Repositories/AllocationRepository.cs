using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AllocationRepository : IAllocationRepository
{
    private readonly ApplicationDbContext _context;

    public AllocationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Allocation>> GetAllAsync()
    {
        return await _context.Allocations
            .Include(a => a.Employee)
            .Include(a => a.Project)
            .ToListAsync();
    }

    public async Task<Allocation?> GetByIdAsync(int id)
    {
        return await _context.Allocations
            .Include(a => a.Employee)
            .Include(a => a.Project)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Allocation>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.Allocations
            .Include(a => a.Project)
            .Where(a => a.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<List<Allocation>> GetByProjectIdAsync(int projectId)
    {
        return await _context.Allocations
            .Include(a => a.Employee)
            .Where(a => a.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task AddAsync(Allocation allocation)
    {
        await _context.Allocations
            .AddAsync(allocation);
    }

    public void Update(Allocation allocation)
    {
        _context.Allocations.Update(allocation);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}