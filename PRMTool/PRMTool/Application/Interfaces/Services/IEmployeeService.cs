using Application.DTOs.Employee;

namespace Application.Interfaces.Services;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync();

    Task<EmployeeDto?> GetByIdAsync(int employeeId);

    Task UpdateAsync(int employeeId, UpdateEmployeeRequestDto request);
}