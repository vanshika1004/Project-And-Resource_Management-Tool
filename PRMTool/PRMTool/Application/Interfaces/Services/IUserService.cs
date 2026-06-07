using Application.DTOs.Auth;

namespace Application.Interfaces.Services;

public interface IUserService
{
    Task<int> CreateUserAsync(CreateUserRequestDto request);
}