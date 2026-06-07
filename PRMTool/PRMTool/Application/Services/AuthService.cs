using Application.DTOs.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using BCrypt.Net;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null) throw new Exception("Invalid username or password.");

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!passwordValid) throw new Exception("Invalid username or password.");

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResponseDto
        {
            Token = token,
            Username = user.Username,
            Role = user.Role.ToString(),
            ForcePasswordChange = user.ForcePasswordChange
        };
    }

    public async Task ChangePasswordAsync(ChangePasswordDto request)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);

        if (user == null)
            throw new Exception("User not found.");

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);

        if (!passwordValid)
            throw new Exception("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        user.ForcePasswordChange = false;

        _userRepository.Update(user);

        await _userRepository.SaveChangesAsync();
    }
}