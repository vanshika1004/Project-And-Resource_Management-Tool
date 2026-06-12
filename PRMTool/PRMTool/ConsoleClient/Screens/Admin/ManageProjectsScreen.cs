using System;
using System.Threading.Tasks;
using ConsoleClient.Screens.Admin.Project;

namespace ConsoleClient.Screens.Admin;

public static class ManageProjectsScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();
            ConsoleUIHelper.DrawHeader("MANAGE PROJECTS");

            var choice = ConsoleUIHelper.ShowMenu(new[]
            {
                "Create Project",
                "View All Projects",
                "Update Project Details",
                "Manage Milestones",
                "Back"
            });

            switch (choice)
            {
                case 1:
                    await CreateProjectScreen.ShowAsync();
                    break;
                case 2:
                    await ViewAllProjectsScreen.ShowAsync();
                    break;
                case 3:
                    await UpdateProjectScreen.ShowAsync();
                    break;
                case 4:
                    await ManageMilestonesScreen.ShowAsync();
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
