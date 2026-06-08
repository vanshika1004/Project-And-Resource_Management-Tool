namespace Application.DTOs.Skill;

public class CreateSkillRequestDto
{
    public string SkillName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
}