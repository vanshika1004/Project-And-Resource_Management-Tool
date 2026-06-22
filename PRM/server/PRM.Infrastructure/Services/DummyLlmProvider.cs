using System.Threading.Tasks;
using PRM.Core.Interfaces;

namespace PRM.Infrastructure.Services;

public class DummyLlmProvider : ILlmProvider
{
    public Task<string> GetSkillMatchAsync(string requirement, object candidatesSummary)
    {
        return Task.FromResult("AI is disabled. Please provide Gemini or Groq API keys.");
    }

    public Task<string> GetRiskSummaryAsync(object projectData)
    {
        return Task.FromResult("AI is disabled. Please provide Gemini or Groq API keys.");
    }
}
