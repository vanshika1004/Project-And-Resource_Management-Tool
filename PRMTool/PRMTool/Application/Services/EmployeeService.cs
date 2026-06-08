using Application.DTOs.Employee;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(
        IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<List<EmployeeDto>> GetAllAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();

        return employees.Select(e => new EmployeeDto
        {
            Id = e.Id,
            FullName = e.FullName,
            Department = e.Department,
            Designation = e.Designation,
            ManagerId = e.ManagerId,
            Status = e.Status.ToString()
        }).ToList();
    }

    public async Task<EmployeeDto?> GetByIdAsync(
        int employeeId)
    {
        var employee =
            await _employeeRepository.GetByIdAsync(employeeId);

        if (employee == null)
            return null;

        return new EmployeeDto
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Department = employee.Department,
            Designation = employee.Designation,
            ManagerId = employee.ManagerId,
            Status = employee.Status.ToString()
        };
    }

    public async Task UpdateAsync(
        int employeeId,
        UpdateEmployeeRequestDto request)
    {
        var employee =
            await _employeeRepository.GetByIdAsync(employeeId);

        if (employee == null)
        {
            throw new Exception("Employee not found.");
        }

        employee.Department = request.Department;
        employee.Designation = request.Designation;
        employee.ManagerId = request.ManagerId;
        employee.Status = request.Status;

        _employeeRepository.Update(employee);

        await _employeeRepository.SaveChangesAsync();
    }
}