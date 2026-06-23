using System.Collections.Generic;
using PRM.Core.Enums;

namespace PRM.Application.DTOs;

public record UserDto(int Id, string Username, string Email, string FullName, string Role, string? Department, int? ManagerId, bool IsActive);
public record CreateUserRequest(string Username, string Email, string FullName, int RoleId, string? Department, int? ManagerId, string TemporaryPassword);
public record UpdateUserRequest(int Id, string FullName, string Email, string? Department, int? ManagerId, bool IsActive);

public record UserSkillDto(int SkillId, string SkillName, SkillCategory Category, ProficiencyLevel ProficiencyLevel);

public record ResetPasswordRequest(string NewTemporaryPassword);
public record AddSkillRequest(string SkillName, SkillCategory Category, ProficiencyLevel ProficiencyLevel);
public record UpdateSkillRequest(ProficiencyLevel ProficiencyLevel);
