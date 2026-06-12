using System;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin.Employee;

public static class AssignManagerScreen
{
    public static async Task ShowAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("ASSIGN MANAGER");

        var empUserIdStr = ConsoleUIHelper.Prompt("Employee User ID");
        var managerUserIdStr = ConsoleUIHelper.Prompt("Manager User ID");

        ConsoleUIHelper.DrawSeparator();
        Console.WriteLine("[S] Save     [B] Back");
        Console.WriteLine();
        
        var option = ConsoleUIHelper.Prompt("Enter option").ToUpper();
        if (option == "B") return;

        if (option == "S")
        {
            if (!int.TryParse(empUserIdStr, out var empId) || !int.TryParse(managerUserIdStr, out var managerId))
            {
                ConsoleUIHelper.ShowError("Invalid IDs.");
                ConsoleUIHelper.PressAnyKey();
                return;
            }

            try
            {
                var emp = await EmployeeApiClient.GetByIdAsync(empId);
                if (emp == null)
                {
                    ConsoleUIHelper.ShowError("Employee not found.");
                    ConsoleUIHelper.PressAnyKey();
                    return;
                }

                await EmployeeApiClient.UpdateAsync(empId, new UpdateEmployeeDto
                {
                    FullName = emp.FullName,
                    Designation = emp.Designation,
                    Department = emp.Department,
                    TotalExperienceYears = emp.TotalExperienceYears,
                    ManagerId = managerId,
                    Status = emp.Status
                });
                
                ConsoleUIHelper.ShowSuccess("Manager assigned successfully!");
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
