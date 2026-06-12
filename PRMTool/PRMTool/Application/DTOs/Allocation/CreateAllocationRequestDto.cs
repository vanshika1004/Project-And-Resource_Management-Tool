namespace Application.DTOs.Allocation;

public class CreateAllocationRequestDto
{
    public int EmployeeId { get; set; }

    public int ProjectId { get; set; }

    public decimal UtilizationPercent { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }
}