using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ISkillRepository
{
    Task<List<Skill>> GetAllAsync();

    Task<Skill?> GetByIdAsync(int id);

    Task<Skill?> GetByNameAsync(string skillName);

    Task AddAsync(Skill skill);

    Task SaveChangesAsync();
}