using ConsoleClient.Screens.Admin;
using ConsoleClient.Storage;

namespace ConsoleClient.Screens;

public static class AdminMenuScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();

            var now = DateTime.Now;

            ConsoleUIHelper.DrawHeader(
                "ADMIN PANEL",
                $"Welcome, {SessionManager.FullName}  |  "
                + now.ToString("dd-MM-yyyy  HH:mm"));

            var choice = ConsoleUIHelper.ShowMenu(new[]
            {
                "Manage Employees",
                "Manage Projects",
                "View All Allocations",
                "Manage Users",
                "System Configuration",
                "Logout"
            });

            switch (choice)
            {
                case 1:
                    await ManageEmployeesScreen.ShowAsync();
                    break;

                case 2:
                    await ManageProjectsScreen.ShowAsync();
                    break;

                case 3:
                    await ViewAllocationsScreen.ShowAsync();
                    break;

                case 4:
                    await ManageUsersScreen.ShowAsync();
                    break;

                case 5:
                    await SystemConfigScreen.ShowAsync();
                    break;

                case 6:
                    SessionManager.ClearSession();
                    ConsoleUIHelper.ShowSuccess("Logged out.");
                    ConsoleUIHelper.PressAnyKey();
                    return;

                default:
                    ConsoleUIHelper.ShowError(
                        "Invalid option. Please try again.");
                    ConsoleUIHelper.PressAnyKey();
                    break;
            }
        }
    }
}
