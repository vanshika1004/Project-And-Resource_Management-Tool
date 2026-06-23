using Microsoft.AspNetCore.Mvc;
using PRM.Application.DTOs;
using PRM.Application.Exceptions;
using PRM.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace PRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (BusinessRuleViolationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            await _authService.ChangePasswordAsync(request);
            return Ok(new { message = "Password changed successfully." });
        }
        catch (Exception ex) when (ex is BusinessRuleViolationException || ex is NotFoundException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
