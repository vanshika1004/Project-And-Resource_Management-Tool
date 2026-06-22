namespace PRM.Application.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, int UserId, string Username, string FullName, string Role, bool ForcePasswordChange);
public record ChangePasswordRequest(int UserId, string OldPassword, string NewPassword);
