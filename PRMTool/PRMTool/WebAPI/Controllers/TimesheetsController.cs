using Application.DTOs.Timesheet;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TimesheetsController : ControllerBase
{
    private readonly ITimesheetService _timesheetService;
    private readonly Application.Interfaces.Repositories.IEmployeeRepository _employeeRepository;

    public TimesheetsController(
        ITimesheetService timesheetService,
        Application.Interfaces.Repositories.IEmployeeRepository employeeRepository)
    {
        _timesheetService = timesheetService;
        _employeeRepository = employeeRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTimesheet(
        CreateTimesheetRequestDto request)
    {
        try
        {
            var timesheetId =
                await _timesheetService
                    .CreateTimesheetAsync(request);

            return Ok(new
            {
                Message = "Timesheet created successfully.",
                TimesheetId = timesheetId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var timesheets = await _timesheetService.GetAllAsync();
        return Ok(timesheets);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTimesheets()
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (int.TryParse(userIdStr, out var userId))
        {
            var emp = await _employeeRepository.GetByUserIdAsync(userId);
            if (emp != null)
            {
                var timesheets = await _timesheetService.GetByEmployeeIdAsync(emp.Id);
                return Ok(timesheets);
            }
        }
        return BadRequest("Unable to identify employee.");
    }

    [HttpGet("team")]
    public async Task<IActionResult> GetTeamTimesheets()
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (int.TryParse(userIdStr, out var userId))
        {
            var emp = await _employeeRepository.GetByUserIdAsync(userId);
            if (emp != null)
            {
                var timesheets = await _timesheetService.GetTeamTimesheetsAsync(emp.Id);
                return Ok(timesheets);
            }
        }
        return BadRequest("Unable to identify manager.");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var timesheet =
            await _timesheetService.GetByIdAsync(id);

        if (timesheet == null)
        {
            return NotFound("Timesheet not found.");
        }

        return Ok(timesheet);
    }
}
