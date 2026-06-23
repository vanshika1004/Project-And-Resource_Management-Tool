using PRM.Core.Enums;

namespace PRM.Core.Entities;

public class UserSkill
{
    public int UserId { get; set; }
    public int SkillId { get; set; }
    public ProficiencyLevel ProficiencyLevel { get; set; }
    
    public User? User { get; set; }
    public Skill? Skill { get; set; }
}
