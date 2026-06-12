using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin;

public static class ViewAllocationsScreen
{
    public static async Task ShowAsync()
    {
        string filter = string.Empty;

        while (true)
        {
            ConsoleUIHelper.ClearScreen();
            ConsoleUIHelper.DrawHeader("ALL ACTIVE ALLOCATIONS");
            
            try
            {
                var allocations = await AllocationApiClient.GetAllAsync();
                var activeAllocations = allocations.Where(a => a.ToDate >= DateTime.Now.Date).ToList();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    activeAllocations = activeAllocations.Where(a => 
                        a.ProjectName.Contains(filter, StringComparison.OrdinalIgnoreCase) || 
                        a.EmployeeName.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                Console.WriteLine($"{"Employee",-15} {"Project",-16} {"%",-6} {"From",-12} {"To"}");
                Console.WriteLine(new string('─', 62));
                
                foreach (var alloc in activeAllocations)
                {
                    var empName = alloc.EmployeeName.Length > 14 ? alloc.EmployeeName.Substring(0, 11) + "..." : alloc.EmployeeName;
                    var projName = alloc.ProjectName.Length > 15 ? alloc.ProjectName.Substring(0, 12) + "..." : alloc.ProjectName;
                    Console.WriteLine($"{empName,-15} {projName,-16} {$"{alloc.UtilizationPercent:0}%",-6} {alloc.FromDate:dd-MMM-yy,-12} {alloc.ToDate:dd-MMM-yy}");
                }
                
                Console.WriteLine(new string('─', 62));
                Console.WriteLine($"Total Active Allocations: {activeAllocations.Count}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                ConsoleUIHelper.ShowError(ex.Message);
            }
            
            Console.WriteLine("[F] Filter by Project/Employee     [B] Back");
            Console.WriteLine();

            var option = ConsoleUIHelper.Prompt("Enter option").ToUpper();
            if (option == "B") return;
            if (option == "F")
            {
                filter = ConsoleUIHelper.Prompt("Enter filter string");
            }
        }
    }
}
