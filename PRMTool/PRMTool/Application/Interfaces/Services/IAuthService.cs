using Application.DTOs.Auth;

namespace Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task ChangePasswordAsync(ChangePasswordDto request);
}