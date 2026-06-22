using PRM.Core.Enums;

namespace PRM.Core.Entities;

public class Skill
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public SkillCategory Category { get; set; }
}
