using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleClient.ApiClients;

public class AIApiClient : ApiClientBase
{
    public static async Task<SkillMatchResultDto?> GetSkillMatchAsync(string requirement, int projectId)
    {
        var request = new SkillMatchRequestDto
        {
            Requirement = requirement,
            ProjectId = projectId
        };

        var response = await PostAsync<SkillMatchResultDto>("AI/skill-match", request);
        return response;
    }

    public static async Task<string?> GetRiskSummaryAsync(int projectId)
    {
        var request = new RiskSummaryRequestDto { ProjectId = projectId };
        var result = await PostAsync<RiskSummaryResponseDto>("AI/risk-summary", request);
        return result?.Summary;
    }
}

public class RiskSummaryRequestDto
{
    public int ProjectId { get; set; }
}

public class RiskSummaryResponseDto
{
    public string Summary { get; set; } = string.Empty;
}

public class SkillMatchRequestDto
{
    public string Requirement { get; set; } = string.Empty;
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
