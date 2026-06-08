using Application.DTOs.Project;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<int> CreateProjectAsync(CreateProjectRequestDto request)
    {
        var project = new Project
        {
            ManagerId = request.ManagerId,
            ProjectName = request.ProjectName,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,

            Status = ProjectStatus.Planned,
            HealthStatus = ProjectHealthStatus.OnTrack,
            TotalStoryPoints = 0
        };

        await _projectRepository.AddAsync(project);

        await _projectRepository.SaveChangesAsync();

        return project.Id;
    }

    public async Task<List<ProjectDto>> GetAllAsync()
    {
        var projects = await _projectRepository.GetAllAsync();

        return projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            ManagerId = p.ManagerId,
            ProjectName = p.ProjectName,
            Description = p.Description,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Status = p.Status.ToString(),
            HealthStatus = p.HealthStatus.ToString(),
            TotalStoryPoints = p.TotalStoryPoints
        }).ToList();
    }

    public async Task<ProjectDto?> GetByIdAsync(int projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);

        if (project == null)
            return null;

        return new ProjectDto
        {
            Id = project.Id,
            ManagerId = project.ManagerId,
            ProjectName = project.ProjectName,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Status = project.Status.ToString(),
            HealthStatus = project.HealthStatus.ToString(),
            TotalStoryPoints = project.TotalStoryPoints
        };
    }
}