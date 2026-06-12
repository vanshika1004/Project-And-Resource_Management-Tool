namespace Application.DTOs.Timesheet;

public class TimesheetEntryDto
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public decimal HoursWorked { get; set; }

    public string ActivityTags { get; set; } = string.Empty;
}