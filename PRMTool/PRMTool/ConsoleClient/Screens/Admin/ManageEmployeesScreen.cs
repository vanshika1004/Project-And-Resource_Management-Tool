using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;
using ConsoleClient.Screens.Admin.Employee;

namespace ConsoleClient.Screens.Admin;

public static class ManageEmployeesScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();
            ConsoleUIHelper.DrawHeader("MANAGE EMPLOYEES");

            var choice = ConsoleUIHelper.ShowMenu(new[]
            {
                "View All Employees",
                "Update Employee",
                "Deactivate Employee",
                "Manage Employee Skills",
                "Assign Manager",
                "Back"
            });

            switch (choice)
            {
                case 1:
                    await ViewAllEmployeesScreen.ShowAsync();
                    break;
                case 2:
                    await UpdateEmployeeScreen.ShowAsync();
                    break;
                case 3:
                    await DeactivateEmployeeScreen.ShowAsync();
                    break;
                case 4:
                    await ManageEmployeeSkillsScreen.ShowAsync();
                    break;
                case 5:
                    await AssignManagerScreen.ShowAsync();
                    break;
                case 6:
                    return;
                default:
                    ConsoleUIHelper.ShowError("Invalid option.");
                    ConsoleUIHelper.PressAnyKey();
                    break;
            }
        }
    }
}
