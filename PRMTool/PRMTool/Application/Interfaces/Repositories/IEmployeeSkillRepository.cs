using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IEmployeeSkillRepository
{
    Task<EmployeeSkill?> GetByIdAsync(int id);

    Task<List<EmployeeSkill>> GetByEmployeeIdAsync(int employeeId);

    Task AddAsync(EmployeeSkill employeeSkill);

    void Update(EmployeeSkill employeeSkill);

    void Delete(EmployeeSkill employeeSkill);

    Task SaveChangesAsync();
}