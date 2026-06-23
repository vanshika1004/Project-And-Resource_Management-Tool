using System.Collections.Generic;
using System.Threading.Tasks;
using PRM.Core.Entities;

namespace PRM.Core.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<Project?> GetByIdWithMilestonesAsync(int id);
    Task<Project?> GetByIdWithDetailsForRiskAsync(int id);
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetByManagerIdAsync(int managerId);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
}
