namespace Application.DTOs.Employee;

public class EmployeeDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Designation { get; set; } = string.Empty;

    public int? ManagerId { get; set; }

    public string Status { get; set; } = string.Empty;
}