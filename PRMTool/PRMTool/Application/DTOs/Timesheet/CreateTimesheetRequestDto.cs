namespace Application.DTOs.Timesheet;

public class CreateTimesheetRequestDto
{
    public int EmployeeId { get; set; }

    public DateTime WeekStartDate { get; set; }

    public List<CreateTimesheetEntryDto> Entries { get; set; } = new();
}

public class CreateTimesheetEntryDto
{
    public int ProjectId { get; set; }
    public decimal HoursWorked { get; set; }
    public string ActivityTags { get; set; } = string.Empty; // Using string to keep it simple, since tags were comma separated
}