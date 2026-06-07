using Domain.Common;

namespace Domain.Entities;

public class TimesheetEntryActivityTag : BaseEntity
{
    public int TimesheetEntryId { get; set; }

    public int ActivityTagId { get; set; }

    // Navigation Properties

    public TimesheetEntry TimesheetEntry { get; set; } = null!;

    public ActivityTag ActivityTag { get; set; } = null!;
}