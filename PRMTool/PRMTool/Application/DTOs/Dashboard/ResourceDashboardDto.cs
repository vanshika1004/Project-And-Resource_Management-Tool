namespace Application.DTOs.Dashboard;

public class ResourceDashboardDto
{
    public List<BenchEmployeeDto> BenchEmployees { get; set; } = new();

    public List<ActiveEmployeeDto> ActiveEmployees { get; set; } = new();

    public int BenchCount { get; set; }

    public int PartiallyAllocatedCount { get; set; }
}
