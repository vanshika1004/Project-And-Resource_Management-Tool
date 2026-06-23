using System;
using System.Threading.Tasks;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class AdminMenuScreen
{
    private readonly ApiClient _api;

    public AdminMenuScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ConsoleUI.PrintHeader("ADMIN PANEL", $"Welcome, {AuthState.FullName}  |  {DateTime.Now:dd-MM-yyyy  HH:mm}");
            Console.WriteLine("1. Manage Resources");
            Console.WriteLine("2. Manage Projects");
            Console.WriteLine("3. View All Allocations");
            Console.WriteLine("4. Manage Users");
            Console.WriteLine("5. System Configuration");
            Console.WriteLine("6. Logout");
            Console.WriteLine();

            string option = ConsoleUI.ReadInput("Enter option");

            switch (option)
            {
                case "1":
                    await new ManageEmployeesScreen(_api).RunAsync();
                    break;
                case "2":
                    await new ManageProjectsScreen(_api).RunAsync();
                    break;
                case "3":
                    await new ViewAllAllocationsScreen(_api).RunAsync();
                    break;
                case "4":
                    await new ManageUsersScreen(_api).RunAsync();
                    break;
                case "5":
                    await new SystemConfigScreen(_api).RunAsync();
                    break;
                case "6":
                    AuthState.Clear();
                    return; 
                default:
                    ConsoleUI.PrintError("Invalid option. Please try again.");
                    break;
            }
        }
    }
}
