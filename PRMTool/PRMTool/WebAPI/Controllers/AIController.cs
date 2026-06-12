using System.Security.Claims;
using Application.DTOs.AI;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Manager")]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("skill-match")]
    public async Task<IActionResult> GetSkillMatch([FromBody] SkillMatchRequestDto request)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var result = await _aiService.GetSkillMatchAsync(request.Requirement, request.ProjectId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred generating skill match.", Details = ex.Message });
        }
    }

    [HttpPost("risk-summary")]
    public async Task<IActionResult> GetRiskSummary([FromBody] RiskSummaryRequestDto request)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var result = await _aiService.GetRiskSummaryAsync(request.ProjectId, userId);
            return Ok(new { Summary = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred generating risk summary.", Details = ex.Message });
        }
    }
}
