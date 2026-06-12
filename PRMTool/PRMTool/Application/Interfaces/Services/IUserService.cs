using Application.DTOs.Auth;

namespace Application.Interfaces.Services;

public interface IUserService
{
    Task<int> CreateUserAsync(CreateUserRequestDto request);
    Task<List<UserDto>> GetAllAsync();
    Task ResetPasswordAsync(int userId, string newTemporaryPassword);
    Task DeactivateAsync(int userId);
    Task ReactivateAsync(int userId);
}