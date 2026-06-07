using Domain.Common;

namespace Domain.Entities;

public class Skill : BaseEntity
{
    public string SkillName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public ICollection<EmployeeSkill> EmployeeSkills { get; set; }
        = new List<EmployeeSkill>();
}