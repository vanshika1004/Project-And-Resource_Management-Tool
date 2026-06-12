namespace ConsoleClient.ApiClients;

public class UserApiClient : ApiClientBase
{
    public static async Task<CreateUserResponse> CreateUserAsync(
        string fullName,
        string email,
        string username,
        string temporaryPassword,
        int role)
    {
        var result = await PostAsync<CreateUserResponse>(
            "users",
            new
            {
                FullName = fullName,
                Email = email,
                Username = username,
                TemporaryPassword = temporaryPassword,
                Role = role
            });

        return result
            ?? throw new Exception("Empty response from server.");
    }
    public static async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await GetAsync<List<UserDto>>("users") ?? new List<UserDto>();
    }

    public static async Task ResetPasswordAsync(int userId, string newPassword)
    {
        await PutAsync($"users/{userId}/reset-password", newPassword);
    }

    public static async Task DeactivateUserAsync(int userId)
    {
        await PutAsync($"users/{userId}/deactivate", new { });
    }

    public static async Task ReactivateUserAsync(int userId)
    {
        await PutAsync($"users/{userId}/reactivate", new { });
    }
}

public class CreateUserResponse
{
    public string Message { get; set; } = string.Empty;

    public int UserId { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
