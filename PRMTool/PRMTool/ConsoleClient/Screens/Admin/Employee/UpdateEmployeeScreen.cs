using System;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin.Employee;

public static class UpdateEmployeeScreen
{
    public static async Task ShowAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("UPDATE EMPLOYEE");

        if (!int.TryParse(ConsoleUIHelper.Prompt("Enter Employee ID to update"), out var empId)) return;

        try
        {
            var emp = await EmployeeApiClient.GetByIdAsync(empId);
            if (emp == null)
            {
                ConsoleUIHelper.ShowError("Employee not found.");
                ConsoleUIHelper.PressAnyKey();
                return;
            }

            Console.WriteLine($"\n── {emp.FullName} ─────────────────────────────────");

            var fullName = ConsoleUIHelper.Prompt($"Full Name ({emp.FullName})");
            var designation = ConsoleUIHelper.Prompt($"Designation ({emp.Designation})");
            var department = ConsoleUIHelper.Prompt($"Department ({emp.Department})");

            await EmployeeApiClient.UpdateAsync(empId, new UpdateEmployeeDto
            {
                FullName = string.IsNullOrWhiteSpace(fullName) ? emp.FullName : fullName,
                Designation = string.IsNullOrWhiteSpace(designation) ? emp.Designation : designation,
                Department = string.IsNullOrWhiteSpace(department) ? emp.Department : department,
                ManagerId = emp.ManagerId,
                Status = emp.Status
            });
            
            ConsoleUIHelper.ShowSuccess("Employee updated successfully!");
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError(ex.Message);
        }

        ConsoleUIHelper.PressAnyKey();
    }
}
