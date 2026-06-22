using System;
using PRM.Core.Enums;

namespace PRM.Core.Entities;

public class Milestone
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public required string Title { get; set; }
    public DateTime DueDate { get; set; }
    public int StoryPoints { get; set; }
    public MilestoneStatus Status { get; set; }
    
    public Project? Project { get; set; }
}
