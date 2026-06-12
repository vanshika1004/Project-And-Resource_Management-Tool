using Application.DTOs.Project;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public ProjectService(IProjectRepository projectRepository, IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<int> CreateProjectAsync(CreateProjectRequestDto request)
    {
        var manager = await _userRepository.GetByIdAsync(request.ManagerId);
        if (manager == null || manager.Role != UserRole.Manager)
        {
            throw new Exception("Assigned user must be a Manager.");
        }

        var project = new Project
        {
            ManagerId = request.ManagerId,
            ProjectName = request.ProjectName,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,

            Status = ProjectStatus.Planned,
            HealthStatus = ProjectHealthStatus.OnTrack,
            TotalStoryPoints = request.TotalStoryPoints
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
            ManagerName = p.Manager != null ? p.Manager.FullName : string.Empty,
            ProjectName = p.ProjectName,
            Description = p.Description,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Status = p.Status.ToString(),
            HealthStatus = p.HealthStatus.ToString(),
            TotalStoryPoints = p.TotalStoryPoints,
            StoryPointsCompleted = p.Milestones.Where(m => m.Status == MilestoneStatus.Done).Sum(m => m.StoryPoints)
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
            ManagerName = project.Manager != null ? project.Manager.FullName : string.Empty,
            ProjectName = project.ProjectName,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Status = project.Status.ToString(),
            HealthStatus = project.HealthStatus.ToString(),
            TotalStoryPoints = project.TotalStoryPoints,
            StoryPointsCompleted = project.Milestones.Where(m => m.Status == MilestoneStatus.Done).Sum(m => m.StoryPoints)
        };
    }

    public async Task UpdateProjectAsync(int projectId, UpdateProjectRequestDto request)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);

        if (project == null)
        {
            throw new Exception("Project not found.");
        }

        var manager = await _userRepository.GetByIdAsync(request.ManagerId);
        if (manager == null || manager.Role != UserRole.Manager)
        {
            throw new Exception("Assigned user must be a Manager.");
        }

        project.ProjectName = request.ProjectName;
        project.Description = request.Description;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.Status = request.Status;
        project.HealthStatus = request.HealthStatus;
        project.ManagerId = request.ManagerId;
        project.TotalStoryPoints = request.TotalStoryPoints;

        _projectRepository.Update(project);

        await _projectRepository.SaveChangesAsync();
    }

    public async Task<ProjectProgressDto> GetProjectProgressAsync(int projectId)
    {
        var project = await _projectRepository.GetProjectWithMilestonesAsync(projectId);

        if (project == null)
        {
            throw new Exception("Project not found.");
        }

        var completedStoryPoints = project.Milestones.Where(m => m.Status == MilestoneStatus.Done)
                   .Sum(m => m.StoryPoints);

        var progressPercentage = 0m;

        if (project.TotalStoryPoints > 0)
        {
            progressPercentage = (decimal)completedStoryPoints / project.TotalStoryPoints * 100;
        }

        return new ProjectProgressDto
        {
            ProjectId = project.Id,
            ProjectName = project.ProjectName,
            TotalStoryPoints = project.TotalStoryPoints,
            CompletedStoryPoints = completedStoryPoints,
            ProgressPercentage = Math.Round(progressPercentage, 2)
        };
    }
}