using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IProjectRepository
{
    Task<List<Project>> GetAllAsync();

    Task<Project?> GetByIdAsync(int id);

    Task AddAsync(Project project);

    void Update(Project project);

    Task<Project?> GetProjectWithMilestonesAsync(int projectId);
    Task SaveChangesAsync();
}