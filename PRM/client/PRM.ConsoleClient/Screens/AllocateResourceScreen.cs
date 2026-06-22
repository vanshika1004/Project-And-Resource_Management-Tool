using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class AllocateResourceScreen
{
    private readonly ApiClient _api;

    public AllocateResourceScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ConsoleUI.PrintHeader("ALLOCATE RESOURCE");
            Console.WriteLine("1. Find resource using AI (recommended)");
            Console.WriteLine("2. Allocate directly (I already know who I want)");
            Console.WriteLine("3. End an existing allocation");
            Console.WriteLine("4. Team Builder");
            Console.WriteLine("5. Back");
            var option = ConsoleUI.ReadInput("Enter option");

            switch (option)
            {
                case "1":
                    await AiSkillMatchAsync();
                    break;
                case "2":
                    await DirectAllocationAsync();
                    break;
                case "3":
                    await EndAllocationAsync();
                    break;
                case "4":
                    var tbScreen = new TeamBuilderScreen(_api);
                    await tbScreen.RunAsync();
                    break;
                case "5":
                    return;
                default:
                    ConsoleUI.PrintError("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private async Task AiSkillMatchAsync()
    {
        ConsoleUI.PrintHeader("ALLOCATE RESOURCE");
        
        Console.WriteLine("Step 1 — Select Project");
        var projectIdStr = ConsoleUI.ReadInput("Enter project name or ID");
        if (string.IsNullOrWhiteSpace(projectIdStr)) return;

        int projectId = 0;
        int.TryParse(projectIdStr, out projectId);

        Console.WriteLine("\nStep 2 — Describe your requirement");
        var req = ConsoleUI.ReadInput("Type what kind of resource you need (e.g., '10 hrs/week, UI testing')");
        if (string.IsNullOrWhiteSpace(req)) return;

        try
        {
            Console.WriteLine("\nSearching... (AI matching in progress)\n");
            var result = await _api.PostAsync<object, JsonElement>("manager/ai/skill-match", new { requirement = req, projectId = projectId });
            
            Console.WriteLine("──────────────────────────────────────────────");
            Console.WriteLine("AI-MATCHED RESULTS");
            Console.WriteLine("──────────────────────────────────────────────");
            Console.WriteLine(result.GetProperty("summary").GetString());
            Console.WriteLine("\nNote: Suggestions are AI-generated. Verify before confirming.");
            Console.WriteLine("──────────────────────────────────────────────\n");
            
            var empIdStr = ConsoleUI.ReadInput("Enter Resource ID to select (e.g., 1004), or 0 to search again");
            if (empIdStr == "0" || string.IsNullOrWhiteSpace(empIdStr)) return;

            if (int.TryParse(empIdStr, out int userId))
            {
                await ProcessAllocationForResourceAsync(userId, projectId);
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError($"Skill match failed: {ex.Message}");
            Console.ReadLine();
        }
    }

    private async Task ProcessAllocationForResourceAsync(int userId, int projectId)
    {
        Console.WriteLine($"\n── Resource {userId} ─────────────────────────────────");
        Console.WriteLine("Set Allocation:");
        var utilStr = ConsoleUI.ReadInput("  Utilisation %   ");
        var fromDateStr = ConsoleUI.ReadInput("  From Date       ");
        var toDateStr = ConsoleUI.ReadInput("  To Date         ");

        Console.WriteLine("\nValidating...");
        if (int.TryParse(utilStr, out int utilPercent) &&
            DateTime.TryParse(fromDateStr, out DateTime fromDate) &&
            DateTime.TryParse(toDateStr, out DateTime toDate))
        {
            var confirm = ConsoleUI.ReadInput("[C] Confirm Allocation     [B] Back");
            if (confirm.ToUpper() == "C")
            {
                try
                {
                    var payload = new
                    {
                        UserId = userId,
                        ProjectId = projectId,
                        FromDate = fromDate,
                        ToDate = toDate,
                        UtilisationPercent = utilPercent
                    };
                    await _api.PostAsync("manager/allocations", payload);
                    ConsoleUI.PrintSuccess($"Allocation saved. Resource {userId} → Project {projectId} ({utilPercent}%) ✓");
                }
                catch (Exception ex)
                {
                    ConsoleUI.PrintError($"Allocation failed: {ex.Message}");
                }
            }
        }
        else
        {
            ConsoleUI.PrintError("Invalid input formats.");
        }
        Console.ReadLine();
    }

    private async Task DirectAllocationAsync()
    {
        ConsoleUI.PrintHeader("Direct Allocation");
        var userIdStr = ConsoleUI.ReadInput("Enter Resource ID");
        var projectIdStr = ConsoleUI.ReadInput("Enter Project ID");
        var fromDateStr = ConsoleUI.ReadInput("Enter From Date (YYYY-MM-DD)");
        var toDateStr = ConsoleUI.ReadInput("Enter To Date (YYYY-MM-DD)");
        var utilStr = ConsoleUI.ReadInput("Enter Utilisation Percent (1-100)");

        if (int.TryParse(userIdStr, out int userId) && 
            int.TryParse(projectIdStr, out int projectId) && 
            DateTime.TryParse(fromDateStr, out DateTime fromDate) && 
            DateTime.TryParse(toDateStr, out DateTime toDate) && 
            int.TryParse(utilStr, out int utilPercent))
        {
            try
            {
                var payload = new
                {
                    UserId = userId,
                    ProjectId = projectId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    UtilisationPercent = utilPercent
                };
                await _api.PostAsync("manager/allocations", payload);
                ConsoleUI.PrintSuccess("Allocation successful!");
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintError($"Allocation failed: {ex.Message}");
            }
        }
        else
        {
            ConsoleUI.PrintError("Invalid input formats.");
        }
        Console.ReadLine();
    }

    private async Task EndAllocationAsync()
    {
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║    END ALLOCATION                            ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝\n");
        
        try
        {
            var projects = await _api.GetAsync<List<JsonElement>>("manager/projects");
            if (projects == null || projects.Count == 0)
            {
                Console.WriteLine("No projects found.");
                Console.ReadLine();
                return;
            }

            var projOpt = ConsoleUI.ReadInput("Select Project");
            if (string.IsNullOrWhiteSpace(projOpt)) return;

            JsonElement selectedProject = default;
            bool found = false;

            foreach (var project in projects)
            {
                var name = project.GetProperty("name").GetString();
                var id = project.GetProperty("id").GetInt32();
                if (projOpt == id.ToString() || string.Equals(name, projOpt, StringComparison.OrdinalIgnoreCase) || projOpt.Contains(name!) || projOpt.Contains(id.ToString()))
                {
                    selectedProject = project;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Console.WriteLine("Project not found.");
                Console.ReadLine();
                return;
            }

            var projectId = selectedProject.GetProperty("id").GetInt32();
            var projectName = selectedProject.GetProperty("name").GetString();

            Console.WriteLine($"\nActive Allocations on this project:");
            
            var projectDetails = await _api.GetAsync<JsonElement>($"manager/projects/{projectId}");
            var projectAllocations = new List<dynamic>();
            
            if (projectDetails.TryGetProperty("allocations", out JsonElement allocs) && allocs.ValueKind == JsonValueKind.Array)
            {
                foreach (var a in allocs.EnumerateArray())
                {
                    var toDate = a.GetProperty("toDate").GetDateTime();
                    if (toDate >= DateTime.Today)
                    {
                        projectAllocations.Add(new {
                            AllocId = a.GetProperty("id").GetInt32(),
                            Name = a.GetProperty("userFullName").GetString(),
                            Percent = a.GetProperty("utilisationPercent").GetInt32(),
                            From = a.GetProperty("fromDate").GetDateTime(),
                            To = toDate
                        });
                    }
                }
            }

            if (!projectAllocations.Any())
            {
                Console.WriteLine("  No resources currently allocated to this project.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"{"#".PadRight(4)} {"Resource".PadRight(15)} {"%".PadRight(6)} {"From".PadRight(11)} {"To"}");
            for (int index = 0; index < projectAllocations.Count; index++)
            {
                var currentAllocation = projectAllocations[index];
                Console.WriteLine($"{(index + 1).ToString() + "."}    {currentAllocation.Name.PadRight(15)} {$"{currentAllocation.Percent}%".PadRight(6)} {currentAllocation.From.ToString("dd-MMM-yy").PadRight(11)} {currentAllocation.To.ToString("dd-MMM-yy")}");
            }
            Console.WriteLine("────────────────────────────────────────────────\n");

            var allocOpt = ConsoleUI.ReadInput("Select allocation to end");
            if (!int.TryParse(allocOpt, out int allocIdx) || allocIdx < 1 || allocIdx > projectAllocations.Count) return;

            var selectedAlloc = projectAllocations[allocIdx - 1];

            Console.WriteLine($"\nEnd {selectedAlloc.Name}'s allocation on {projectName}?");
            Console.WriteLine($"Set end date to today ({DateTime.Today.ToString("dd-MMM-yyyy")})?\n");
            var confirm = ConsoleUI.ReadInput("[Y] Yes, End Now    [B] Back");

            if (confirm.ToUpper() == "Y" || confirm.ToUpper() == "YES")
            {
                // Passing endDate as JSON scalar in body
                await _api.PutAsync($"manager/allocations/{selectedAlloc.AllocId}/end", DateTime.Today.ToString("yyyy-MM-dd"));
                Console.WriteLine($"\nAllocation ended. {selectedAlloc.Name} freed from {projectName} as of {DateTime.Today.ToString("dd-MMM-yyyy")}. ✓");
                Console.WriteLine("Resource status updated to BENCH if no other active allocations remain.");
                Console.ReadLine();
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError($"Failed: {ex.Message}");
            Console.ReadLine();
        }
    }
}
