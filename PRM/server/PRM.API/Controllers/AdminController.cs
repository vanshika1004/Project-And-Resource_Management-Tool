using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM.Application.DTOs;
using PRM.Application.Interfaces;
using System.Threading.Tasks;

namespace PRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IProjectService _projectService;

    public AdminController(IUserService userService, IProjectService projectService)
    {
        _userService = userService;
        _projectService = projectService;
    }

    [HttpPost("users")]
    [Authorize(Policy = "Users.Manage")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        return Ok(user);
    }

    [HttpGet("users")]
    [Authorize(Policy = "Users.Manage")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{id}")]
    [Authorize(Policy = "Users.Manage")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpPut("users/{id}")]
    [Authorize(Policy = "Users.Manage")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        if (id != request.Id) return BadRequest("ID mismatch");
        await _userService.UpdateUserAsync(request);
        return Ok();
    }

    [HttpPut("users/{id}/deactivate")]
    [Authorize(Policy = "Users.Manage")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        await _userService.DeactivateUserAsync(id);
        return Ok();
    }

    [HttpPut("users/{id}/reactivate")]
    [Authorize(Policy = "Users.Manage")]
    public async Task<IActionResult> ReactivateUser(int id)
    {
        await _userService.ReactivateUserAsync(id);
        return Ok();
    }

    [HttpPost("users/{id}/reset-password")]
    [Authorize(Policy = "Users.Manage")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
    {
        await _userService.ResetPasswordAsync(id, request.NewTemporaryPassword);
        return Ok();
    }

    [HttpPost("users/{id}/skills")]
    [Authorize(Policy = "Resources.Manage")]
    public async Task<IActionResult> AddSkill(int id, [FromBody] AddSkillRequest request)
    {
        await _userService.AddSkillAsync(id, request);
        return Ok();
    }

    [HttpGet("users/{id}/skills")]
    [Authorize(Policy = "Resources.Manage")]
    public async Task<IActionResult> GetSkills(int id)
    {
        var skills = await _userService.GetUserSkillsAsync(id);
        return Ok(skills);
    }

    [HttpPut("users/{id}/skills/{skillId}")]
    [Authorize(Policy = "Resources.Manage")]
    public async Task<IActionResult> UpdateSkill(int id, int skillId, [FromBody] UpdateSkillRequest request)
    {
        await _userService.UpdateSkillAsync(id, skillId, request);
        return Ok();
    }

    [HttpDelete("users/{id}/skills/{skillId}")]
    [Authorize(Policy = "Resources.Manage")]
    public async Task<IActionResult> RemoveSkill(int id, int skillId)
    {
        await _userService.RemoveSkillAsync(id, skillId);
        return Ok();
    }

    [HttpGet("projects")]
    [Authorize(Policy = "Projects.Manage")]
    public async Task<IActionResult> GetProjects()
    {
        var projects = await _projectService.GetAllProjectsAsync();
        return Ok(projects);
    }

    [HttpPost("projects")]
    [Authorize(Policy = "Projects.Manage")]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        var project = await _projectService.CreateProjectAsync(request);
        return Ok(project);
    }

    [HttpPut("projects/{id}")]
    [Authorize(Policy = "Projects.Manage")]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
    {
        if (id != request.Id) return BadRequest("ID mismatch");
        await _projectService.UpdateProjectAsync(request);
        return Ok();
    }

    [HttpPost("projects/{id}/milestones")]
    [Authorize(Policy = "Projects.Manage")]
    public async Task<IActionResult> AddMilestone(int id, [FromBody] CreateMilestoneRequest request)
    {
        var milestone = await _projectService.AddMilestoneAsync(id, request);
        return Ok(milestone);
    }

    [HttpPut("projects/{id}/milestones/{milestoneId}/status")]
    [Authorize(Policy = "Projects.Manage")]
    public async Task<IActionResult> UpdateMilestoneStatus(int id, int milestoneId, [FromBody] PRM.Core.Enums.MilestoneStatus status)
    {
        await _projectService.UpdateMilestoneStatusAsync(id, milestoneId, status);
        return Ok();
    }

    [HttpGet("allocations")]
    [Authorize(Policy = "Allocations.ViewAll")]
    public async Task<IActionResult> GetAllocations([FromServices] IAllocationService allocationService)
    {
        var allocations = await allocationService.GetAllAllocationsAsync();
        return Ok(allocations);
    }
}
