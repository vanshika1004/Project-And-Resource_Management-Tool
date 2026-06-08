using Application.DTOs.Skill;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeSkillsController : ControllerBase
{
    private readonly IEmployeeSkillService _employeeSkillService;

    public EmployeeSkillsController(IEmployeeSkillService employeeSkillService)
    {
        _employeeSkillService = employeeSkillService;
    }

    [HttpPost]
    public async Task<IActionResult> AssignSkill(AssignEmployeeSkillRequestDto request)
    {
        var id = await _employeeSkillService.AssignSkillAsync(request);

        return Ok(new
        {
            Message = "Skill assigned successfully.",
            Id = id
        });
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeeSkills(int employeeId)
    {
        var skills = await _employeeSkillService.GetEmployeeSkillsAsync(employeeId);

        return Ok(skills);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProficiency(int id, UpdateEmployeeSkillRequestDto request)
    {
        await _employeeSkillService.UpdateProficiencyAsync(id, request);

        return Ok("Proficiency updated successfully.");
    }
}