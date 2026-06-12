using System;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin.Project;

public static class UpdateProjectScreen
{
    public static async Task ShowAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("UPDATE PROJECT DETAILS");

        var projIdStr = ConsoleUIHelper.Prompt("Enter Project ID to update");
        if (!int.TryParse(projIdStr, out var projId)) return;

        try
        {
            var proj = await ProjectApiClient.GetByIdAsync(projId);
            if (proj == null)
            {
                ConsoleUIHelper.ShowError("Project not found.");
                ConsoleUIHelper.PressAnyKey();
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"── {proj.ProjectName} ".PadRight(46, '─'));
            Console.WriteLine($"Current Manager User ID  : {proj.ManagerId}");
            Console.WriteLine($"Current Start Date       : {proj.StartDate:dd-MMM-yyyy}");
            Console.WriteLine($"Current End Date         : {(proj.EndDate != DateTime.MinValue ? proj.EndDate.ToString("dd-MMM-yyyy") : "-")}");
            Console.WriteLine($"Total Story Points       : {proj.TotalStoryPoints} (editable)");
            Console.WriteLine();

            var managerIdStr = ConsoleUIHelper.Prompt($"Manager User ID (press Enter to keep {proj.ManagerId})");
            var managerId = string.IsNullOrWhiteSpace(managerIdStr) ? proj.ManagerId : int.Parse(managerIdStr);

            var startStr = ConsoleUIHelper.Prompt($"Start Date (press Enter to keep {proj.StartDate:dd-MMM-yyyy})");
            var start = string.IsNullOrWhiteSpace(startStr) ? proj.StartDate : DateTime.ParseExact(startStr, "dd-MMM-yyyy", null);

            var endStr = ConsoleUIHelper.Prompt($"End Date (press Enter to keep {(proj.EndDate != DateTime.MinValue ? proj.EndDate.ToString("dd-MMM-yyyy") : "-")})");
            var end = string.IsNullOrWhiteSpace(endStr) ? proj.EndDate : DateTime.ParseExact(endStr, "dd-MMM-yyyy", null);

            var spStr = ConsoleUIHelper.Prompt($"Total Story Points (press Enter to keep {proj.TotalStoryPoints})");
            var sp = string.IsNullOrWhiteSpace(spStr) ? proj.TotalStoryPoints : int.Parse(spStr);

            ConsoleUIHelper.DrawSeparator();
            Console.WriteLine("[S] Save Updates     [B] Cancel");
            Console.WriteLine();
            
            var option = ConsoleUIHelper.Prompt("Enter option").ToUpper();
            if (option == "S")
            {
                // Update properties in Backend Project (StoryPoints would require another API endpoint, 
                // but we will send what's available).
                await ProjectApiClient.UpdateAsync(projId, new UpdateProjectDto
                {
                    ProjectName = proj.ProjectName,
                    Description = proj.Description,
                    StartDate = start,
                    EndDate = end,
                    Status = proj.Status,
                    HealthStatus = proj.HealthStatus,
                    ManagerId = managerId,
                    TotalStoryPoints = sp
                });
                
                ConsoleUIHelper.ShowSuccess("Project updated successfully!");
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
