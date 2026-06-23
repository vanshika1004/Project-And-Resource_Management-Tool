using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class MyProjectsScreen
{
    private readonly ApiClient _api;

    public MyProjectsScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║    MY PROJECTS                               ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝\n");
            
            try
            {
                var projects = await _api.GetAsync<List<JsonElement>>("manager/projects");
                if (projects != null && projects.Count > 0)
                {
                    Console.WriteLine($"{"#".PadRight(4)} {"Project".PadRight(16)} {"End Date".PadRight(12)} {"Health"}");
                    Console.WriteLine("──────────────────────────────────────────────");
                    int i = 1;
                    foreach (var p in projects)
                    {
                        var name = p.GetProperty("name").GetString() ?? "";
                        var end = p.GetProperty("endDate").GetDateTime().ToString("dd-MMM-yy");
                        var health = p.GetProperty("healthStatus").GetString() ?? "";
                        
                        string healthEmoji = health.ToUpper() switch
                        {
                            "ONTRACK" => "🟢 ON TRACK",
                            "ATTENTION" => "🟡 ATTENTION",
                            "ATRISK" => "🔴 AT RISK",
                            _ => health
                        };

                        Console.WriteLine($"{i.ToString().PadRight(4)} {name.PadRight(16)} {end.PadRight(12)} {healthEmoji}");
                        i++;
                    }
                    Console.WriteLine("──────────────────────────────────────────────");
                    
                    var option = ConsoleUI.ReadInput("Select project number to view details (or 0 to go back)");
                    if (option == "0" || string.IsNullOrWhiteSpace(option))
                    {
                        break;
                    }

                    if (int.TryParse(option, out int selectedIndex) && selectedIndex > 0 && selectedIndex <= projects.Count)
                    {
                        var selectedProject = projects[selectedIndex - 1];
                        var projectId = selectedProject.GetProperty("id").GetInt32();
                        await ShowProjectDetailsAsync(projectId);
                    }
                }
                else
                {
                    Console.WriteLine("No projects found.");
                    Console.WriteLine("\nPress Enter to return...");
                    Console.ReadLine();
                    break;
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintError($"Failed to load projects: {ex.Message}");
                Console.ReadLine();
                break;
            }
        }
    }

    private async Task ShowProjectDetailsAsync(int projectId)
    {
        while (true)
        {
            try
            {
                var project = await _api.GetAsync<JsonElement>($"manager/projects/{projectId}");
                var name = project.GetProperty("name").GetString() ?? "";
                var health = project.GetProperty("healthStatus").GetString() ?? "";
                
                string healthEmoji = health.ToUpper() switch
                {
                    "ONTRACK" => "🟢 ON TRACK",
                    "ATTENTION" => "🟡 ATTENTION",
                    "ATRISK" => "🔴 AT RISK",
                    _ => health
                };

                Console.WriteLine($"\n── {name} ───────────────────────────────");
                Console.WriteLine($"Health Status : {healthEmoji}\n");

                Console.WriteLine("Risk Flags:");
                try
                {
                    var flags = await _api.GetAsync<List<string>>($"manager/projects/{projectId}/risk-flags");
                    if (flags != null && flags.Count > 0)
                    {
                        foreach (var flag in flags)
                        {
                            Console.WriteLine($"  {flag}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("  None");
                    }
                }
                catch
                {
                    Console.WriteLine("  Failed to load risk flags.");
                }
                Console.WriteLine();
                
                Console.WriteLine("Milestones:");
                Console.WriteLine($"{"#".PadRight(4)} {"Title".PadRight(18)} {"Due Date".PadRight(12)} {"Status"}");
                
                if (project.TryGetProperty("milestones", out JsonElement milestones) && milestones.ValueKind == JsonValueKind.Array)
                {
                    int mIdx = 1;
                    foreach (var m in milestones.EnumerateArray())
                    {
                        var mTitle = m.GetProperty("title").GetString() ?? "";
                        var mDue = m.GetProperty("dueDate").GetDateTime().ToString("dd-MMM-yy");
                        
                        string mStatusStr = "";
                        var mStatus = m.GetProperty("status");
                        if (mStatus.ValueKind == JsonValueKind.String) mStatusStr = mStatus.GetString() ?? "";
                        else if (mStatus.ValueKind == JsonValueKind.Number)
                        {
                            mStatusStr = mStatus.GetInt32() switch { 0 => "NOT_STARTED", 1 => "IN_PROGRESS", 2 => "IN_REVIEW", 3 => "DONE", _ => "UNKNOWN" };
                        }
                        
                        string overdueFlag = "";
                        if (mStatusStr != "DONE" && m.GetProperty("dueDate").GetDateTime() < DateTime.Today)
                        {
                            overdueFlag = "  ⚠ OVERDUE";
                        }

                        Console.WriteLine($"{mIdx.ToString().PadRight(4)} {mTitle.PadRight(18)} {mDue.PadRight(12)} {mStatusStr}{overdueFlag}");
                        mIdx++;
                    }
                }

                Console.WriteLine("\nAllocated Resources:");
                var projectAllocations = new List<dynamic>();
                
                if (project.TryGetProperty("allocations", out JsonElement allocs) && allocs.ValueKind == JsonValueKind.Array)
                {
                    foreach (var a in allocs.EnumerateArray())
                    {
                        projectAllocations.Add(new {
                            Name = a.GetProperty("userFullName").GetString() ?? "Unknown",
                            Percent = a.GetProperty("utilisationPercent").GetInt32(),
                            From = a.GetProperty("fromDate").GetDateTime(),
                            To = a.GetProperty("toDate").GetDateTime()
                        });
                    }
                }

                if (projectAllocations.Any())
                {
                    Console.WriteLine($"{"Name".PadRight(14)} {"%".PadRight(6)} {"From".PadRight(12)} {"To"}");
                    foreach (var a in projectAllocations)
                    {
                        Console.WriteLine($"{a.Name.PadRight(14)} {$"{a.Percent}%".PadRight(6)} {a.From.ToString("dd-MMM-yy").PadRight(12)} {a.To.ToString("dd-MMM-yy")}");
                    }
                }
                else
                {
                    Console.WriteLine("  No resources allocated.");
                }

                Console.WriteLine("\n──────────────────────────────────────────────");
                var opt = ConsoleUI.ReadInput("[A] Get AI Risk Summary     [B] Back");
                if (opt.ToUpper() == "B")
                {
                    break;
                }
                else if (opt.ToUpper() == "A")
                {
                    Console.WriteLine($"\n── AI Risk Summary — {name} ────────────\n");
                    Console.WriteLine("Generating AI summary...");
                    try
                    {
                        var summary = await _api.GetAsync<JsonElement>($"manager/ai/risk-summary/{projectId}");
                        Console.WriteLine($"\n{summary.GetProperty("summary").GetString()}\n");
                        Console.WriteLine("  Note: This summary is AI-generated from milestone and timesheet data.\n");
                    }
                    catch (Exception ex)
                    {
                        ConsoleUI.PrintError($"Failed to get summary: {ex.Message}");
                    }
                    ConsoleUI.ReadInput("[B] Back");
                    break;
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintError($"Failed to load project details: {ex.Message}");
                Console.ReadLine();
                break;
            }
        }
    }
}
