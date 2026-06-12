using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly Application.Interfaces.Repositories.IEmployeeRepository _employeeRepository;

    public DashboardController(
        IDashboardService dashboardService,
        Application.Interfaces.Repositories.IEmployeeRepository employeeRepository)
    {
        _dashboardService = dashboardService;
        _employeeRepository = employeeRepository;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary =
            await _dashboardService
                .GetDashboardSummaryAsync();

        return Ok(summary);
    }

    [HttpGet("resources")]
    public async Task<IActionResult> GetResources()
    {
        int? managerId = null;
        if (User.IsInRole("Manager"))
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (int.TryParse(userIdStr, out var userId))
            {
                var emp = await _employeeRepository.GetByUserIdAsync(userId);
                managerId = emp?.Id;
            }
        }

        var result = await _dashboardService.GetResourceDashboardAsync(managerId);

        return Ok(result);
    }

    [HttpGet("resources/{employeeId}")]
    public async Task<IActionResult> GetResourceDetail(
        int employeeId)
    {
        var result =
            await _dashboardService
                .GetEmployeeDetailAsync(employeeId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("utilization")]
    public async Task<IActionResult> GetUtilization()
    {
        var result =
            await _dashboardService
                .GetUtilizationReportAsync();

        return Ok(result);
    }

    [HttpGet("team-timesheets")]
    public async Task<IActionResult> GetTeamTimesheets(
        [FromQuery] DateTime? weekStartDate)
    {
        var result =
            await _dashboardService
                .GetTeamTimesheetReportAsync(weekStartDate);

        return Ok(result);
    }

    [HttpGet("skills")]
    public async Task<IActionResult> GetSkillMatrix()
    {
        var result =
            await _dashboardService
                .GetSkillMatrixReportAsync();

        return Ok(result);
    }

    [HttpGet("projects")]
    public async Task<IActionResult> GetProjectsDashboard()
    {
        var result =
            await _dashboardService
                .GetProjectDashboardAsync();

        return Ok(result);
    }
}