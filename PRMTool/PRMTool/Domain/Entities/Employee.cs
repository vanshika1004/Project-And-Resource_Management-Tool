using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Employee : BaseEntity
{
    public int UserId { get; set; }

    public int? ManagerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Designation { get; set; } = string.Empty;

    public EmployeeStatus Status { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties

    public User User { get; set; } = null!;

    public Employee? Manager { get; set; }

    public ICollection<Employee> TeamMembers { get; set; }
        = new List<Employee>();

    public ICollection<EmployeeSkill> EmployeeSkills { get; set; }
        = new List<EmployeeSkill>();

    public ICollection<Allocation> Allocations { get; set; }
        = new List<Allocation>();

    public ICollection<Timesheet> Timesheets { get; set; }
        = new List<Timesheet>();
}