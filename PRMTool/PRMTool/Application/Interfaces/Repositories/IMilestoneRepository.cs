using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IMilestoneRepository
{
    Task<List<Milestone>> GetByProjectIdAsync(int projectId);

    Task<Milestone?> GetByIdAsync(int id);

    Task AddAsync(Milestone milestone);

    void Update(Milestone milestone);

    Task SaveChangesAsync();
}