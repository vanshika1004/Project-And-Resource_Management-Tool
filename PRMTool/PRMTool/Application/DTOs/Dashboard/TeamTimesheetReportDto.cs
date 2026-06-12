namespace Application.DTOs.Dashboard;

public class TeamTimesheetEntryDto
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public decimal HoursWorked { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class TeamTimesheetReportDto
{
    public DateTime WeekStartDate { get; set; }

    public List<TeamTimesheetEntryDto> Entries { get; set; } = new();
}
