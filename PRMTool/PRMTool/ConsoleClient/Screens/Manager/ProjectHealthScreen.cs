using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Manager;

public static class ProjectHealthScreen
{
    public static async Task ShowAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("MY PROJECTS (Health View)");
        
        try
        {
            var dashboard = await DashboardApiClient.GetProjectDashboardAsync();
            
            Console.WriteLine($"{"ID",-5} {"Name",-16} {"Status",-12} {"Health",-9} {"Next Milestone"}");
            Console.WriteLine(new string('─', 60));
            
            foreach (var proj in dashboard.Projects)
            {
                var pName = proj.ProjectName.Length > 15 ? proj.ProjectName.Substring(0, 12) + "..." : proj.ProjectName;
                var nextMsDate = proj.Milestones.OrderBy(m => m.DueDate).FirstOrDefault()?.DueDate;
                var nextMs = nextMsDate?.ToString("dd-MMM-yy") ?? "(None)";
                Console.WriteLine($"{proj.ProjectId,-5} {pName,-16} {proj.Status,-12} {proj.HealthStatus,-9} {nextMs}");
            }
            Console.WriteLine(new string('─', 60));
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError(ex.Message);
        }
        
        Console.WriteLine("[B] Back");
        Console.WriteLine();
        while (ConsoleUIHelper.Prompt("Enter option").ToUpper() != "B") { }
    }
}
