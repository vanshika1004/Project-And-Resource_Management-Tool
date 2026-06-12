using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;
using ConsoleClient.Storage;

namespace ConsoleClient.Screens.Employee;

public static class MyAllocationsScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();
            ConsoleUIHelper.DrawHeader("MY ALLOCATIONS");

            try
            {
                var employees = await EmployeeApiClient.GetAllAsync();
                var me = employees.FirstOrDefault(e => e.UserId == SessionManager.UserId);
                
                if (me == null)
                {
                    ConsoleUIHelper.ShowError("Employee profile not found for this user.");
                    ConsoleUIHelper.PressAnyKey();
                    return;
                }

                var allAllocations = await AllocationApiClient.GetAllAsync();
                var myAllocations = allAllocations.Where(a => a.EmployeeId == me.Id && a.ToDate >= DateTime.Now.Date).ToList();

                Console.WriteLine($"{"No.",-4} {"Project",-16} {"%",-6} {"From",-12} {"To"}");
                Console.WriteLine(new string('─', 52));

                var totalUtil = 0m;
                for (int i = 0; i < myAllocations.Count; i++)
                {
                    var alloc = myAllocations[i];
                    var projName = alloc.ProjectName.Length > 15 ? alloc.ProjectName.Substring(0, 12) + "..." : alloc.ProjectName;
                    Console.WriteLine($"{i + 1,-4} {projName,-16} {$"{alloc.UtilizationPercent:0}%",-6} {alloc.FromDate:dd-MMM-yy,-12} {alloc.ToDate:dd-MMM-yy}");
                    totalUtil += alloc.UtilizationPercent;
                }

                Console.WriteLine(new string('─', 52));
                Console.WriteLine($"Total Current Utilisation: {totalUtil:0}%");
                Console.WriteLine($"Current Status: {me.Status}");
                Console.WriteLine();
                
                Console.WriteLine("[V] View Project Details     [B] Back");
                Console.WriteLine();

                var option = ConsoleUIHelper.Prompt("Enter option").ToUpper();
                if (option == "B") return;
                if (option == "V")
                {
                    var idxStr = ConsoleUIHelper.Prompt("Enter project number to view");
                    if (int.TryParse(idxStr, out var idx) && idx >= 1 && idx <= myAllocations.Count)
                    {
                        var selectedAlloc = myAllocations[idx - 1];
                        await ShowProjectDetails(selectedAlloc.ProjectId);
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleUIHelper.ShowError(ex.Message);
                Console.WriteLine("[B] Back");
                Console.WriteLine();
                while (ConsoleUIHelper.Prompt("Enter option").ToUpper() != "B") { }
                return;
            }
        }
    }

    private static async Task ShowProjectDetails(int projectId)
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("PROJECT DETAILS");

        try
        {
            var project = await ProjectApiClient.GetByIdAsync(projectId);
            if (project == null)
            {
                ConsoleUIHelper.ShowError("Project details not found.");
                ConsoleUIHelper.PressAnyKey();
                return;
            }

            Console.WriteLine($"Name:        {project.ProjectName}");
            Console.WriteLine($"Manager:     {project.ManagerName}");
            Console.WriteLine($"Status:      {project.Status}");
            Console.WriteLine($"Health:      {project.HealthStatus}");
            Console.WriteLine($"Start Date:  {project.StartDate:dd-MMM-yyyy}");
            Console.WriteLine($"End Date:    {project.EndDate:dd-MMM-yyyy}");
            Console.WriteLine($"Progress:    {project.StoryPointsCompleted} / {project.TotalStoryPoints} Story Points");
            Console.WriteLine();
            Console.WriteLine("Description:");
            Console.WriteLine(project.Description);
            Console.WriteLine(new string('─', 60));
            Console.WriteLine();

            Console.WriteLine("[A] AI Risk Summary     [B] Back");
            Console.WriteLine();

            while (true)
            {
                var opt = ConsoleUIHelper.Prompt("Enter option").ToUpper();
                if (opt == "B") return;
                if (opt == "A")
                {
                    Console.WriteLine("\nGenerating AI Risk Summary...");
                    var riskSummary = await AIApiClient.GetRiskSummaryAsync(projectId);
                    Console.WriteLine();
                    ConsoleUIHelper.DrawHeader("AI RISK SUMMARY");
                    Console.WriteLine(riskSummary ?? "No summary available.");
                    Console.WriteLine(new string('─', 60));
                    Console.WriteLine();
                    ConsoleUIHelper.PressAnyKey();
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError(ex.Message);
            ConsoleUIHelper.PressAnyKey();
        }
    }
}
