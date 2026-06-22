using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRM.ConsoleClient.Models;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class ViewMyTimesheetsScreen
{
    private readonly ApiClient _api;

    public ViewMyTimesheetsScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ConsoleUI.PrintHeader("MY TIMESHEETS");

            List<TimesheetDto> timesheets = new List<TimesheetDto>();
            try
            {
                var response = await _api.GetAsync<List<TimesheetDto>>("resource/timesheets");
                if (response != null)
                {
                    timesheets = response.OrderByDescending(t => t.WeekStartDate).ToList();
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintError($"Failed to fetch timesheets: {ex.Message}");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"{"Week Start",-15} {"Total Hrs",-12} {"Status"}");
            Console.WriteLine("──────────────────────────────────────────────");

            if (timesheets.Count == 0)
            {
                Console.WriteLine("No timesheets found.");
            }
            else
            {
                foreach (var ts in timesheets)
                {
                    decimal totalHrs = ts.Entries?.Sum(e => e.HoursWorked) ?? 0;
                    string statusStr = ts.Status.ToString();
                    if (statusStr.ToUpper() == "MISSED") statusStr += "    ⚠";
                    Console.WriteLine($"{ts.WeekStartDate,-15:dd-MMM-yyyy} {totalHrs + " hrs",-12} {statusStr}");
                }
            }
            Console.WriteLine("──────────────────────────────────────────────\n");

            string option = ConsoleUI.ReadInput("[V] View week details     [B] Back").ToUpper();
            if (option == "B") break;

            if (option == "V")
            {
                string dateStr = ConsoleUI.ReadInput("Enter week start date (DD-MM-YYYY)");
                if (DateTime.TryParseExact(dateStr, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime weekStart))
                {
                    var ts = timesheets.FirstOrDefault(t => t.WeekStartDate.Date == weekStart.Date);
                    if (ts != null)
                    {
                        ViewWeekDetails(ts);
                    }
                    else
                    {
                        ConsoleUI.PrintError("Timesheet not found for that week.");
                        Console.ReadLine();
                    }
                }
            }
        }
    }

    private void ViewWeekDetails(TimesheetDto ts)
    {
        Console.Clear();
        Console.WriteLine($"── Week: {ts.WeekStartDate:dd-MMM-yyyy} — Status: {ts.Status} ─────\n");
        Console.WriteLine($"{"Project",-16} {"Hrs",-6} {"Activity Tags"}");
        Console.WriteLine("──────────────────────────────────────────────");

        decimal totalHrs = 0;
        if (ts.Entries != null)
        {
            foreach (var entry in ts.Entries)
            {
                Console.WriteLine($"{entry.ProjectName,-16} {entry.HoursWorked,-6} {entry.ActivityTags}");
                totalHrs += entry.HoursWorked;
            }
        }
        Console.WriteLine("──────────────────────────────────────────────");
        Console.WriteLine($"Total: {totalHrs} hrs\n");

        while (true)
        {
            if (ConsoleUI.ReadInput("[B] Back").ToUpper() == "B") break;
        }
    }
}
