namespace Application.DTOs.Dashboard;

public class BenchEmployeeDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public List<string> Skills { get; set; } = new();
}
