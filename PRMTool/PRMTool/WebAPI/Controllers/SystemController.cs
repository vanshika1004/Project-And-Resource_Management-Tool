using System;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SystemController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ISystemConfigurationService _configService;

    public SystemController(IMaintenanceService maintenanceService, ISystemConfigurationService configService)
    {
        _maintenanceService = maintenanceService;
        _configService = configService;
    }

    [HttpGet("config")]
    public async Task<IActionResult> GetConfig()
    {
        try
        {
            var config = await _configService.GetConfigurationAsync();
            return Ok(config);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred fetching configuration.", Details = ex.Message });
        }
    }

    [HttpPut("config")]
    public async Task<IActionResult> UpdateConfig([FromBody] Application.DTOs.System.SystemConfigurationDto request)
    {
        try
        {
            await _configService.UpdateConfigurationAsync(request);
            return Ok(new { Message = "Configuration updated successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred updating configuration.", Details = ex.Message });
        }
    }

    /// <summary>
    /// Manually triggers the system maintenance tasks (updates employee status, project health, flags missed timesheets).
    /// </summary>
    /// <returns>A success message upon completion.</returns>
    [HttpPost("run-maintenance")]
    public async Task<IActionResult> RunMaintenance()
    {
        try
        {
            await _maintenanceService.RunMaintenanceAsync();
            return Ok(new { Message = "Maintenance tasks completed successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred during maintenance.", Details = ex.Message });
        }
    }
}
