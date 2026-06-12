using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin.User;

public static class CreateUserScreen
{
    public static async Task ShowAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("CREATE USER ACCOUNT");

        var fullName = ConsoleUIHelper.Prompt("Full Name         ");
        var email = ConsoleUIHelper.Prompt("Email             ");
        var username = ConsoleUIHelper.Prompt("Username          ");
        var tempPassword = ConsoleUIHelper.PromptPassword("Temporary Password");

        Console.WriteLine();
        Console.WriteLine("Role:");
        Console.WriteLine("  (1) Admin");
        Console.WriteLine("  (2) Manager");
        Console.WriteLine("  (3) Employee");
        Console.WriteLine();
        var roleInput = ConsoleUIHelper.Prompt("Enter choice");

        int roleValue = roleInput switch
        {
            "1" => 1,
            "2" => 2,
            "3" => 3,
            _ => -1
        };

        if (roleValue == -1)
        {
            ConsoleUIHelper.ShowError("Invalid role selection.");
            ConsoleUIHelper.PressAnyKey();
            return;
        }

        ConsoleUIHelper.DrawSeparator();
        Console.WriteLine("[S] Save     [B] Back");
        Console.WriteLine();

        var option = ConsoleUIHelper.Prompt("Enter option").ToUpper();

        if (option != "S")
        {
            return;
        }

        try
        {
            var result = await UserApiClient.CreateUserAsync(
                fullName, email, username,
                tempPassword, roleValue);

            ConsoleUIHelper.ShowSuccess(
                "Account created. User must change password "
                + $"on first login. (User ID: {result.UserId})");
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError(ex.Message);
        }

        ConsoleUIHelper.PressAnyKey();
    }
}
