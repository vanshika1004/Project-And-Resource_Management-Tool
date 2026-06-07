using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Milestone : BaseEntity
{
    public int ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public int StoryPoints { get; set; }

    public MilestoneStatus Status { get; set; }

    // Navigation Property

    public Project Project { get; set; } = null!;
}