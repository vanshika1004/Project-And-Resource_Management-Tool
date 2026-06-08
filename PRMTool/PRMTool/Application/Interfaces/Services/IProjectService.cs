using Application.DTOs.Project;

namespace Application.Interfaces.Services;

public interface IProjectService
{
    Task<int> CreateProjectAsync(CreateProjectRequestDto request);

    Task<List<ProjectDto>> GetAllAsync();

    Task<ProjectDto?> GetByIdAsync(int projectId);
}