using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin.Employee;

public static class DeactivateEmployeeScreen
{
    public static async Task ShowAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("DEACTIVATE EMPLOYEE");

        var empIdStr = ConsoleUIHelper.Prompt("Enter Employee ID");
        if (!int.TryParse(empIdStr, out var empId)) return;

        try
        {
            var emp = await EmployeeApiClient.GetByIdAsync(empId);
            if (emp == null)
            {
                ConsoleUIHelper.ShowError("Employee not found.");
                ConsoleUIHelper.PressAnyKey();
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"── {emp.FullName} ".PadRight(46, '─'));
            Console.WriteLine($"Department : {emp.Department}");
            Console.WriteLine($"Status     : {(emp.IsActive ? emp.Status : "Inactive")}");

            var allAllocations = await AllocationApiClient.GetAllAsync();
            var activeAllocations = allAllocations.Where(a => a.EmployeeId == empId && a.ToDate >= DateTime.Now.Date).ToList();

            if (activeAllocations.Any())
            {
                Console.WriteLine();
                ConsoleUIHelper.ShowWarning($"This employee has {activeAllocations.Count} active allocations.");
                Console.WriteLine("   Ending their employment will remove them from:");
                foreach (var alloc in activeAllocations)
                {
                    Console.WriteLine($"     - {alloc.ProjectName,-15} ({alloc.UtilizationPercent:0}%, ends {alloc.ToDate:dd-MMM-yy})");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Are you sure you want to deactivate {emp.FullName}?");
            Console.WriteLine("This will: set is_active = false, end all active allocations today,");
            Console.WriteLine("and block their login account.");
            Console.WriteLine();
            Console.WriteLine("[Y] Yes, Deactivate     [B] Cancel");
            Console.WriteLine();
            
            var input = ConsoleUIHelper.Prompt("Enter option").ToUpper();
            if (input == "Y")
            {
                // End all active allocations
                foreach (var alloc in activeAllocations)
                {
                    await AllocationApiClient.EndAllocationAsync(alloc.Id);
                }

                // Deactivate the employee record
                await EmployeeApiClient.UpdateAsync(empId, new UpdateEmployeeDto
                {
                    FullName = emp.FullName,
                    Designation = emp.Designation,
                    Department = emp.Department,
                    TotalExperienceYears = emp.TotalExperienceYears,
                    ManagerId = emp.ManagerId,
                    Status = emp.Status,
                    IsActive = false
                });

                // Deactivate the corresponding user account to block login
                await UserApiClient.DeactivateUserAsync(emp.UserId);

                ConsoleUIHelper.ShowSuccess("Employee deactivated, allocations ended, and login blocked.");
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
