namespace Application.DTOs.Allocation;

public class AllocationDto
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public decimal UtilizationPercent { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }
}