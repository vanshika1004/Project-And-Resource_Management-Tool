using Domain.Common;

namespace Domain.Entities;

public class Allocation : BaseEntity
{
    public int EmployeeId { get; set; }

    public int ProjectId { get; set; }

    public decimal UtilizationPercent { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    // Navigation Properties

    public Employee Employee { get; set; } = null!;

    public Project Project { get; set; } = null!;
}