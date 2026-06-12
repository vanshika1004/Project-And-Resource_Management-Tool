using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin.Project;

public static class ViewAllProjectsScreen
{
    public static async Task ShowAsync()
    {
        var filter = string.Empty;

        while (true)
        {
            ConsoleUIHelper.ClearScreen();
            ConsoleUIHelper.DrawHeader("ALL PROJECTS");
            
            try
            {
                var projects = await ProjectApiClient.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    projects = projects.Where(p => 
                        p.Status.Contains(filter, StringComparison.OrdinalIgnoreCase) || 
                        p.ManagerName.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                Console.WriteLine($"{"ID",-5} {"Name",-16} {"Manager",-13} {"End Date",-12} {"Status",-11} {"SP Done/Total"}");
                Console.WriteLine(new string('─', 75));
                
                foreach (var proj in projects)
                {
                    var shortName = proj.ProjectName.Length > 15 ? proj.ProjectName.Substring(0, 12) + "..." : proj.ProjectName;
                    var shortManager = proj.ManagerName.Length > 13 ? proj.ManagerName.Substring(0, 10) + "..." : proj.ManagerName;
                    var endDate = proj.EndDate != DateTime.MinValue ? proj.EndDate.ToString("dd-MMM-yy") : "-";
                    var sp = $"{proj.StoryPointsCompleted} / {proj.TotalStoryPoints}";
                    Console.WriteLine($"{proj.Id,-5} {shortName,-16} {shortManager,-13} {endDate,-12} {proj.Status,-11} {sp}");
                }
                
                Console.WriteLine(new string('─', 75));
                Console.WriteLine();
                
                Console.WriteLine("[F] Filter by Status / Manager        [B] Back");
                Console.WriteLine();
                
                var input = ConsoleUIHelper.Prompt("Enter option").ToUpper();
                
                if (input == "B")
                {
                    return;
                }
                else if (input == "F")
                {
                    filter = ConsoleUIHelper.Prompt("Enter filter string");
                }
            }
            catch (Exception ex)
            {
                ConsoleUIHelper.ShowError(ex.Message);
                ConsoleUIHelper.PressAnyKey();
                return;
            }
        }
    }
}
