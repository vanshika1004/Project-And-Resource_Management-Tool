using Domain.Common;

namespace Domain.Entities;

public class TimesheetEntry : BaseEntity
{
    public int TimesheetId { get; set; }

    public int ProjectId { get; set; }

    public decimal HoursWorked { get; set; }

    // Navigation Properties

    public Timesheet Timesheet { get; set; } = null!;

    public Project Project { get; set; } = null!;

    public ICollection<TimesheetEntryActivityTag> ActivityTags { get; set; }
        = new List<TimesheetEntryActivityTag>();
}