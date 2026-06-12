namespace ConsoleClient.Storage;

public static class SessionManager
{
    public static string? Token { get; private set; }

    public static string? Username { get; private set; }

    public static string? FullName { get; private set; }

    public static string? Role { get; private set; }

    public static int UserId { get; private set; }

    public static bool ForcePasswordChange { get; private set; }

    public static void SetSession(
        string token,
        string username,
        string fullName,
        string role,
        int userId,
        bool forcePasswordChange)
    {
        Token = token;
        Username = username;
        FullName = fullName;
        Role = role;
        UserId = userId;
        ForcePasswordChange = forcePasswordChange;
    }

    public static void ClearForcePasswordChange()
    {
        ForcePasswordChange = false;
    }

    public static void ClearSession()
    {
        Token = null;
        Username = null;
        FullName = null;
        Role = null;
        UserId = 0;
        ForcePasswordChange = false;
    }

    public static bool IsLoggedIn => !string.IsNullOrEmpty(Token);
}
