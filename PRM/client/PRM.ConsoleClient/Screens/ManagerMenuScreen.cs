using System;
using System.Threading.Tasks;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class ManagerMenuScreen
{
    private readonly ApiClient _api;

    public ManagerMenuScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ConsoleUI.PrintHeader($"Welcome, {AuthState.FullName}!  |  {DateTime.Now:dd-MMM-yyyy  HH:mm}");
            Console.WriteLine("1. Resource Dashboard");
            Console.WriteLine("2. Allocate Resource");
            Console.WriteLine("3. My Projects");
            Console.WriteLine("4. Timesheets");
            Console.WriteLine("5. AI Assistant");
            Console.WriteLine("6. Logout");
            Console.WriteLine();

            string option = ConsoleUI.ReadInput("Enter option");

            switch (option)
            {
                case "1":
                    await new ResourceDashboardScreen(_api).RunAsync();
                    break;
                case "2":
                    await new AllocateResourceScreen(_api).RunAsync();
                    break;
                case "3":
                    await new MyProjectsScreen(_api).RunAsync();
                    break;
                case "4":
                    await new ManagerTimesheetsScreen(_api).RunAsync();
                    break;
                case "5":
                    await new AiAssistantScreen(_api).RunAsync();
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
