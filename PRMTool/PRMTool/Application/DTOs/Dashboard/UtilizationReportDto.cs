namespace Application.DTOs.Dashboard;

public class EmployeeUtilizationDto
{
    public int EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public decimal UtilizationPercent { get; set; }

    public string AllocationStatus { get; set; } = string.Empty;
}

public class UtilizationReportDto
{
    public List<EmployeeUtilizationDto> Employees { get; set; } = new();
}
