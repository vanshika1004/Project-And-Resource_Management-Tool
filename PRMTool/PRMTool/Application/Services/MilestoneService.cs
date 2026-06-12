using Application.DTOs.Project;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class MilestoneService : IMilestoneService
{
    private readonly IMilestoneRepository _milestoneRepository;

    private readonly IProjectRepository _projectRepository;

    public MilestoneService(IMilestoneRepository milestoneRepository, IProjectRepository projectRepository)
    {
        _milestoneRepository = milestoneRepository;
        _projectRepository = projectRepository;
    }

    public async Task<int> CreateMilestoneAsync(CreateMilestoneRequestDto request)
    {
        var milestone = new Milestone
        {
            ProjectId = request.ProjectId,
            Title = request.Title,
            DueDate = request.DueDate,
            StoryPoints = request.StoryPoints,
            Status = MilestoneStatus.NotStarted
        };

        await _milestoneRepository.AddAsync(milestone);

        await _milestoneRepository.SaveChangesAsync();

        var project = await _projectRepository.GetByIdAsync(request.ProjectId);

        if (project != null)
        {
            project.TotalStoryPoints += request.StoryPoints;

            _projectRepository.Update(project);

            await _projectRepository.SaveChangesAsync();
        }

        return milestone.Id;
    }

    public async Task<List<MilestoneDto>> GetByProjectIdAsync(int projectId)
    {
        var milestones = await _milestoneRepository.GetByProjectIdAsync(projectId);

        return milestones.Select(m => new MilestoneDto
        {
            Id = m.Id,
            ProjectId = m.ProjectId,
            Title = m.Title,
            DueDate = m.DueDate,
            StoryPoints = m.StoryPoints,
            Status = m.Status.ToString()
        }).ToList();
    }

    public async Task UpdateMilestoneAsync(int milestoneId, UpdateMilestoneRequestDto request)
    {
        var milestone = await _milestoneRepository.GetByIdAsync(milestoneId);

        if (milestone == null)
        {
            throw new Exception("Milestone not found.");
        }

        milestone.Title = request.Title;
        milestone.DueDate = request.DueDate;
        milestone.StoryPoints = request.StoryPoints;
        milestone.Status = request.Status;

        _milestoneRepository.Update(milestone);

        await _milestoneRepository.SaveChangesAsync();
    }
}