using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PRM.ConsoleClient.Models;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class ResourceDashboardScreen
{
    private readonly ApiClient _api;

    public ResourceDashboardScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ConsoleUI.PrintHeader($"RESOURCE DASHBOARD - {DateTime.Now:MMM yyyy}");
            
            try
            {
                var members = await _api.GetAsync<List<TeamDashboardMemberDto>>("manager/team-dashboard");
                if (members != null)
                {
                    var onBench = members.Where(m => m.AllocPercent == 0).ToList();
                    var active = members.Where(m => m.AllocPercent > 0).ToList();

                    Console.WriteLine($"ON BENCH   ({onBench.Count} Resources available)");
                    Console.WriteLine("─────────────────────────────────────────────────────");
                    Console.WriteLine($"{"ID",-4} {"Name",-16} {"Department",-13} {"Skills"}");
                    foreach (var m in onBench)
                    {
                        string dept = m.Department ?? "-";
                        Console.WriteLine($"{m.Id,-4} {m.FullName,-16} {dept,-13} {m.Skills}");
                    }
                    Console.WriteLine();

                    Console.WriteLine("ACTIVE ResourceS");
                    Console.WriteLine("─────────────────────────────────────────────────────");
                    Console.WriteLine($"{"ID",-4} {"Name",-16} {"Alloc %",-9} {"Availability"}");
                    foreach (var m in active)
                    {
                        string avail = m.AllocPercent >= 100 ? "FULL" : $"{100 - m.AllocPercent}% free";
                        Console.WriteLine($"{m.Id,-4} {m.FullName,-16} {m.AllocPercent,3}%      {avail}");
                    }
                    Console.WriteLine("─────────────────────────────────────────────────────");
                    Console.WriteLine($"Bench: {onBench.Count}      |     Partial: {active.Count(a => a.AllocPercent < 100)}\n");
                }
                else
                {
                    Console.WriteLine("No team members found.\n");
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintError($"Failed to load dashboard: {ex.Message}");
            }

            Console.WriteLine("[D] Drill into Resource details       [B] Back\n");
            
            string option = ConsoleUI.ReadInput("Enter option").ToUpper();
            if (option == "B") break;
            
            if (option == "D")
            {
                string idStr = ConsoleUI.ReadInput("Enter Resource ID");
                if (int.TryParse(idStr, out int empId))
                {
                    await ShowResourceDetailsAsync(empId);
                }
            }
        }
    }

    private async Task ShowResourceDetailsAsync(int empId)
    {
        Console.Clear();
        try
        {
            var details = await _api.GetAsync<TeamMemberDetailsDto>($"manager/team/{empId}");
            if (details != null)
            {
                Console.WriteLine($"\n── {details.FullName} ───────────────────────────────────────");
                Console.WriteLine($"Department      : {details.Department ?? "-"}");
                Console.WriteLine($"Current Status  : {details.Status}");
                Console.WriteLine($"Profile Skills  : {details.Skills}\n");

                Console.WriteLine("Active Allocations:");
                if (details.ActiveAllocations != null && details.ActiveAllocations.Count > 0)
                {
                    Console.WriteLine($"  {"Project",-14} {"%",-6} {"From",-11} {"To"}");
                    foreach (var a in details.ActiveAllocations)
                    {
                        Console.WriteLine($"  {a.ProjectName,-14} {a.Percent}%    {a.FromDate:dd-MMM-yy}   {a.ToDate:dd-MMM-yy}");
                    }
                }
                else
                {
                    Console.WriteLine("  None");
                }
                Console.WriteLine();

                Console.WriteLine("Recent Activity Tags (last 4 weeks):");
                if (details.RecentActivityTags != null && details.RecentActivityTags.Count > 0)
                {
                    Console.WriteLine($"  {string.Join(", ", details.RecentActivityTags)}");
                }
                else
                {
                    Console.WriteLine("  None");
                }
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError($"Failed to load Resource details: {ex.Message}");
        }

        Console.WriteLine("[B] Back");
        while (true)
        {
            string opt = ConsoleUI.ReadInput("Enter option").ToUpper();
            if (opt == "B") break;
        }
    }
}
