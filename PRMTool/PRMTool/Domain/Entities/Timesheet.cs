using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Timesheet : BaseEntity
{
    public int EmployeeId { get; set; }

    public DateTime WeekStartDate { get; set; }

    public decimal TotalHours { get; set; }

    public TimesheetStatus Status { get; set; }

    public DateTime SubmittedDate { get; set; }

    // Navigation Properties

    public Employee Employee { get; set; } = null!;

    public ICollection<TimesheetEntry> Entries { get; set; }
        = new List<TimesheetEntry>();
}