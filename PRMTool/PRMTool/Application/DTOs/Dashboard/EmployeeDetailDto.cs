namespace Application.DTOs.Dashboard;

public class EmployeeAllocationDto
{
    public string ProjectName { get; set; } = string.Empty;

    public decimal UtilizationPercent { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }
}

public class EmployeeDetailDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal TotalAllocationPercent { get; set; }

    public List<string> ProfileSkills { get; set; } = new();

    public List<EmployeeAllocationDto> ActiveAllocations { get; set; } = new();

    public List<string> RecentActivityTags { get; set; } = new();
}
