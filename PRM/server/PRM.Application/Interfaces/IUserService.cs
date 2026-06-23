using System.Collections.Generic;
using System.Threading.Tasks;
using PRM.Application.DTOs;

namespace PRM.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task UpdateUserAsync(UpdateUserRequest request);
    Task DeactivateUserAsync(int id);
    Task ReactivateUserAsync(int id);
    Task ResetPasswordAsync(int id, string newTemporaryPassword);
    Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(int userId);
    Task AddSkillAsync(int userId, AddSkillRequest request);
    Task UpdateSkillAsync(int userId, int skillId, UpdateSkillRequest request);
    Task RemoveSkillAsync(int userId, int skillId);

    Task<IEnumerable<UserDto>> GetTeamMembersAsync(int managerId);
    Task<IEnumerable<TeamDashboardMemberDto>> GetTeamDashboardAsync(int managerId);
    Task<TeamMemberDetailsDto> GetTeamMemberDetailsAsync(int id);
    Task UnfreezeTimesheetAccessAsync(int userId, int managerId);
}
