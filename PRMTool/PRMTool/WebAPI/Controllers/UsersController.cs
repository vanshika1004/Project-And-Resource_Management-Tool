using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequestDto request)
    {
        try
        {
            var userId = await _userService.CreateUserAsync(request);

            return Ok(new
            {
                Message = "User created successfully.",
                UserId = userId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}