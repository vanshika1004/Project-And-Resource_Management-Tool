namespace Application.DTOs.Project;

public class ProjectDto
{
    public int Id { get; set; }

    public int ManagerId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string HealthStatus { get; set; } = string.Empty;

    public int TotalStoryPoints { get; set; }
}