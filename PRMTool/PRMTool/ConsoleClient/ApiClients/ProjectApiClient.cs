using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleClient.ApiClients;

public class ProjectApiClient : ApiClientBase
{
    public static async Task<List<ProjectDto>> GetAllAsync()
    {
        return await GetAsync<List<ProjectDto>>("projects") ?? new List<ProjectDto>();
    }

    public static async Task<ProjectDto?> GetByIdAsync(int id)
    {
        return await GetAsync<ProjectDto>($"projects/{id}");
    }

    public static async Task<ProjectDto> CreateAsync(CreateProjectDto dto)
    {
        var result = await PostAsync<ProjectDto>("projects", dto);
        return result ?? throw new Exception("Empty response from server.");
    }

    public static async Task UpdateAsync(int id, UpdateProjectDto dto)
    {
        await PutAsync($"projects/{id}", dto);
    }
}

public class ProjectDto
{
    public int Id { get; set; }
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public int TotalStoryPoints { get; set; }
    public int StoryPointsCompleted { get; set; }
}

public class CreateProjectDto
{
    public int ManagerId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalStoryPoints { get; set; }
}

public class UpdateProjectDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public int ManagerId { get; set; }
    public int TotalStoryPoints { get; set; }
}
