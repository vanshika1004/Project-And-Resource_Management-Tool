using Domain.Enums;

namespace Application.DTOs.Skill;

public class UpdateEmployeeSkillRequestDto
{
    public ProficiencyLevel ProficiencyLevel { get; set; }
}