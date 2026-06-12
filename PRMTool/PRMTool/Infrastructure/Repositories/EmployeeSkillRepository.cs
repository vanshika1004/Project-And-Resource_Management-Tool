using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EmployeeSkillRepository : IEmployeeSkillRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeSkillRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeSkill?> GetByIdAsync(int id)
    {
        return await _context.EmployeeSkills
            .Include(es => es.Skill)
            .Include(es => es.Employee)
            .FirstOrDefaultAsync(es => es.Id == id);
    }

    public async Task<List<EmployeeSkill>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.EmployeeSkills
            .Include(es => es.Skill)
            .Where(es => es.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task AddAsync(EmployeeSkill employeeSkill)
    {
        await _context.EmployeeSkills.AddAsync(employeeSkill);
    }

    public void Update(EmployeeSkill employeeSkill)
    {
        _context.EmployeeSkills.Update(employeeSkill);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Delete(EmployeeSkill employeeSkill)
    {
        _context.EmployeeSkills.Remove(employeeSkill);
    }
}