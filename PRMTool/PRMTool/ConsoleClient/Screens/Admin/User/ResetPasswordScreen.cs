using System;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin.User;

public static class ResetPasswordScreen
{
    public static async Task ShowAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("RESET PASSWORD");

        var userIdStr = ConsoleUIHelper.Prompt("Enter User ID");
        if (!int.TryParse(userIdStr, out var userId)) return;

        try
        {
            var users = await UserApiClient.GetAllUsersAsync();
            var user = users.Find(u => u.Id == userId);
            
            if (user == null)
            {
                ConsoleUIHelper.ShowError("User not found.");
                ConsoleUIHelper.PressAnyKey();
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"── {user.Username} ".PadRight(46, '─'));
            Console.WriteLine($"Role: {user.Role}");
            Console.WriteLine();

            var tempPassword = ConsoleUIHelper.PromptPassword("New Temporary Password");

            ConsoleUIHelper.DrawSeparator();
            Console.WriteLine("[S] Save     [B] Cancel");
            Console.WriteLine();
            
            var option = ConsoleUIHelper.Prompt("Enter option").ToUpper();
            if (option == "S")
            {
                await UserApiClient.ResetPasswordAsync(userId, tempPassword);

                ConsoleUIHelper.ShowSuccess("Password reset successfully. User will be forced\nto change it on their next login. ✓");
                ConsoleUIHelper.PressAnyKey();
            }
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError(ex.Message);
            ConsoleUIHelper.PressAnyKey();
        }
    }
}
