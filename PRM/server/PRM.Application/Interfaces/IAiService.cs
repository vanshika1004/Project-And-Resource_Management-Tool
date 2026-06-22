using System.Threading.Tasks;

namespace PRM.Application.Interfaces;

public interface IAiService
{
    Task<string> GetSkillMatchRecommendationAsync(string requirement, int managerId);
    Task<string> GetProjectRiskSummaryAsync(int projectId, int managerId);
    Task<PRM.Application.DTOs.TeamBuildResultDto> BuildTeamAsync(PRM.Application.DTOs.TeamBuildRequest request, int managerId);
}
