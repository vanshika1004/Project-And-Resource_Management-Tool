using Domain.Enums;

namespace Application.DTOs.Project;

public class UpdateMilestoneRequestDto
{
    public string Title { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public int StoryPoints { get; set; }

    public MilestoneStatus Status { get; set; }
}