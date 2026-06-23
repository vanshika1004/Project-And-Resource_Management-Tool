using System;
using System.Collections.Generic;

namespace PRM.Core.Entities;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public int RoleId { get; set; }
    public required string FullName { get; set; }
    public string? Department { get; set; }
    public int? ManagerId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ForcePasswordChange { get; set; } = true;
    public bool IsTimesheetFrozen { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public Role? Role { get; set; }
    public User? Manager { get; set; }
    public ICollection<User> DirectReports { get; set; } = new List<User>();
    public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    public ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
    public ICollection<Timesheet> Timesheets { get; set; } = new List<Timesheet>();
}
