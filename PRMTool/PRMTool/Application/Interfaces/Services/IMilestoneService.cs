using Application.DTOs.Project;

namespace Application.Interfaces.Services;

public interface IMilestoneService
{
    Task<int> CreateMilestoneAsync(CreateMilestoneRequestDto request);

    Task<List<MilestoneDto>> GetByProjectIdAsync(int projectId);

    Task UpdateMilestoneAsync(int milestoneId, UpdateMilestoneRequestDto request);
}