using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class EmployeeSkill : BaseEntity
{
    public int EmployeeId { get; set; }

    public int SkillId { get; set; }

    public ProficiencyLevel ProficiencyLevel { get; set; }

    // Navigation Properties

    public Employee Employee { get; set; } = null!;

    public Skill Skill { get; set; } = null!;
}