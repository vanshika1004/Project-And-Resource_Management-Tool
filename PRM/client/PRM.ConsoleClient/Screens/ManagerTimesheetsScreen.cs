using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class ManagerTimesheetsScreen
{
    private readonly ApiClient _api;

    public ManagerTimesheetsScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║    TIMESHEETS — MY TEAM                      ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝\n");
        Console.WriteLine("Filter by week (DD-MM-YYYY) or press Enter for current week:");
        var weekInput = ConsoleUI.ReadInput("Week");
        
        DateTime weekStart;
        if (string.IsNullOrWhiteSpace(weekInput))
        {
            var diff = (7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7;
            weekStart = DateTime.Today.AddDays(-1 * diff).Date;
        }
        else
        {
            if (!DateTime.TryParse(weekInput, out weekStart))
            {
                ConsoleUI.PrintError("Invalid date format.");
                Console.ReadLine();
                return;
            }
        }

        List<JsonElement> timesheets = null;
        try
        {
            timesheets = await _api.GetAsync<List<JsonElement>>($"manager/team-timesheets?weekStartDate={weekStart:yyyy-MM-dd}");
            if (timesheets != null && timesheets.Count > 0)
            {
                Console.WriteLine("\n────────────────────────────────────────────────");
                Console.WriteLine($"{"Resource".PadRight(16)} {"Project".PadRight(16)} {"Hrs".PadRight(8)} {"Status"}");
                Console.WriteLine("────────────────────────────────────────────────");
                foreach (var t in timesheets)
                {
                    var name = t.GetProperty("userFullName").GetString() ?? "";
                    var status = t.GetProperty("status").GetString() ?? "";
                    if (status.ToUpper() == "MISSED") status += " ⚠";

                    if (t.TryGetProperty("entries", out JsonElement entries) && entries.ValueKind == JsonValueKind.Array && entries.GetArrayLength() > 0)
                    {
                        foreach (var entry in entries.EnumerateArray())
                        {
                            var project = entry.GetProperty("projectName").GetString() ?? "";
                            var hrs = entry.GetProperty("hoursWorked").GetDecimal().ToString("0.##");
                            Console.WriteLine($"{name.PadRight(16)} {project.PadRight(16)} {hrs.PadRight(8)} {status}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{name.PadRight(16)} {"-".PadRight(16)} {"0".PadRight(8)} {status}");
                    }
                }
                Console.WriteLine("────────────────────────────────────────────────\n");
            }
            else
            {
                Console.WriteLine("\nNo timesheets found for this week.\n");
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError($"Failed to load timesheets: {ex.Message}");
        }

        var option = ConsoleUI.ReadInput("[V] View timesheet detail    [U] Unfreeze Resource    [B] Back");
        if (option.ToUpper() == "U")
        {
            var empIdStr = ConsoleUI.ReadInput("Enter Resource ID to unfreeze");
            if (int.TryParse(empIdStr, out int empId))
            {
                try
                {
                    await _api.PutAsync<object>($"manager/team/{empId}/unfreeze", new { });
                    ConsoleUI.PrintSuccess("Resource timesheet access unfrozen successfully.");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    ConsoleUI.PrintError($"Failed to unfreeze: {ex.Message}");
                    Console.ReadLine();
                }
            }
        }
        else if (option.ToUpper() == "V")
        {
            var empIdStr = ConsoleUI.ReadInput("Enter Resource ID to view");
            if (int.TryParse(empIdStr, out int empId))
            {
                if (timesheets != null)
                {
                    bool found = false;
                    foreach (var t in timesheets)
                    {
                        if (t.GetProperty("userId").GetInt32() == empId)
                        {
                            var name = t.GetProperty("userFullName").GetString() ?? "Unknown";
                            Console.WriteLine($"\n── Timesheet Detail: {name} ────────────────────");
                            
                            if (t.TryGetProperty("entries", out JsonElement entries) && entries.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var entry in entries.EnumerateArray())
                                {
                                    var project = entry.GetProperty("projectName").GetString() ?? "";
                                    var hrs = entry.GetProperty("hoursWorked").GetDecimal().ToString("0.##");
                                    var tags = entry.GetProperty("activityTags").GetString() ?? "None";
                                    Console.WriteLine($"Project : {project}");
                                    Console.WriteLine($"Hours   : {hrs}");
                                    Console.WriteLine($"Tags    : {tags}\n");
                                }
                            }
                            else
                            {
                                Console.WriteLine("No entries logged.");
                            }
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        ConsoleUI.PrintError("Timesheet not found for this Resource ID in the current week.");
                    }
                }
                Console.ReadLine();
            }
        }
    }
}
