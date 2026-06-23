using System.Collections.Generic;
using System.Threading.Tasks;
using PRM.Application.DTOs;

namespace PRM.Application.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
    Task<IEnumerable<ProjectDto>> GetMyProjectsAsync(int managerId);
    Task<ProjectDto> GetProjectByIdAsync(int id);
    Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request);
    Task UpdateProjectAsync(UpdateProjectRequest request);
    Task<MilestoneDto> AddMilestoneAsync(int projectId, CreateMilestoneRequest request);
    Task UpdateMilestoneStatusAsync(int projectId, int milestoneId, PRM.Core.Enums.MilestoneStatus status);
    Task<List<string>> GetProjectRiskFlagsAsync(int projectId);
}
