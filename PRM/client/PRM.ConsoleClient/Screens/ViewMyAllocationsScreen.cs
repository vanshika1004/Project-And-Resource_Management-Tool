using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRM.ConsoleClient.Models;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class ViewMyAllocationsScreen
{
    private readonly ApiClient _api;

    public ViewMyAllocationsScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        ConsoleUI.PrintHeader("MY ALLOCATIONS");

        List<AllocationDto> allocations = new List<AllocationDto>();
        try
        {
            var response = await _api.GetAsync<List<AllocationDto>>("resource/allocations");
            if (response != null)
            {
                allocations = response.OrderBy(a => a.FromDate).ToList();
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError($"Failed to fetch allocations: {ex.Message}");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"{"Project",-17} {"%",-6} {"From",-12} {"To",-12} {"Status"}");
        Console.WriteLine("──────────────────────────────────────────────────────────");

        int totalUtilisation = 0;
        DateTime today = DateTime.Today;

        if (allocations.Count == 0)
        {
            Console.WriteLine("No allocations found.");
        }
        else
        {
            foreach (var a in allocations)
            {
                string status = "ACTIVE";
                if (a.ToDate < today) status = "COMPLETED";
                else if (a.FromDate > today) status = "PLANNED";

                if (status == "ACTIVE")
                {
                    totalUtilisation += a.UtilisationPercent;
                }

                Console.WriteLine($"{a.ProjectName,-17} {a.UtilisationPercent + "%",-6} {a.FromDate,-12:dd-MMM-yy} {a.ToDate,-12:dd-MMM-yy} {status}");
            }
        }
        Console.WriteLine("──────────────────────────────────────────────────────────");
        Console.WriteLine($"Total Utilisation: {totalUtilisation}%\n");

        while (true)
        {
            if (ConsoleUI.ReadInput("[B] Back").ToUpper() == "B") break;
        }
    }
}
