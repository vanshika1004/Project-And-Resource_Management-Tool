using ConsoleClient.Storage;
using ConsoleClient.Screens.Manager;

namespace ConsoleClient.Screens;

public static class ManagerMenuScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();

            var now = DateTime.Now;

            ConsoleUIHelper.DrawHeader(
                $"Welcome, {SessionManager.FullName}!",
                now.ToString("dd-MMM-yyyy  HH:mm"));

            var choice = ConsoleUIHelper.ShowMenu(new[]
            {
                "Resource Dashboard",
                "Allocate Resource",
                "My Projects",
                "Timesheets",
                "AI Assistant",
                "Logout"
            });

            switch (choice)
            {
                case 1:
                    await ResourceDashboardScreen.ShowAsync();
                    break;

                case 2:
                    await AllocateResourceScreen.ShowAsync();
                    break;

                case 3:
                    await ProjectHealthScreen.ShowAsync();
                    break;

                case 4:
                    await TeamTimesheetsScreen.ShowAsync();
                    break;

                case 5:
                    await AIAssistantScreen.ShowAsync();
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
