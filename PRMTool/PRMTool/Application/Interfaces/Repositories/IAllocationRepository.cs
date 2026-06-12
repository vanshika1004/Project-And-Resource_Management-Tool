using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IAllocationRepository
{
    Task<List<Allocation>> GetAllAsync();

    Task<Allocation?> GetByIdAsync(int id);

    Task<List<Allocation>> GetByEmployeeIdAsync(int employeeId);

    Task<List<Allocation>> GetByProjectIdAsync(int projectId);

    Task AddAsync(Allocation allocation);

    void Update(Allocation allocation);

    Task SaveChangesAsync();
}