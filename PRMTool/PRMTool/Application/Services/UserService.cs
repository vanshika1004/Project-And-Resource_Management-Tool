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

        // Manager and Employee get employee profiles

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
}