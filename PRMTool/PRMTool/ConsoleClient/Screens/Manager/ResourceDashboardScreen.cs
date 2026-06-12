using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Manager;

public static class ResourceDashboardScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();
            ConsoleUIHelper.DrawHeader($"RESOURCE DASHBOARD - {DateTime.Now:MMM yyyy}");
            
            try
            {
                var dashboard = await DashboardApiClient.GetManagerDashboardAsync();
                
                Console.WriteLine($"ON BENCH  ({dashboard.BenchCount} employees available)");
                Console.WriteLine(new string('─', 56));
                Console.WriteLine($"{"ID",-5} {"Name",-17} {"Department",-15} {"Skills"}");
                
                foreach (var emp in dashboard.BenchEmployees)
                {
                    Console.WriteLine($"{emp.Id,-5} {emp.FullName,-17} {emp.Department,-15} {string.Join(", ", emp.Skills)}");
                }
                Console.WriteLine();
                
                Console.WriteLine("ACTIVE EMPLOYEES");
                Console.WriteLine(new string('─', 56));
                Console.WriteLine($"{"ID",-5} {"Name",-17} {"Alloc %",-9} {"Availability"}");
                
                foreach (var emp in dashboard.ActiveEmployees)
                {
                    Console.WriteLine($"{emp.Id,-5} {emp.FullName,-17} {$"{emp.TotalAllocationPercent:0}%",7}   {emp.Availability}");
                }
                
                Console.WriteLine();
                Console.WriteLine(new string('─', 56));
                Console.WriteLine($"Bench: {dashboard.BenchCount}   |   Partial: {dashboard.PartiallyAllocatedCount}");
                Console.WriteLine();

                Console.WriteLine("[D] Drill into employee details      [B] Back");
                Console.WriteLine();

                var option = ConsoleUIHelper.Prompt("Enter option").ToUpper();

                if (option == "B")
                {
                    return;
                }
                else if (option == "D")
                {
                    await ViewEmployeeDetailsAsync();
                }
                else
                {
                    ConsoleUIHelper.ShowError("Invalid option.");
                    ConsoleUIHelper.PressAnyKey();
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

    private static async Task ViewEmployeeDetailsAsync()
    {
        Console.WriteLine();
        var empIdStr = ConsoleUIHelper.Prompt("On [D] — Enter Employee ID");
        if (!int.TryParse(empIdStr, out var empId)) return;

        try
        {
            var emp = await DashboardApiClient.GetEmployeeDetailAsync(empId);

            Console.WriteLine();
            Console.WriteLine($"── {emp.FullName} ".PadRight(56, '─'));
            
            Console.WriteLine($"Department       : {emp.Department}");
            Console.WriteLine($"Current Status   : {emp.Status} ({emp.TotalAllocationPercent:0}%)");
            Console.WriteLine($"Profile Skills   : {string.Join(", ", emp.ProfileSkills)}");
            Console.WriteLine();

            Console.WriteLine("Active Allocations:");
            Console.WriteLine("  Project          %      From        To");
            foreach (var alloc in emp.ActiveAllocations)
            {
                var projName = alloc.ProjectName.Length > 14 ? alloc.ProjectName.Substring(0, 11) + "..." : alloc.ProjectName;
                Console.WriteLine($"  {projName,-14} {$"{alloc.UtilizationPercent:0}%",3}   {alloc.FromDate:dd-MMM-yy}   {alloc.ToDate:dd-MMM-yy}");
            }

            Console.WriteLine();
            Console.WriteLine("Recent Activity Tags (last 4 weeks):");
            if (emp.RecentActivityTags.Any())
            {
                Console.WriteLine($"  {string.Join(", ", emp.RecentActivityTags)}");
            }
            else
            {
                Console.WriteLine("  None");
            }
            Console.WriteLine();
            
            Console.WriteLine("[B] Back");
            Console.WriteLine();
            while (ConsoleUIHelper.Prompt("Enter option").ToUpper() != "B") { }
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError(ex.Message);
            ConsoleUIHelper.PressAnyKey();
        }
    }
}
