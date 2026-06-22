namespace PRM.ConsoleClient.Services;

public static class AuthState
{
    public static string? Token { get; set; }
    public static int UserId { get; set; }
    public static string? Username { get; set; }
    public static string? FullName { get; set; }
    public static string? Role { get; set; }

    public static void Clear()
    {
        Token = null;
        UserId = 0;
        Username = null;
        FullName = null;
        Role = null;
    }

    public static bool IsAuthenticated => !string.IsNullOrEmpty(Token);
}
