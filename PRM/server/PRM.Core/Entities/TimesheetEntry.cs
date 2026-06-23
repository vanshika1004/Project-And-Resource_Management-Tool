namespace PRM.Core.Entities;

public class TimesheetEntry
{
    public int Id { get; set; }
    public int TimesheetId { get; set; }
    public int ProjectId { get; set; }
    public decimal HoursWorked { get; set; }
    public string? ActivityTags { get; set; }
    
    public Timesheet? Timesheet { get; set; }
    public Project? Project { get; set; }
}
