using Application.DTOs.Skill;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _skillService;

    public SkillsController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSkill(CreateSkillRequestDto request)
    {
        try
        {
            var skillId = await _skillService.CreateSkillAsync(request);

            return Ok(new
            {
                Message = "Skill created successfully.",
                SkillId = skillId
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
        var skills = await _skillService.GetAllAsync();

        return Ok(skills);
    }
}