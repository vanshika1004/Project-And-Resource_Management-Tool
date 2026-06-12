namespace Application.DTOs.Dashboard;

public class SkillDetailDto
{
    public string SkillName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string ProficiencyLevel { get; set; } = string.Empty;
}

public class EmployeeSkillMatrixDto
{
    public int EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public List<SkillDetailDto> Skills { get; set; } = new();
}

public class SkillMatrixReportDto
{
    public List<EmployeeSkillMatrixDto> Employees { get; set; } = new();
}
