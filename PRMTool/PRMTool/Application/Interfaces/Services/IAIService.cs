using Application.DTOs.AI;

namespace Application.Interfaces.Services;

public interface IAIService
{
    Task<SkillMatchResultDto> GetSkillMatchAsync(string requirement, int projectId, int managerId);
    Task<string> GetRiskSummaryAsync(int projectId, int managerId);
}
