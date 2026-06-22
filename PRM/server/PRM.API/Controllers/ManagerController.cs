using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM.Application.DTOs;
using PRM.Application.Exceptions;
using PRM.Application.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ManagerController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IAllocationService _allocationService;
    private readonly IAiService _aiService;

    public ManagerController(IProjectService projectService, IAllocationService allocationService, IAiService aiService)
    {
        _projectService = projectService;
        _allocationService = allocationService;
        _aiService = aiService;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue("uid") ?? "0");

    [HttpGet("projects")]
    [Authorize(Policy = "Projects.ViewTeam")]
    public async Task<IActionResult> GetMyProjects()
    {
        var projects = await _projectService.GetMyProjectsAsync(GetCurrentUserId());
        return Ok(projects);
    }

    [HttpPost("allocations")]
    [Authorize(Policy = "Resources.Allocate")]
    public async Task<IActionResult> AllocateResource([FromBody] CreateAllocationRequest request)
    {
        try
        {
            var allocation = await _allocationService.AllocateResourceAsync(request, GetCurrentUserId());
            return Ok(allocation);
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("ai/risk-summary/{projectId}")]
    [Authorize(Policy = "Projects.ViewTeam")]
    public async Task<IActionResult> GetRiskSummary(int projectId)
    {
        try
        {
            var summary = await _aiService.GetProjectRiskSummaryAsync(projectId, GetCurrentUserId());
            return Ok(new { summary });
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("projects/{projectId}/risk-flags")]
    [Authorize(Policy = "Projects.ViewTeam")]
    public async Task<IActionResult> GetProjectRiskFlags(int projectId)
    {
        var flags = await _projectService.GetProjectRiskFlagsAsync(projectId);
        return Ok(flags);
    }

    [HttpGet("team-dashboard")]
    [Authorize(Policy = "Projects.ViewTeam")] 
    public async Task<IActionResult> GetTeamDashboard([FromServices] IUserService userService)
    {
        var members = await userService.GetTeamDashboardAsync(GetCurrentUserId());
        return Ok(members);
    }

    [HttpGet("team/{id}")]
    [Authorize(Policy = "Projects.ViewTeam")] // Changed to Projects.ViewTeam
    public async Task<IActionResult> GetTeamMemberDetails(int id, [FromServices] IUserService userService)
    {
        var details = await userService.GetTeamMemberDetailsAsync(id);
        return Ok(details);
    }

    [HttpPost("ai/skill-match")]
    [Authorize(Policy = "Resources.Allocate")]
    public async Task<IActionResult> SkillMatch([FromBody] PRM.Application.DTOs.AiSkillMatchRequest request)
    {
        try
        {
            var summary = await _aiService.GetSkillMatchRecommendationAsync(request.Requirement, GetCurrentUserId());
            return Ok(new { summary });
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("ai/team-builder")]
    [Authorize(Policy = "Resources.Allocate")]
    public async Task<IActionResult> BuildTeam([FromBody] TeamBuildRequest request)
    {
        try
        {
            var result = await _aiService.BuildTeamAsync(request, GetCurrentUserId());
            return Ok(result);
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("allocations/{id}/end")]
    [Authorize(Policy = "Resources.Allocate")]
    public async Task<IActionResult> EndAllocation(int id, [FromBody] System.DateTime endDate)
    {
        try
        {
            await _allocationService.DeallocateResourceAsync(id, endDate, GetCurrentUserId());
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("projects/{id}")]
    [Authorize(Policy = "Projects.ViewTeam")]
    public async Task<IActionResult> GetProject(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        return Ok(project);
    }

    [HttpGet("team-timesheets")]
    [Authorize(Policy = "Timesheets.ViewTeam")]
    public async Task<IActionResult> GetTeamTimesheets([FromQuery] System.DateTime weekStartDate, [FromServices] ITimesheetService timesheetService)
    {
        var timesheets = await timesheetService.GetTeamTimesheetsAsync(GetCurrentUserId(), weekStartDate);
        return Ok(timesheets);
    }

    [HttpPut("team/{id}/unfreeze")]
    [Authorize(Policy = "Timesheets.ViewTeam")]
    public async Task<IActionResult> UnfreezeTimesheet(int id, [FromServices] IUserService userService)
    {
        try
        {
            await userService.UnfreezeTimesheetAccessAsync(id, GetCurrentUserId());
            return Ok();
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
