using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MilestoneRepository : IMilestoneRepository
{
    private readonly ApplicationDbContext _context;

    public MilestoneRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Milestone>> GetByProjectIdAsync(int projectId)
    {
        return await _context.Milestones.Where(m => m.ProjectId == projectId).ToListAsync();
    }

    public async Task<Milestone?> GetByIdAsync(int id)
    {
        return await _context.Milestones.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddAsync(Milestone milestone)
    {
        await _context.Milestones.AddAsync(milestone);
    }

    public void Update(Milestone milestone)
    {
        _context.Milestones.Update(milestone);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}