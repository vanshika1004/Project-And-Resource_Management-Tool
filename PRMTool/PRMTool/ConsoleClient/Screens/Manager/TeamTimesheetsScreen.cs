using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Manager;

public static class TeamTimesheetsScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();
            ConsoleUIHelper.DrawHeader("TEAM TIMESHEETS");
            
            try
            {
                var timesheets = await TimesheetApiClient.GetTeamTimesheetsAsync();
                
                var weekEndings = timesheets
                    .Select(t => t.WeekStartDate.AddDays(4))
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToList();
                
                if (!weekEndings.Any())
                {
                    ConsoleUIHelper.ShowInfo("No timesheets found for your team.");
                    ConsoleUIHelper.PressAnyKey();
                    return;
                }

                Console.WriteLine("Select Week Ending (Friday)");
                for (int i = 0; i < Math.Min(3, weekEndings.Count); i++)
                {
                    Console.WriteLine($"{i + 1}. {weekEndings[i]:dd-MMM-yy}");
                }
                var backIndex = Math.Min(3, weekEndings.Count) + 1;
                Console.WriteLine($"{backIndex}. Back");
                
                var optStr = ConsoleUIHelper.Prompt("Enter choice");
                if (!int.TryParse(optStr, out var choice) || choice < 1 || choice > backIndex)
                {
                    ConsoleUIHelper.ShowError("Invalid choice.");
                    ConsoleUIHelper.PressAnyKey();
                    continue;
                }
                
                if (choice == backIndex) return;

                var selectedWeekEnd = weekEndings[choice - 1];
                var selectedWeekStart = selectedWeekEnd.AddDays(-4);

                var weekTimesheets = timesheets.Where(t => t.WeekStartDate.Date == selectedWeekStart.Date).ToList();

                await ShowWeekListAsync(weekTimesheets, selectedWeekEnd);
            }
            catch (Exception ex)
            {
                ConsoleUIHelper.ShowError(ex.Message);
                ConsoleUIHelper.PressAnyKey();
                return;
            }
        }
    }

    private static async Task ShowWeekListAsync(System.Collections.Generic.List<TimesheetDto> weekTimesheets, DateTime selectedWeekEnd)
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();
            ConsoleUIHelper.DrawHeader("TEAM TIMESHEETS");
            Console.WriteLine();
            Console.WriteLine($"── Timesheets for Week Ending {selectedWeekEnd:dd-MMM-yy} ".PadRight(48, '─'));
            Console.WriteLine();

            Console.WriteLine($"{"No.",-4} {"Employee",-15} {"Hrs",-5} {"Status"}");
            Console.WriteLine(new string('─', 48));

            var totalHrs = 0m;
            for (int i = 0; i < weekTimesheets.Count; i++)
            {
                var ts = weekTimesheets[i];
                var empName = ts.EmployeeName.Length > 14 ? ts.EmployeeName.Substring(0, 11) + "..." : ts.EmployeeName;
                Console.WriteLine($"{i + 1,-4} {empName,-15} {ts.TotalHours,-5} {ts.Status}");
                totalHrs += ts.TotalHours;
            }

            Console.WriteLine(new string('─', 48));
            Console.WriteLine($"Total Hours: {totalHrs}");
            Console.WriteLine();

            Console.WriteLine("[V] View Detail     [B] Back");
            Console.WriteLine();

            var action = ConsoleUIHelper.Prompt("Enter option").ToUpper();
            if (action == "B") return;
            if (action == "V")
            {
                var idxStr = ConsoleUIHelper.Prompt("Enter timesheet number to view");
                if (int.TryParse(idxStr, out var idx) && idx >= 1 && idx <= weekTimesheets.Count)
                {
                    var selectedTs = weekTimesheets[idx - 1];
                    await ShowTimesheetDetailAsync(selectedTs);
                }
            }
        }
    }

    private static async Task ShowTimesheetDetailAsync(TimesheetDto ts)
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader($"TIMESHEET DETAIL - {ts.EmployeeName}");
        
        Console.WriteLine($"Week Ending: {ts.WeekStartDate.AddDays(4):dd-MMM-yyyy}");
        Console.WriteLine($"Status:      {ts.Status}");
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
        Console.WriteLine($"Total Hours: {ts.TotalHours}");
        Console.WriteLine();

        if (ts.Status == "Submitted")
        {
            Console.WriteLine("[A] Approve     [F] Flag     [B] Back");
        }
        else
        {
            Console.WriteLine("[B] Back");
        }

        Console.WriteLine();

        var action = ConsoleUIHelper.Prompt("Enter option").ToUpper();
        if (action == "B") return;
        if (ts.Status == "Submitted")
        {
            if (action == "A")
            {
                await TimesheetApiClient.UpdateStatusAsync(ts.Id, "Approved");
                ts.Status = "Approved";
                ConsoleUIHelper.ShowSuccess("Timesheet approved.");
                ConsoleUIHelper.PressAnyKey();
            }
            else if (action == "F")
            {
                await TimesheetApiClient.UpdateStatusAsync(ts.Id, "Flagged");
                ts.Status = "Flagged";
                ConsoleUIHelper.ShowSuccess("Timesheet flagged.");
                ConsoleUIHelper.PressAnyKey();
            }
        }
    }
}
