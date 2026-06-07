using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Project : BaseEntity
{
    public int ManagerId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public ProjectStatus Status { get; set; }

    public ProjectHealthStatus HealthStatus { get; set; }

    public int TotalStoryPoints { get; set; }

    // Navigation Properties

    public User Manager { get; set; } = null!;

    public ICollection<Milestone> Milestones { get; set; }
        = new List<Milestone>();

    public ICollection<Allocation> Allocations { get; set; }
        = new List<Allocation>();

    public ICollection<TimesheetEntry> TimesheetEntries { get; set; }
        = new List<TimesheetEntry>();
}