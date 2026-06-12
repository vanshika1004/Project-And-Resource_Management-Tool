namespace Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalEmployees { get; set; }

    public int AllocatedEmployees { get; set; }

    public int BenchEmployees { get; set; }

    public int ActiveProjects { get; set; }

    public int SubmittedTimesheets { get; set; }

    public int MissedTimesheets { get; set; }
}