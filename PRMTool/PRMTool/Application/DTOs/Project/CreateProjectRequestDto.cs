namespace Application.DTOs.Project;

public class CreateProjectRequestDto
{
    public int ManagerId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}