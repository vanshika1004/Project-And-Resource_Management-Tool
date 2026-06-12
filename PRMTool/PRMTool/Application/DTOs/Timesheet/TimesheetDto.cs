namespace Application.DTOs.Timesheet;

public class TimesheetDto
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public DateTime WeekStartDate { get; set; }

    public decimal TotalHours { get; set; }

    public string Status { get; set; } = string.Empty;
    
    public List<TimesheetEntryDto> Entries { get; set; } = new();
}