namespace Application.DTOs.Dashboard;

public class ActiveEmployeeDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public decimal TotalAllocationPercent { get; set; }

    public string Availability { get; set; } = string.Empty;
}
