using System;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin.Project;

public static class CreateProjectScreen
{
    public static async Task ShowAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("CREATE PROJECT");

        var name = ConsoleUIHelper.Prompt("Project Name");
        var desc = ConsoleUIHelper.Prompt("Description");
        var managerIdStr = ConsoleUIHelper.Prompt("Manager User ID");
        
        DateTime start;
        while (!DateTime.TryParseExact(ConsoleUIHelper.Prompt("Start Date (DD-MM-YYYY)"), "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out start))
        {
            ConsoleUIHelper.ShowError("Invalid date format. Use DD-MM-YYYY.");
        }

        var endStr = ConsoleUIHelper.Prompt("End Date   (DD-MM-YYYY)");
        DateTime? end = null;
        if (DateTime.TryParseExact(endStr, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var eDate))
        {
            end = eDate;
        }

        var spStr = ConsoleUIHelper.Prompt("Total Story Points");
        int.TryParse(spStr, out var totalSp);
        
        ConsoleUIHelper.DrawSeparator();
        Console.WriteLine("[S] Save Project     [B] Cancel");
        Console.WriteLine();
        
        var option = ConsoleUIHelper.Prompt("Enter option").ToUpper();
        if (option == "B") return;

        if (option == "S")
        {
            if (!int.TryParse(managerIdStr, out var managerId))
            {
                ConsoleUIHelper.ShowError("Invalid Manager ID.");
                ConsoleUIHelper.PressAnyKey();
                return;
            }
            
            try
            {
                await ProjectApiClient.CreateAsync(new CreateProjectDto
                {
                    ProjectName = name,
                    Description = desc,
                    StartDate = start,
                    EndDate = end ?? DateTime.MinValue,
                    ManagerId = managerId,
                    TotalStoryPoints = totalSp
                });
                
                ConsoleUIHelper.ShowSuccess("Project created successfully!");
                ConsoleUIHelper.PressAnyKey();
            }
            catch (Exception ex)
            {
                ConsoleUIHelper.ShowError(ex.Message);
                ConsoleUIHelper.PressAnyKey();
            }
        }
    }
}
