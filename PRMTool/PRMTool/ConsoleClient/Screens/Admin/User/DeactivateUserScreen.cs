using System;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin.User;

public static class DeactivateUserScreen
{
    public static async Task ShowAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("DEACTIVATE USER ACCOUNT");

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
            Console.WriteLine($"Role   : {user.Role}");
            Console.WriteLine($"Status : {(user.IsActive ? "ACTIVE" : "INACTIVE")}");
            Console.WriteLine();

            Console.WriteLine("Are you sure you want to deactivate this user?");
            Console.WriteLine("They will no longer be able to log in.");
            Console.WriteLine();

            Console.WriteLine("[Y] Yes, Deactivate     [B] Cancel");
            Console.WriteLine();
            
            var option = ConsoleUIHelper.Prompt("Enter option").ToUpper();
            if (option == "Y")
            {
                await UserApiClient.DeactivateUserAsync(userId);
                ConsoleUIHelper.ShowSuccess("User deactivated. ✓");
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
