using Domain.Enums;

namespace Application.DTOs.Auth;

public class CreateUserRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string TemporaryPassword { get; set; } = string.Empty;

    public UserRole Role { get; set; }
}