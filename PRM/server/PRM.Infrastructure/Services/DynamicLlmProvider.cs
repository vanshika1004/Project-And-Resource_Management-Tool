using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Data;

namespace PRM.Infrastructure.Services;

public class DynamicLlmProvider : ILlmProvider
{
    private readonly PrmDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;

    public DynamicLlmProvider(PrmDbContext dbContext, IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
    }

    private async Task<string> GenerateTextAsync(string prompt)
    {
        // Use AsNoTracking to always get fresh values from the DB
        var providerConfig = await _dbContext.SystemConfigs.AsNoTracking().FirstOrDefaultAsync(c => c.Key == "LlmProvider");
        var keyConfig = await _dbContext.SystemConfigs.AsNoTracking().FirstOrDefaultAsync(c => c.Key == "LlmApiKey");
        var urlConfig = await _dbContext.SystemConfigs.AsNoTracking().FirstOrDefaultAsync(c => c.Key == "LlmEndpointUrl");
        var modelConfig = await _dbContext.SystemConfigs.AsNoTracking().FirstOrDefaultAsync(c => c.Key == "LlmModel");

        string provider = providerConfig?.Value?.Trim().ToLower() ?? "groq";
        string apiKey = keyConfig?.Value?.Trim() ?? "";
        string endpointUrl = urlConfig?.Value?.Trim() ?? "";
        string model = modelConfig?.Value?.Trim() ?? "gemma";

        // Use named LlmClient with extended timeout (5 min for slow model inference)
        var httpClient = _httpClientFactory.CreateClient("LlmClient");

        if (provider == "gemini")
        {
            var gemini = new GeminiLlmProvider(httpClient, apiKey);
            return await gemini.GenerateTextAsync(prompt);
        }
        else if (provider == "custom" || provider == "ollama")
        {
            if (string.IsNullOrWhiteSpace(endpointUrl))
            {
                return $"AI unavailable: Provider is set to '{provider}' but no Custom Endpoint URL is configured. Please set it in System Configuration.";
            }
            var custom = new CustomLlmProvider(httpClient, apiKey, endpointUrl, model);
            return await custom.GenerateTextAsync(prompt);
        }
        else
        {
            // Default to Groq
            var groq = new GroqLlmProvider(httpClient, apiKey);
            return await groq.GenerateTextAsync(prompt);
        }
    }

    public async Task<string> GetSkillMatchAsync(string requirement, object candidatesSummary)
    {
        string jsonCandidates = JsonSerializer.Serialize(candidatesSummary, new JsonSerializerOptions { WriteIndented = true });
        
        string prompt = $@"You are an AI HR assistant for a Project & Resource Management tool.

TASK: Rank the candidates below for the following project requirement. Return a short, numbered ranked list.

REQUIREMENT: {requirement}

CANDIDATE DATA (JSON):
{jsonCandidates}

INSTRUCTIONS:
1. Match candidates by comparing ProfileSkills AND RecentActivityTags against the requirement.
2. A candidate whose ProfileSkills contain the required technology is a strong match.
3. RecentActivityTags from timesheets provide real evidence of recent work — weigh these heavily.
4. Prefer candidates who are ON BENCH (100% free) over partially allocated ones.
5. For each candidate, write 1-2 sentences explaining why they match (mention specific skills and availability).
6. If the requirement mentions hours per week, note the candidate's FreeHoursPerWeek.
7. Format: numbered list, each entry is: Name (Id: X) — Reason.
8. Do NOT include candidates who have zero relevant skills.
9. Keep the response concise — no more than 5 candidates.";

        return await GenerateTextAsync(prompt);
    }

    public async Task<string> GetRiskSummaryAsync(object projectData)
    {
        string jsonProject = JsonSerializer.Serialize(projectData, new JsonSerializerOptions { WriteIndented = true });
        
        string prompt = $@"
You are an expert AI Project Manager. Analyze the following JSON data of a project and provide a concise, plain English risk summary.
Identify if any milestones are overdue, and if the project appears on track or at risk. Do not output raw JSON, just a conversational summary paragraph.

Project Data:
{jsonProject}
";
        return await GenerateTextAsync(prompt);
    }
}
