namespace Application.DTOs.Dashboard;

public class ProjectMilestoneSummaryDto
{
    public string Title { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public int StoryPoints { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsOverdue { get; set; }
}

public class ProjectDashboardItemDto
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string ManagerName { get; set; } = string.Empty;

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string HealthStatus { get; set; } = string.Empty;

    public int TotalStoryPoints { get; set; }

    public int CompletedStoryPoints { get; set; }

    public decimal ProgressPercentage { get; set; }

    public int ResourceCount { get; set; }

    public List<ProjectMilestoneSummaryDto> Milestones { get; set; } = new();
}

public class ProjectDashboardDto
{
    public List<ProjectDashboardItemDto> Projects { get; set; } = new();
}
