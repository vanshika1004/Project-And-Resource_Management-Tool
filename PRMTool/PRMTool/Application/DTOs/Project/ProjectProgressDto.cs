namespace Application.DTOs.Project;

public class ProjectProgressDto
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public int TotalStoryPoints { get; set; }

    public int CompletedStoryPoints { get; set; }

    public decimal ProgressPercentage { get; set; }
}