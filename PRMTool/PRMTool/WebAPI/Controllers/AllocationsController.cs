using Application.DTOs.Allocation;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AllocationsController : ControllerBase
{
    private readonly IAllocationService _allocationService;

    public AllocationsController(IAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAllocation(CreateAllocationRequestDto request)
    {
        try
        {
            var allocationId = await _allocationService.CreateAllocationAsync(request);

            return Ok(new
            {
                Message = "Allocation created successfully.",
                AllocationId = allocationId
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
        var allocations = await _allocationService.GetAllAsync();

        return Ok(allocations);
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetByProjectId(int projectId)
    {
        var allocations = await _allocationService.GetByProjectIdAsync(projectId);

        return Ok(allocations);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAllocation(int id, UpdateAllocationRequestDto request)
    {
        try
        {
            await _allocationService.UpdateAllocationAsync(id, request);

            return Ok("Allocation updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/end")]
    public async Task<IActionResult> EndAllocation(int id)
    {
        try
        {
            await _allocationService.EndAllocationAsync(id);
            return Ok(new { Message = "Allocation ended successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
