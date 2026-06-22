using System;
using System.Threading.Tasks;
using PRM.Application.DTOs;
using PRM.Application.Exceptions;
using PRM.Application.Interfaces;
using PRM.Application.Security;
using PRM.Core.Interfaces;

namespace PRM.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(IUserRepository userRepo, IJwtTokenGenerator tokenGenerator)
    {
        _userRepo = userRepo;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.GetByUsernameAsync(request.Username);

        if (user == null || !user.IsActive)
            throw new BusinessRuleViolationException("Invalid username or password.");

        if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
            throw new BusinessRuleViolationException("Invalid username or password.");

        var token = _tokenGenerator.GenerateToken(user);

        return new LoginResponse(token, user.Id, user.Username, user.FullName, user.Role!.Name, user.ForcePasswordChange);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException("User", request.UserId);

        if (!PasswordHasher.Verify(request.OldPassword, user.PasswordHash))
            throw new BusinessRuleViolationException("Incorrect old password.");

        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        user.ForcePasswordChange = false;
        
        await _userRepo.UpdateAsync(user);
    }
}
