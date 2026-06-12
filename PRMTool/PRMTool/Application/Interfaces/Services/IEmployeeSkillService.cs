using Application.DTOs.Skill;

namespace Application.Interfaces.Services;

public interface IEmployeeSkillService
{
    Task<int> AssignSkillAsync(AssignEmployeeSkillRequestDto request);

    Task<List<EmployeeSkillDto>> GetEmployeeSkillsAsync(int employeeId);

    Task UpdateProficiencyAsync(int employeeSkillId, UpdateEmployeeSkillRequestDto request);

    Task RemoveSkillAsync(int employeeSkillId);
}