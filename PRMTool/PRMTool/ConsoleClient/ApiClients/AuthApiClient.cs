namespace ConsoleClient.ApiClients;

public class AuthApiClient : ApiClientBase
{
    public static async Task<LoginResponse> LoginAsync(
        string username, string password)
    {
        var result = await PostAsync<LoginResponse>(
            "auth/login",
            new { Username = username, Password = password });

        return result
            ?? throw new Exception("Empty response from server.");
    }

    public static async Task ChangePasswordAsync(
        int userId,
        string currentPassword,
        string newPassword)
    {
        await PostAsync(
            "auth/change-password",
            new
            {
                UserId = userId,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            });
    }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool ForcePasswordChange { get; set; }

    public string FullName { get; set; } = string.Empty;

    public int UserId { get; set; }
}
