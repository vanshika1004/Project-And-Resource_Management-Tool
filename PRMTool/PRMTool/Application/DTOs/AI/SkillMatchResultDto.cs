namespace Application.DTOs.AI;

public class SkillMatchRequestDto
{
    public string Requirement { get; set; } = string.Empty;
    public int ProjectId { get; set; }
}

public class RiskSummaryRequestDto
{
    public int ProjectId { get; set; }
}



public class SkillMatchCandidateDto
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public string RecentActivity { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int SuggestedAllocationPercent { get; set; }
    public int FreeHours { get; set; }
}

public class SkillMatchResultDto
{
    public List<SkillMatchCandidateDto> Candidates { get; set; } = new();
    public string Note { get; set; } = "These are AI-generated suggestions. Always verify availability and skills with the employee before allocating.";
}
