using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin.Employee;

public static class ViewAllEmployeesScreen
{
    public static async Task ShowAsync()
    {
        var filter = string.Empty;

        while (true)
        {
            ConsoleUIHelper.ClearScreen();
            ConsoleUIHelper.DrawHeader("ALL EMPLOYEES");
            
            try
            {
                var employees = await EmployeeApiClient.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    employees = employees.Where(e => 
                        e.Status.Contains(filter, StringComparison.OrdinalIgnoreCase) || 
                        e.Department.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                Console.WriteLine($"{"ID",-5} {"Name",-16} {"Department",-13} {"Status"}");
                ConsoleUIHelper.DrawSeparator();
                
                foreach (var emp in employees)
                {
                    var shortName = emp.FullName.Length > 15 ? emp.FullName.Substring(0, 12) + "..." : emp.FullName;
                    var displayStatus = emp.IsActive ? emp.Status : "Inactive";
                    Console.WriteLine($"{emp.Id,-5} {shortName,-16} {emp.Department,-13} {displayStatus}");
                }
                
                ConsoleUIHelper.DrawSeparator();
                var total = employees.Count;
                var allocated = employees.Count(e => e.Status.Equals("Allocated", StringComparison.OrdinalIgnoreCase));
                var bench = employees.Count(e => e.Status.Equals("Bench", StringComparison.OrdinalIgnoreCase));
                
                Console.WriteLine($"Total: {total}   |   Allocated: {allocated}   |   Bench: {bench}");
                Console.WriteLine();
                
                Console.WriteLine("[F] Filter by Status / Department     [B] Back");
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
