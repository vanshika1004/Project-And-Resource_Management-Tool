using Domain.Enums;

namespace Application.DTOs.Skill;

public class AssignEmployeeSkillRequestDto
{
    public int EmployeeId { get; set; }

    public int SkillId { get; set; }

    public ProficiencyLevel ProficiencyLevel { get; set; }
}