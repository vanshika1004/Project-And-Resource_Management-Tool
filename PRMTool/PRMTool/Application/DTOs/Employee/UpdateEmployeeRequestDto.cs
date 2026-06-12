using Domain.Enums;

namespace Application.DTOs.Employee;

public class UpdateEmployeeRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Designation { get; set; } = string.Empty;

    public int? ManagerId { get; set; }

    public EmployeeStatus Status { get; set; }

    public bool IsActive { get; set; } = true;
}