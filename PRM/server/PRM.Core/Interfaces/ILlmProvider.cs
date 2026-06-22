using System.Threading.Tasks;

namespace PRM.Core.Interfaces;

public interface ILlmProvider
{
    Task<string> GetSkillMatchAsync(string requirement, object candidatesSummary);
    Task<string> GetRiskSummaryAsync(object projectData);
}
