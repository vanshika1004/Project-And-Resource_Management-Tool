using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class AiAssistantScreen
{
    private readonly ApiClient _api;

    public AiAssistantScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║    AI ASSISTANT                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝\n");
            Console.WriteLine("1. Skill Match    — Find best Resources for a project requirement");
            Console.WriteLine("2. Risk Summary   — Get a health analysis for a project");
            Console.WriteLine("3. Back");
            var option = ConsoleUI.ReadInput("Enter option");

            switch (option)
            {
                case "1":
                    await AiSkillMatchAsync();
                    break;
                case "2":
                    await AiRiskSummaryAsync();
                    break;
                case "3":
                    return;
                default:
                    ConsoleUI.PrintError("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private async Task AiSkillMatchAsync()
    {
        Console.WriteLine("\n── Skill Match ────────────────────────────────\n");
        var req = ConsoleUI.ReadInput("Describe your project requirement in plain English");
        if (string.IsNullOrWhiteSpace(req)) return;

        try
        {
            Console.WriteLine("\nSearching... (calling AI)\n");
            var result = await _api.PostAsync<object, JsonElement>("manager/ai/skill-match", new { requirement = req });
            
            Console.WriteLine("Results:");
            Console.WriteLine(result.GetProperty("summary").GetString());
            
            Console.WriteLine("\n  Note: These are AI-generated suggestions. Always verify availability");
            Console.WriteLine("  and skills with the Resource before allocating.\n");

            var option = ConsoleUI.ReadInput("[A] Go to Allocate Resource     [B] Back");
            if (option.ToUpper() == "A")
            {
                var allocateScreen = new AllocateResourceScreen(_api);
                await allocateScreen.RunAsync();
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError($"Failed: {ex.Message}");
            Console.ReadLine();
        }
    }

    private async Task AiRiskSummaryAsync()
    {
        Console.WriteLine("\n── Risk Summary ───────────────────────────────\n");
        
        try
        {
            var projects = await _api.GetAsync<List<JsonElement>>("manager/projects");
            if (projects != null && projects.Count > 0)
            {
                Console.WriteLine("Select project:");
                int index = 1;
                foreach (var project in projects)
                {
                    var name = project.GetProperty("name").GetString() ?? "";
                    var health = project.GetProperty("healthStatus").GetString() ?? "";
                    string healthEmoji = health.ToUpper() switch
                    {
                        "ONTRACK" => "🟢 ON TRACK",
                        "ATTENTION" => "🟡 ATTENTION",
                        "ATRISK" => "🔴 AT RISK",
                        _ => health
                    };
                    Console.WriteLine($"  {index}.  {name.PadRight(15)} {healthEmoji}");
                    index++;
                }

                var opt = ConsoleUI.ReadInput("\nEnter project number");
                if (int.TryParse(opt, out int selectedIndex) && selectedIndex > 0 && selectedIndex <= projects.Count)
                {
                    var selectedProject = projects[selectedIndex - 1];
                    var projectId = selectedProject.GetProperty("id").GetInt32();
                    
                    Console.WriteLine("\nGenerating AI summary...\n");
                    var summary = await _api.GetAsync<JsonElement>($"manager/ai/risk-summary/{projectId}");
                    Console.WriteLine(summary.GetProperty("summary").GetString());
                    
                    Console.WriteLine("\n  Note: AI-generated from current milestone and timesheet data.\n");
                }
            }
            else
            {
                Console.WriteLine("No projects found.");
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError($"Failed: {ex.Message}");
        }
        
        ConsoleUI.ReadInput("[B] Back");
    }
}
