using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRM.ConsoleClient.Models;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class SubmitTimesheetScreen
{
    private readonly ApiClient _api;

    public SubmitTimesheetScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        ConsoleUI.PrintHeader("SUBMIT TIMESHEET");
        Console.WriteLine($"Resource  : {AuthState.FullName}");

        DateTime today = DateTime.Today;
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        DateTime lastMonday = today.AddDays(-diff).AddDays(-7).Date;

        string dateInput = ConsoleUI.ReadInput($"Week Start: Enter date (DD-MM-YYYY) or press Enter for last Monday\n          > ");
        DateTime weekStart = lastMonday;

        if (!string.IsNullOrWhiteSpace(dateInput))
        {
            if (!DateTime.TryParseExact(dateInput, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out weekStart))
            {
                ConsoleUI.PrintError("Invalid date format.");
                return;
            }
        }

        Console.WriteLine("\nChecking your active allocations for this week...\n");

        List<AllocationDto> allocations = new List<AllocationDto>();
        try
        {
            var allAllocations = await _api.GetAsync<List<AllocationDto>>("resource/allocations");
            if (allAllocations != null)
            {
                allocations = allAllocations.Where(a => a.FromDate <= weekStart.AddDays(6) && a.ToDate >= weekStart).ToList();
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError($"Failed to fetch allocations: {ex.Message}");
            return;
        }

        if (allocations.Count == 0)
        {
            Console.WriteLine("No active allocations found for this week.");
            Console.WriteLine("\n[B] Back");
            ConsoleUI.ReadInput("Enter option");
            return;
        }

        List<CreateTimesheetEntryRequest> entries = new List<CreateTimesheetEntryRequest>();
        decimal totalHours = 0;
        int maxWeeklyHours = 40; // Default

        int projectIndex = 1;
        List<string> summaryLines = new List<string>();

        foreach (var alloc in allocations)
        {
            Console.WriteLine("──────────────────────────────────────────────");
            decimal expectedMax = (alloc.UtilisationPercent / 100m) * maxWeeklyHours;
            Console.WriteLine($"PROJECT {projectIndex} OF {allocations.Count} — {alloc.ProjectName}");
            Console.WriteLine($"  Allocation: {alloc.UtilisationPercent}%   |   Expected: {expectedMax:0.##} hrs max");
            Console.WriteLine("──────────────────────────────────────────────");

            string hoursInput = ConsoleUI.ReadInput("Hours worked this week");
            if (!decimal.TryParse(hoursInput, out decimal hours)) hours = 0;

            string activityTags = "";
            if (hours > 0)
            {
                Console.WriteLine("\nWhat did you work on? Select activity tags:\n");
                Console.WriteLine("  1.  Backend API Development");
                Console.WriteLine("  2.  Microservices / Architecture");
                Console.WriteLine("  3.  Database Design & Queries");
                Console.WriteLine("  4.  WebSocket / Real-time Features");
                Console.WriteLine("  5.  Frontend Development");
                Console.WriteLine("  6.  Code Review / Mentoring");
                Console.WriteLine("  7.  Bug Fixing");
                Console.WriteLine("  8.  DevOps / Deployment");
                Console.WriteLine("  9.  Testing & QA");
                Console.WriteLine("  10. Documentation");
                Console.WriteLine("  11. Other (type manually)\n");

                string tagsInput = ConsoleUI.ReadInput("Select tags (comma-separated)");
                
                var tagParts = tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                List<string> selectedTags = new List<string>();
                
                foreach(var part in tagParts)
                {
                    if (part == "1") selectedTags.Add("Backend API");
                    else if (part == "2") selectedTags.Add("Microservices");
                    else if (part == "3") selectedTags.Add("Database Design");
                    else if (part == "4") selectedTags.Add("WebSocket");
                    else if (part == "5") selectedTags.Add("Frontend Development");
                    else if (part == "6") selectedTags.Add("Code Review");
                    else if (part == "7") selectedTags.Add("Bug Fixing");
                    else if (part == "8") selectedTags.Add("DevOps");
                    else if (part == "9") selectedTags.Add("Testing");
                    else if (part == "10") selectedTags.Add("Documentation");
                    else if (part == "11") selectedTags.Add(ConsoleUI.ReadInput("Enter custom tag"));
                    else selectedTags.Add(part);
                }

                activityTags = string.Join(", ", selectedTags);
                entries.Add(new CreateTimesheetEntryRequest(alloc.ProjectId, hours, activityTags));
                summaryLines.Add($"  {alloc.ProjectName,-15} {hours} hrs    [{activityTags}]");
                totalHours += hours;
            }

            projectIndex++;
            Console.WriteLine();
        }

        Console.WriteLine("──────────────────────────────────────────────");
        Console.WriteLine("SUMMARY");
        foreach(var line in summaryLines)
        {
            Console.WriteLine(line);
        }
        Console.WriteLine("  ─────────────────────────────────────────");
        Console.WriteLine($"  Total           {totalHours} hrs / {maxWeeklyHours} hrs max   " + (totalHours <= maxWeeklyHours ? "✓" : "⚠ OVER"));
        Console.WriteLine("\n──────────────────────────────────────────────");

        string confirm = ConsoleUI.ReadInput("[S] Submit Timesheet     [B] Back").ToUpper();
        if (confirm == "S")
        {
            try
            {
                var req = new SubmitTimesheetRequest(AuthState.UserId, weekStart, entries);
                await _api.PostAsync("resource/timesheets", req);
                ConsoleUI.PrintSuccess("Timesheet submitted successfully. Status: SUBMITTED ✓");
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintError($"Failed to submit timesheet: {ex.Message}");
                Console.ReadLine();
            }
        }
    }
}
