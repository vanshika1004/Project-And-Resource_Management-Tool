using Application.DTOs.Skill;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services;

public class EmployeeSkillService : IEmployeeSkillService
{
    private readonly IEmployeeSkillRepository _employeeSkillRepository;

    public EmployeeSkillService(IEmployeeSkillRepository employeeSkillRepository)
    {
        _employeeSkillRepository = employeeSkillRepository;
    }

    public async Task<int> AssignSkillAsync(AssignEmployeeSkillRequestDto request)
    {
        var employeeSkill = new EmployeeSkill
        {
            EmployeeId = request.EmployeeId,
            SkillId = request.SkillId,
            ProficiencyLevel = request.ProficiencyLevel
        };

        await _employeeSkillRepository.AddAsync(employeeSkill);

        await _employeeSkillRepository.SaveChangesAsync();

        return employeeSkill.Id;
    }

    public async Task<List<EmployeeSkillDto>> GetEmployeeSkillsAsync(int employeeId)
    {
        var employeeSkills = await _employeeSkillRepository.GetByEmployeeIdAsync(employeeId);

        return employeeSkills.Select(es =>
            new EmployeeSkillDto
            {
                Id = es.Id,
                SkillName = es.Skill.SkillName,
                Category = es.Skill.Category,
                ProficiencyLevel = es.ProficiencyLevel.ToString()
            }).ToList();
    }

    public async Task UpdateProficiencyAsync(int employeeSkillId, UpdateEmployeeSkillRequestDto request)
    {
        var employeeSkill = await _employeeSkillRepository.GetByIdAsync(employeeSkillId);

        if (employeeSkill == null)
        {
            throw new Exception("Employee skill not found.");
        }

        employeeSkill.ProficiencyLevel = request.ProficiencyLevel;

        _employeeSkillRepository.Update(employeeSkill);

        await _employeeSkillRepository.SaveChangesAsync();
    }
}