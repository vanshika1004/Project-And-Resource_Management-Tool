using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id);

    Task<Employee?> GetByUserIdAsync(int userId);

    Task AddAsync(Employee employee);

    void Update(Employee employee);

    Task SaveChangesAsync();
}