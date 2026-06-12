using Application.DTOs.Timesheet;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TimesheetEntriesController : ControllerBase
{
    private readonly ITimesheetEntryService
        _entryService;

    public TimesheetEntriesController(
        ITimesheetEntryService entryService)
    {
        _entryService = entryService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEntry(
        CreateTimesheetEntryRequestDto request)
    {
        try
        {
            var entryId =
                await _entryService
                    .CreateEntryAsync(request);

            return Ok(new
            {
                Message = "Entry created successfully.",
                EntryId = entryId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("timesheet/{timesheetId}")]
    public async Task<IActionResult>
        GetEntries(int timesheetId)
    {
        var entries =
            await _entryService
                .GetByTimesheetIdAsync(timesheetId);

        return Ok(entries);
    }
}
