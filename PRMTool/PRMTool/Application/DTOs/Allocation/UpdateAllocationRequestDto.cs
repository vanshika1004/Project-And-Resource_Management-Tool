namespace Application.DTOs.Allocation;

public class UpdateAllocationRequestDto
{
    public decimal UtilizationPercent { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }
}