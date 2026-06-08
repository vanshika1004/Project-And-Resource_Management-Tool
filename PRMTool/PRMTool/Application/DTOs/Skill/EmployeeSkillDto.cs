namespace Application.DTOs.Skill;

public class EmployeeSkillDto
{
    public int Id { get; set; }

    public string SkillName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string ProficiencyLevel { get; set; } = string.Empty;
}