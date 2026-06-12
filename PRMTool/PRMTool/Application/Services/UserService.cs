using Application.DTOs.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public UserService(IUserRepository userRepository, IEmployeeRepository employeeRepository)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<int> CreateUserAsync(CreateUserRequestDto request)
    {
        if (await _userRepository.UsernameExistsAsync(request.Username))
        {
            throw new Exception("Username already exists.");
        }

        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new Exception("Email already exists.");
        }

        var user = new User
        {
            FullName = request.FullName,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.TemporaryPassword),
            Role = request.Role,
            ForcePasswordChange = true,
            IsActive = true
        };

        await _userRepository.AddAsync(user);

        await _userRepository.SaveChangesAsync();


        if (request.Role == UserRole.Manager || request.Role == UserRole.Employee)
        {
            var employee = new Employee
            {
                UserId = user.Id,
                FullName = request.FullName,
                Department = string.Empty,
                Designation = string.Empty,
                Status = EmployeeStatus.Bench,
                IsActive = true
            };

            await _employeeRepository.AddAsync(employee);

            await _employeeRepository.SaveChangesAsync();
        }

        return user.Id;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Role = u.Role.ToString(),
            IsActive = u.IsActive
        }).ToList();
    }

    public async Task ResetPasswordAsync(int userId, string newTemporaryPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newTemporaryPassword);
        user.ForcePasswordChange = true;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found.");

        user.IsActive = false;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task ReactivateAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found.");

        user.IsActive = true;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }
}