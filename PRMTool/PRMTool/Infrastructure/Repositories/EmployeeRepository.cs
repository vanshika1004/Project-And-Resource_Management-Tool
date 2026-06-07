using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Employee?> GetByUserIdAsync(int userId)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task AddAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
    }

    public void Update(Employee employee)
    {
        _context.Employees.Update(employee);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}