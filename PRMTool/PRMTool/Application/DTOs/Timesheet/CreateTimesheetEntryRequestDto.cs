namespace Application.DTOs.Timesheet;

public class CreateTimesheetEntryRequestDto
{
    public int TimesheetId { get; set; }

    public int ProjectId { get; set; }

    public decimal HoursWorked { get; set; }
}