using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Employee;

public static class TimesheetHistoryScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();
            ConsoleUIHelper.DrawHeader("MY TIMESHEETS");

            try
            {
                var timesheets = await TimesheetApiClient.GetMyTimesheetsAsync();

                if (!timesheets.Any())
                {
                    Console.WriteLine("No timesheets found.");
                    Console.WriteLine();
                }
                else
                {
                    // Group by week (they should already be grouped by week from backend, 1 timesheet per week)
                    var orderedTimesheets = timesheets.OrderByDescending(t => t.WeekStartDate).ToList();

                    Console.WriteLine($"{"No.",-5} {"Week Start",-16} {"Total Hrs",-10} {"Status"}");
                    Console.WriteLine(new string('─', 50));

                    for (int i = 0; i < orderedTimesheets.Count; i++)
                    {
                        var ts = orderedTimesheets[i];
                        var statusStr = ts.Status;
                        if (ts.Status == "Missed") statusStr = "⚠ MISSED";

                        Console.WriteLine($"{i + 1,-5} {ts.WeekStartDate:dd-MMM-yy,-16} {ts.TotalHours,-10} {statusStr}");
                    }
                    Console.WriteLine(new string('─', 50));
                    Console.WriteLine();

                    Console.WriteLine("[V] View week details     [B] Back");
                    Console.WriteLine();

                    var option = ConsoleUIHelper.Prompt("Enter option").ToUpper();
                    if (option == "B") return;
                    if (option == "V")
                    {
                        var idxStr = ConsoleUIHelper.Prompt("Enter week number to view");
                        if (int.TryParse(idxStr, out var idx) && idx >= 1 && idx <= orderedTimesheets.Count)
                        {
                            var selectedTs = orderedTimesheets[idx - 1];
                            ShowWeekDetails(selectedTs);
                        }
                    }
                    continue;
                }
            }
            catch (Exception ex)
            {
                ConsoleUIHelper.ShowError(ex.Message);
            }

            Console.WriteLine("[B] Back");
            Console.WriteLine();
            var fallbackOption = ConsoleUIHelper.Prompt("Enter option").ToUpper();
            if (fallbackOption == "B") return;
        }
    }

    private static void ShowWeekDetails(TimesheetDto ts)
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader($"TIMESHEET DETAILS (Week of {ts.WeekStartDate:dd-MMM-yyyy})");
        
        Console.WriteLine($"Status: {ts.Status}");
        Console.WriteLine($"Total Hours: {ts.TotalHours}");
        Console.WriteLine();
        
        Console.WriteLine($"{"Project",-16} {"Hrs",-5} {"Activity Tags"}");
        Console.WriteLine(new string('─', 60));

        if (ts.Entries == null || !ts.Entries.Any())
        {
            Console.WriteLine("No entries found for this week.");
        }
        else
        {
            foreach (var entry in ts.Entries)
            {
                var pNameShort = entry.ProjectName.Length > 15 ? entry.ProjectName.Substring(0, 12) + "..." : entry.ProjectName;
                Console.WriteLine($"{pNameShort,-16} {entry.HoursWorked,-5} {entry.ActivityTags}");
            }
        }

        Console.WriteLine(new string('─', 60));
        Console.WriteLine();
        Console.WriteLine("Press any key to go back...");
        ConsoleUIHelper.PressAnyKey();
    }
}
