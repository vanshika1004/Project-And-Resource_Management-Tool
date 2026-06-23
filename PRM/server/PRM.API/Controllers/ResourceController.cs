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
public class ResourceController : ControllerBase
{
    private readonly ITimesheetService _timesheetService;
    private readonly IAllocationService _allocationService;

    public ResourceController(ITimesheetService timesheetService, IAllocationService allocationService)
    {
        _timesheetService = timesheetService;
        _allocationService = allocationService;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue("uid") ?? "0");

    [HttpGet("allocations")]
    [Authorize(Policy = "Allocations.ViewOwn")]
    public async Task<IActionResult> GetMyAllocations()
    {
        var allocations = await _allocationService.GetAllocationsByResourceAsync(GetCurrentUserId());
        return Ok(allocations);
    }

    [HttpPost("timesheets")]
    [Authorize(Policy = "Timesheets.SubmitOwn")]
    public async Task<IActionResult> SubmitTimesheet([FromBody] SubmitTimesheetRequest request)
    {
        try
        {
            var timesheet = await _timesheetService.SubmitTimesheetAsync(request, GetCurrentUserId());
            return Ok(timesheet);
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("timesheets")]
    [Authorize(Policy = "Timesheets.SubmitOwn")]
    public async Task<IActionResult> GetMyTimesheets()
    {
        var timesheets = await _timesheetService.GetMyTimesheetsAsync(GetCurrentUserId());
        return Ok(timesheets);
    }
}
