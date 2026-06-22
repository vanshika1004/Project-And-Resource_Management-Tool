using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PRM.ConsoleClient.Models;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class TeamBuilderScreen
{
    private readonly ApiClient _api;

    public TeamBuilderScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        ConsoleUI.PrintHeader("TEAM BUILDER");
        
        var projectIdStr = ConsoleUI.ReadInput("Step 1 — Project ID");
        if (!int.TryParse(projectIdStr, out int projectId))
        {
            ConsoleUI.PrintError("Invalid Project ID.");
            Console.ReadLine();
            return;
        }

        var roles = new List<TeamBuildRoleRequest>();

        while (true)
        {
            ConsoleUI.PrintHeader("TEAM BUILDER");
            Console.WriteLine($"Step 1 — Project ID: {projectId}");
            Console.WriteLine("Step 2 — Define Roles needed:");
            
            if (!roles.Any())
            {
                Console.WriteLine("  (No roles added yet)");
            }
            else
            {
                for (int i = 0; i < roles.Count; i++)
                {
                    var r = roles[i];
                    Console.WriteLine($"[{i + 1}] Role: {r.Title}");
                    Console.WriteLine($"    Skills: {string.Join(", ", r.RequiredSkills)} (Min: {r.MinimumProficiency})");
                }
            }

            Console.WriteLine("\n[A] Add Role   [B] Build Team   [C] Cancel");
            var option = ConsoleUI.ReadInput("Enter option");

            if (option.ToUpper() == "C") return;
            
            if (option.ToUpper() == "A")
            {
                var title = ConsoleUI.ReadInput("Role Title");
                var skillsInput = ConsoleUI.ReadInput("Required Skills (comma separated)");
                var proficiencyInput = ConsoleUI.ReadInput("Min Proficiency (1=Beginner, 2=Intermediate, 3=Expert)");

                if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(skillsInput) && int.TryParse(proficiencyInput, out int prof))
                {
                    var skills = skillsInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                    var proficiency = prof switch
                    {
                        1 => ProficiencyLevel.Beginner,
                        2 => ProficiencyLevel.Intermediate,
                        3 => ProficiencyLevel.Advanced,
                        _ => ProficiencyLevel.Beginner
                    };
                    roles.Add(new TeamBuildRoleRequest(title, skills, proficiency));
                }
                else
                {
                    ConsoleUI.PrintError("Invalid role input.");
                    Console.ReadLine();
                }
            }
            else if (option.ToUpper() == "B")
            {
                if (!roles.Any())
                {
                    ConsoleUI.PrintError("Add at least one role before building.");
                    Console.ReadLine();
                    continue;
                }

                await PerformTeamBuildAsync(projectId, roles);
            }
        }
    }

    private async Task PerformTeamBuildAsync(int projectId, List<TeamBuildRoleRequest> roles)
    {
        Console.WriteLine("\nRunning AI Team Matcher...\n");
        
        try
        {
            var req = new TeamBuildRequest(projectId, roles);
            var result = await _api.PostAsync<TeamBuildRequest, TeamBuildResultDto>("manager/ai/team-builder", req);
            if (result == null) throw new Exception("Failed to get team match results.");
            
            Console.WriteLine("AI-MATCHED RESULTS");
            Console.WriteLine("──────────────────────────────────────────────");
            
            foreach (var match in result.Matches)
            {
                Console.WriteLine($"Role: {match.Title}");
                if (match.AssignedUserId.HasValue)
                {
                    Console.WriteLine($"  Assigned: {match.AssignedUserName} (ID: {match.AssignedUserId})");
                }
                else
                {
                    Console.WriteLine($"  Assigned: UNFILLED");
                }
                Console.WriteLine($"  Reason  : {match.Reason}\n");
            }

            Console.WriteLine($"Summary: {result.AiSummary}");
            Console.WriteLine("──────────────────────────────────────────────");
            
            var option = ConsoleUI.ReadInput("[S] Save Filled Allocations    [R] Revise Roles");
            
            if (option.ToUpper() == "S")
            {
                var allocMode = ConsoleUI.ReadInput("\n[1] Quick Allocate (100% until project end date)   [2] Manual Allocate");
                
                DateTime toDate;
                int percent;
                
                if (allocMode == "1")
                {
                    var project = await _api.GetAsync<JsonElement>($"manager/projects/{projectId}");
                    toDate = project.GetProperty("endDate").GetDateTime();
                    percent = 100;
                }
                else
                {
                    var percentInput = ConsoleUI.ReadInput("Enter utilization percent (1-100)");
                    if (!int.TryParse(percentInput, out percent) || percent < 1 || percent > 100)
                    {
                        ConsoleUI.PrintError("Invalid utilization percent.");
                        Console.ReadLine();
                        return;
                    }
                    
                    var dateInput = ConsoleUI.ReadInput("Enter end date (YYYY-MM-DD)");
                    if (!DateTime.TryParse(dateInput, out toDate) || toDate < DateTime.Today)
                    {
                        ConsoleUI.PrintError("Invalid end date.");
                        Console.ReadLine();
                        return;
                    }
                }

                Console.WriteLine("\nSaving allocations...");
                foreach (var match in result.Matches)
                {
                    if (match.AssignedUserId.HasValue)
                    {
                        var allocReq = new
                        {
                            UserId = match.AssignedUserId.Value,
                            ProjectId = projectId,
                            FromDate = DateTime.Today,
                            ToDate = toDate,
                            UtilisationPercent = percent
                        };
                        
                        try
                        {
                            await _api.PostAsync("manager/allocations", allocReq);
                            ConsoleUI.PrintSuccess($"Allocated {match.AssignedUserName} (ID: {match.AssignedUserId.Value}) to Project {projectId} at {percent}% until {toDate:yyyy-MM-dd}");
                        }
                        catch (Exception ex)
                        {
                            ConsoleUI.PrintError($"Failed to allocate {match.AssignedUserName}: {ex.Message}");
                        }
                    }
                }
                Console.WriteLine("\nDone. Press Enter to return.");
                Console.ReadLine();
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError($"Team build failed: {ex.Message}");
            Console.ReadLine();
        }
    }
}
