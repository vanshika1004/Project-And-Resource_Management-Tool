using Application.DTOs.Employee;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly Application.Interfaces.Repositories.IEmployeeRepository _employeeRepository;

    public EmployeesController(
        IEmployeeService employeeService,
        Application.Interfaces.Repositories.IEmployeeRepository employeeRepository)
    {
        _employeeService = employeeService;
        _employeeRepository = employeeRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _employeeService.GetAllAsync();

        return Ok(employees);
    }

    [HttpGet("team")]
    public async Task<IActionResult> GetMyTeam()
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (int.TryParse(userIdStr, out var userId))
        {
            var emp = await _employeeRepository.GetByUserIdAsync(userId);
            if (emp != null)
            {
                var employees = await _employeeService.GetAllAsync();
                var team = employees.Where(e => e.ManagerId == emp.Id).ToList();
                return Ok(team);
            }
        }
        return BadRequest("Unable to identify manager.");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);

        if (employee == null)
        {
            return NotFound("Employee not found.");
        }

        return Ok(employee);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateEmployeeRequestDto request)
    {
        try
        {
            await _employeeService.UpdateAsync(id, request);

            return Ok("Employee updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
