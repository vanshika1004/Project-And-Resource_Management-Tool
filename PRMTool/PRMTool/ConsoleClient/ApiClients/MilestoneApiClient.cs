namespace ConsoleClient.ApiClients;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class MilestoneApiClient : ApiClientBase
{
    public static async Task<List<MilestoneDto>> GetByProjectIdAsync(int projectId)
    {
        return await GetAsync<List<MilestoneDto>>($"Milestones/project/{projectId}") ?? new List<MilestoneDto>();
    }

    public static async Task<MilestoneDto> CreateAsync(CreateMilestoneDto dto)
    {
        var result = await PostAsync<MilestoneDto>($"Milestones", dto);
        return result ?? throw new Exception("Empty response from server.");
    }

    public static async Task UpdateAsync(int milestoneId, UpdateMilestoneDto dto)
    {
        await PutAsync($"milestones/{milestoneId}", dto);
    }
}

public class MilestoneDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int StoryPoints { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateMilestoneDto
{
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int StoryPoints { get; set; }
}

public class UpdateMilestoneDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int StoryPoints { get; set; }
    public string Status { get; set; } = string.Empty;
}
