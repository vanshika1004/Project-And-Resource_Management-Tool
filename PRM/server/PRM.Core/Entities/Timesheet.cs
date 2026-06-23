using System;
using System.Collections.Generic;
using PRM.Core.Enums;

namespace PRM.Core.Entities;

public class Timesheet
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime WeekStartDate { get; set; }
    public TimesheetStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int ReminderCount { get; set; } = 0;
    
    public User? User { get; set; }
    public ICollection<TimesheetEntry> Entries { get; set; } = new List<TimesheetEntry>();
}
