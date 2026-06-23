using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PRM.ConsoleClient.Models;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class ViewAllAllocationsScreen
{
    private readonly ApiClient _api;

    public ViewAllAllocationsScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        string filter = "";
        while (true)
        {
            Console.Clear();
            ConsoleUI.PrintHeader("ALL ALLOCATIONS");

            try
            {
                var allocations = await _api.GetAsync<List<AllocationDto>>("admin/allocations");
                if (allocations != null)
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        var lowerFilter = filter.ToLower();
                        allocations = allocations.FindAll(a => 
                            (a.UserFullName ?? "").ToLower().Contains(lowerFilter) || 
                            (a.ProjectName ?? "").ToLower().Contains(lowerFilter));
                    }

                    Console.WriteLine($"{"Resource".PadRight(20)} {"Project".PadRight(20)} {"%".PadRight(6)} {"From".PadRight(12)} {"To"}");
                    Console.WriteLine("─────────────────────────────────────────────────────────────────────────");
                    foreach (var a in allocations)
                    {
                        Console.WriteLine($"{(a.UserFullName ?? "").PadRight(20)} {(a.ProjectName ?? "").PadRight(20)} {(a.UtilisationPercent + "%").PadRight(6)} {a.FromDate:dd-MMM-yy}  {a.ToDate:dd-MMM-yy}");
                    }
                    Console.WriteLine("─────────────────────────────────────────────────────────────────────────");
                    Console.WriteLine($"\nTotal Active Allocations: {allocations.Count}");
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintError(ex.Message);
            }

            Console.WriteLine("\n[F] Filter by Resource / Project    [B] Back");
            var option = ConsoleUI.ReadInput("Enter option");

            if (option.ToUpper() == "B")
                break;
            else if (option.ToUpper() == "F")
            {
                filter = ConsoleUI.ReadInput("Enter filter text (leave empty to clear)");
            }
        }
    }
}
