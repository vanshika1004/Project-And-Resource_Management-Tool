using System;
using System.Threading.Tasks;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class EmployeeMenuScreen
{
    private readonly ApiClient _api;

    public EmployeeMenuScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ConsoleUI.PrintHeader($"Welcome, {AuthState.FullName}!  |  {DateTime.Now:dd-MMM-yyyy  HH:mm}");

            // Check for missing timesheet for most recent completed week (last Monday)
            DateTime today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime lastMonday = today.AddDays(-diff).AddDays(-7).Date;

            try
            {
                var timesheets = await _api.GetAsync<System.Collections.Generic.List<PRM.ConsoleClient.Models.TimesheetDto>>("resource/timesheets");
                if (timesheets != null)
                {
                    bool submitted = false;
                    foreach (var ts in timesheets)
                    {
                        if (ts.WeekStartDate.Date == lastMonday)
                        {
                            submitted = true;
                            break;
                        }
                    }

                    if (!submitted)
                    {
                        Console.WriteLine($"  \u26A0  Reminder: Timesheet for week {lastMonday:dd-MMM-yyyy} has not been submitted.");
                        Console.WriteLine("\n──────────────────────────────────────────────");
                    }
                }
            }
            catch
            {
                // Ignore API error for reminder
            }

            Console.WriteLine("1. Submit Timesheet");
            Console.WriteLine("2. View My Timesheets");
            Console.WriteLine("3. View My Allocations");
            Console.WriteLine("4. Logout");
            Console.WriteLine();

            string option = ConsoleUI.ReadInput("Enter option");

            switch (option)
            {
                case "1":
                    await new SubmitTimesheetScreen(_api).RunAsync();
                    break;
                case "2":
                    await new ViewMyTimesheetsScreen(_api).RunAsync();
                    break;
                case "3":
                    await new ViewMyAllocationsScreen(_api).RunAsync();
                    break;
                case "4":
                    AuthState.Clear();
                    return;
                default:
                    ConsoleUI.PrintError("Invalid option. Please try again.");
                    break;
            }
        }
    }
}
