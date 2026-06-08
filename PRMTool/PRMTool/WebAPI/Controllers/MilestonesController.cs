using Application.DTOs.Project;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MilestonesController : ControllerBase
{
    private readonly IMilestoneService _milestoneService;

    public MilestonesController(IMilestoneService milestoneService)
    {
        _milestoneService = milestoneService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMilestone(CreateMilestoneRequestDto request)
    {
        try
        {
            var milestoneId = await _milestoneService.CreateMilestoneAsync(request);

            return Ok(new
            {
                Message = "Milestone created successfully.",
                MilestoneId = milestoneId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetProjectMilestones(int projectId)
    {
        var milestones = await _milestoneService.GetByProjectIdAsync(projectId);

        return Ok(milestones);
    }
}