using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;
using ConsoleClient.Storage;

namespace ConsoleClient.Screens.Employee;

public static class SubmitTimesheetScreen
{
    public static async Task ShowAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("SUBMIT TIMESHEET");

        try
        {
            var employees = await EmployeeApiClient.GetAllAsync();
            var me = employees.FirstOrDefault(e => e.UserId == SessionManager.UserId);
            
            if (me == null)
            {
                ConsoleUIHelper.ShowError("Employee profile not found for this user.");
                ConsoleUIHelper.PressAnyKey();
                return;
            }

            var allAllocations = await AllocationApiClient.GetAllAsync();
            var myActiveAllocations = allAllocations.Where(a => a.EmployeeId == me.Id && a.ToDate >= DateTime.Now.Date).ToList();

            if (!myActiveAllocations.Any())
            {
                ConsoleUIHelper.ShowInfo("You have no active project allocations.");
                ConsoleUIHelper.PressAnyKey();
                return;
            }

            var nextFriday = DateTime.Now;
            while (nextFriday.DayOfWeek != DayOfWeek.Friday)
            {
                nextFriday = nextFriday.AddDays(1);
            }
            
            var weekStr = ConsoleUIHelper.Prompt($"Week Ending (Friday) (press Enter for {nextFriday:dd-MMM-yyyy})");
            var weekEnd = string.IsNullOrWhiteSpace(weekStr) ? nextFriday : DateTime.ParseExact(weekStr, "dd-MMM-yyyy", null);
            var weekStart = weekEnd.AddDays(-4);

            var tagsList = new[]
            {
                "Architecture", "Requirements", "Development", "Testing", "Deployment", 
                "Bug Fixing", "Support", "Documentation", "Meetings", "Training", "Bench / Internal"
            };

            var entries = new List<SubmitTimesheetEntryDto>();
            decimal totalExpectedHours = 40m; // Default max

            for (int i = 0; i < myActiveAllocations.Count; i++)
            {
                Console.WriteLine();
                var proj = myActiveAllocations[i];
                Console.WriteLine($"PROJECT {i + 1} OF {myActiveAllocations.Count} — {proj.ProjectName}");
                Console.WriteLine($"Allocation: {proj.UtilizationPercent:0}%");
                var expectedHrs = (proj.UtilizationPercent / 100m) * totalExpectedHours;
                Console.WriteLine($"Expected Hours (Max): {expectedHrs}");

                var hrsStr = ConsoleUIHelper.Prompt("Hours Worked");
                if (!decimal.TryParse(hrsStr, out var hours) || hours < 0) return;

                Console.WriteLine("Select Activity Tags (comma separated numbers):");
                for (int t = 0; t < tagsList.Length; t++)
                {
                    Console.WriteLine($"  {t + 1}. {tagsList[t]}");
                }
                var tagIdxStr = ConsoleUIHelper.Prompt("Activity Tags");
                
                var selectedTags = new List<string>();
                if (!string.IsNullOrWhiteSpace(tagIdxStr))
                {
                    var tagIndices = tagIdxStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var idx in tagIndices)
                    {
                        if (int.TryParse(idx.Trim(), out var tIdx) && tIdx >= 1 && tIdx <= tagsList.Length)
                        {
                            selectedTags.Add(tagsList[tIdx - 1]);
                        }
                    }
                }

                entries.Add(new SubmitTimesheetEntryDto
                {
                    ProjectId = proj.ProjectId,
                    HoursWorked = hours,
                    ActivityTags = string.Join(", ", selectedTags)
                });
            }

            Console.WriteLine();
            ConsoleUIHelper.DrawHeader("SUMMARY");
            Console.WriteLine($"{"Project",-16} {"Hrs",-5} {"Tags"}");
            Console.WriteLine(new string('─', 60));
            
            decimal totalHours = 0;
            foreach (var entry in entries)
            {
                var projName = myActiveAllocations.First(a => a.ProjectId == entry.ProjectId).ProjectName;
                var pNameShort = projName.Length > 15 ? projName.Substring(0, 12) + "..." : projName;
                Console.WriteLine($"{pNameShort,-16} {entry.HoursWorked,-5} {entry.ActivityTags}");
                totalHours += entry.HoursWorked;
            }
            Console.WriteLine(new string('─', 60));
            Console.WriteLine($"Total Hours: {totalHours} / {totalExpectedHours}");
            
            Console.WriteLine();
            Console.WriteLine("[S] Submit     [B] Cancel");
            Console.WriteLine();

            var option = ConsoleUIHelper.Prompt("Enter option").ToUpper();
            if (option == "S")
            {
                await TimesheetApiClient.SubmitTimesheetAsync(new SubmitTimesheetDto
                {
                    EmployeeId = me.Id,
                    WeekStartDate = weekStart,
                    Entries = entries
                });
                ConsoleUIHelper.ShowSuccess("Timesheet submitted successfully!");
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
