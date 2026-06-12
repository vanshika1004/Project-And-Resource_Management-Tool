using Domain.Enums;

namespace Application.DTOs.Project;

public class UpdateProjectRequestDto
{
    public string ProjectName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public ProjectStatus Status { get; set; }

    public ProjectHealthStatus HealthStatus { get; set; }

    public int ManagerId { get; set; }

    public int TotalStoryPoints { get; set; }
}