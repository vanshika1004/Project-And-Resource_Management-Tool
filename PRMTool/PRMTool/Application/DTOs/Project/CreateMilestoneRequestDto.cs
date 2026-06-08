namespace Application.DTOs.Project;

public class CreateMilestoneRequestDto
{
    public int ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public int StoryPoints { get; set; }
}