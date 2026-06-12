using System;
using System.Threading.Tasks;
using ConsoleClient.Screens.Admin.User;

namespace ConsoleClient.Screens.Admin;

public static class ManageUsersScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();
            ConsoleUIHelper.DrawHeader("MANAGE USERS");

            var choice = ConsoleUIHelper.ShowMenu(new[]
            {
                "Create User Account",
                "View All Users",
                "Reset Password",
                "Deactivate User Account",
                "Back"
            });

            switch (choice)
            {
                case 1:
                    await CreateUserScreen.ShowAsync();
                    break;
                case 2:
                    await ViewAllUsersScreen.ShowAsync();
                    break;
                case 3:
                    await ResetPasswordScreen.ShowAsync();
                    break;
                case 4:
                    await DeactivateUserScreen.ShowAsync();
                    break;
                case 5:
                    return;
                default:
                    ConsoleUIHelper.ShowError("Invalid option.");
                    ConsoleUIHelper.PressAnyKey();
                    break;
            }
        }
    }
}
