using Application.DTOs.Project;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject(CreateProjectRequestDto request)
    {
        try
        {
            var projectId = await _projectService.CreateProjectAsync(request);

            return Ok(new
            {
                Message = "Project created successfully.",
                ProjectId = projectId
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
        var projects = await _projectService.GetAllAsync();

        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _projectService.GetByIdAsync(id);

        if (project == null)
        {
            return NotFound("Project not found.");
        }

        return Ok(project);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(int id, UpdateProjectRequestDto request)
    {
        await _projectService.UpdateProjectAsync(id, request);

        return Ok("Project updated successfully.");
    }

    [HttpGet("{id}/progress")]
    public async Task<IActionResult> GetProjectProgress(int id)
    {
        try
        {
            var progress = await _projectService.GetProjectProgressAsync(id);

            return Ok(progress);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
