using System;
using System.Collections.Generic;
using PRM.Core.Enums;

namespace PRM.Core.Entities;

public class Project
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ProjectStatus Status { get; set; }
    public int? ManagerId { get; set; }
    public int TotalStoryPoints { get; set; }
    public HealthStatus HealthStatus { get; set; } = HealthStatus.OnTrack;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public User? Manager { get; set; }
    public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    public ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
    public ICollection<TimesheetEntry> TimesheetEntries { get; set; } = new List<TimesheetEntry>();
}
