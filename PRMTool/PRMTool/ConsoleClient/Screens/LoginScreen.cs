using ConsoleClient.ApiClients;
using ConsoleClient.Storage;

namespace ConsoleClient.Screens;

public static class LoginScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();

            ConsoleUIHelper.DrawHeader(
                "PROJECT & RESOURCE MANAGEMENT TOOL",
                "Learn & Code — Final Project");

            var choice = ConsoleUIHelper.ShowMenu(new[]
            {
                "Login",
                "Exit"
            });

            switch (choice)
            {
                case 1:
                    await HandleLoginAsync();
                    break;

                case 2:
                    ConsoleUIHelper.ShowInfo(
                        "\nGoodbye! Application terminated.");
                    Environment.Exit(0);
                    break;

                default:
                    ConsoleUIHelper.ShowError(
                        "Invalid option. Please try again.");
                    ConsoleUIHelper.PressAnyKey();
                    break;
            }
        }
    }

    private static async Task HandleLoginAsync()
    {
        Console.WriteLine();

        var username = ConsoleUIHelper.Prompt("Username");
        var password = ConsoleUIHelper.PromptPassword("Password");

        try
        {
            ConsoleUIHelper.ShowInfo("\nAuthenticating...");

            var response = await AuthApiClient.LoginAsync(
                username, password);

            SessionManager.SetSession(
                token: response.Token,
                username: response.Username,
                fullName: response.FullName,
                role: response.Role,
                userId: response.UserId,
                forcePasswordChange: response.ForcePasswordChange);

            ConsoleUIHelper.ShowSuccess(
                $"Welcome, {response.Username}!");

            if (response.ForcePasswordChange)
            {
                ConsoleUIHelper.ShowWarning(
                    "You must change your password "
                    + "before continuing.");
                ConsoleUIHelper.PressAnyKey();

                await ChangePasswordScreen.ShowAsync(
                    response.UserId, password);
            }

            // Route to role-specific menu
            await RouteToMenuAsync();
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError(ex.Message);
            ConsoleUIHelper.PressAnyKey();
        }
    }

    private static async Task RouteToMenuAsync()
    {
        var role = SessionManager.Role?.ToUpper();

        switch (role)
        {
            case "ADMIN":
                await AdminMenuScreen.ShowAsync();
                break;

            case "MANAGER":
                await ManagerMenuScreen.ShowAsync();
                break;

            case "EMPLOYEE":
                await EmployeeMenuScreen.ShowAsync();
                break;

            default:
                ConsoleUIHelper.ShowError(
                    $"Unknown role: {role}");
                SessionManager.ClearSession();
                ConsoleUIHelper.PressAnyKey();
                break;
        }
    }
}
