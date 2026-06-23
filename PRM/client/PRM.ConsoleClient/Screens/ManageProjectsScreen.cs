using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PRM.ConsoleClient.Models;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class ManageProjectsScreen
{
    private readonly ApiClient _api;

    public ManageProjectsScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ConsoleUI.PrintHeader("MANAGE PROJECTS");
            Console.WriteLine("1. Create Project");
            Console.WriteLine("2. View All Projects");
            Console.WriteLine("3. Update Project Details");
            Console.WriteLine("4. Manage Milestones");
            Console.WriteLine("5. Back");
            Console.WriteLine();

            string option = ConsoleUI.ReadInput("Enter option");

            switch (option)
            {
                case "1":
                    await CreateProjectAsync();
                    break;
                case "2":
                    await ViewAllProjectsAsync();
                    break;
                case "3":
                    await UpdateProjectAsync();
                    break;
                case "4":
                    await ManageMilestonesAsync();
                    break;
                case "5":
                    return;
                default:
                    ConsoleUI.PrintError("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private async Task CreateProjectAsync()
    {
        ConsoleUI.PrintHeader("CREATE PROJECT");

        string name = ConsoleUI.ReadInput("Project Name    ");
        string desc = ConsoleUI.ReadInput("Description     ");
        string startDateStr = ConsoleUI.ReadInput("Start Date      ");
        string endDateStr = ConsoleUI.ReadInput("End Date        ");
        string mgrIdStr = ConsoleUI.ReadInput("Manager User ID ");
        string ptsStr = ConsoleUI.ReadInput("Story Points    ");

        Console.WriteLine("\n──────────────────────────────────────────────");
        string confirm = ConsoleUI.ReadInput("[S] Save     [B] Back");

        if (confirm.ToUpper() == "S")
        {
            if (DateTime.TryParse(startDateStr, out var startDate) &&
                DateTime.TryParse(endDateStr, out var endDate) &&
                int.TryParse(ptsStr, out int pts))
            {
                int? mgrId = int.TryParse(mgrIdStr, out int m) ? m : null;

                try
                {
                    var req = new CreateProjectRequest(name, desc, startDate, endDate, mgrId, pts);
                    await _api.PostAsync("admin/projects", req);
                    ConsoleUI.PrintSuccess("Project created successfully.");
                }
                catch (Exception ex)
                {
                    ConsoleUI.PrintError(ex.Message);
                }
            }
            else
            {
                ConsoleUI.PrintError("Invalid input formats.");
            }
        }
    }

    private async Task ViewAllProjectsAsync()
    {
        ConsoleUI.PrintHeader("ALL PROJECTS");

        try
        {
            var projects = await _api.GetAsync<List<ProjectDto>>("admin/projects");
            var users = await _api.GetAsync<List<UserDto>>("admin/users");

            if (projects != null)
            {
                Console.WriteLine($"{"ID",-5} {"Name",-16} {"Manager",-14} {"End Date",-13} {"Status",-11} {"SP Done/Total"}");
                Console.WriteLine("─────────────────────────────────────────────────────────────────────────────");
                foreach (var p in projects)
                {
                    string mgrName = "";
                    if (p.ManagerId.HasValue && users != null)
                    {
                        var mgr = users.Find(u => u.Id == p.ManagerId.Value);
                        if (mgr != null) mgrName = mgr.FullName;
                    }

                    int spTotal = p.TotalStoryPoints;
                    int spDone = 0;
                    if (p.Milestones != null)
                    {
                        foreach (var m in p.Milestones)
                        {
                            if (m.Status == MilestoneStatus.Done) spDone += m.StoryPoints;
                        }
                    }

                    Console.WriteLine($"{p.Id,-5} {p.Name,-16} {mgrName,-14} {p.EndDate.ToString("dd-MMM-yy"),-13} {p.Status.ToString().ToUpper(),-11} {spDone,2} / {spTotal}");
                }
                Console.WriteLine("─────────────────────────────────────────────────────────────────────────────");
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError(ex.Message);
        }

        Console.WriteLine("\n[B] Back");
        ConsoleUI.ReadInput("Enter option");
    }

    private async Task UpdateProjectAsync()
    {
        ConsoleUI.PrintHeader("UPDATE PROJECT DETAILS");
        string idStr = ConsoleUI.ReadInput("Enter Project ID");

        if (!int.TryParse(idStr, out int id)) return;

        try
        {
            var projects = await _api.GetAsync<List<ProjectDto>>("admin/projects");
            var project = projects?.Find(p => p.Id == id);

            if (project != null)
            {
                Console.WriteLine($"\n— {project.Name} ─────────────────────────────");
                
                string namePrompt = $"Project Name        : {project.Name,-20}(editable)";
                string name = ConsoleUI.ReadInput(namePrompt);
                
                string descPrompt = $"Description         : {project.Description,-20}(editable)";
                string desc = ConsoleUI.ReadInput(descPrompt);
                
                string sdPrompt = $"Start Date          : {project.StartDate.ToString("dd-MMM-yy"),-20}(editable)";
                string startDateStr = ConsoleUI.ReadInput(sdPrompt);
                
                string edPrompt = $"End Date            : {project.EndDate.ToString("dd-MMM-yy"),-20}(editable)";
                string endDateStr = ConsoleUI.ReadInput(edPrompt);
                
                Console.WriteLine("Status              : (1) PLANNED    (2) ACTIVE    (3) ON_HOLD    (4) COMPLETED");
                string statusStr = ConsoleUI.ReadInput("Enter choice");
                
                string mgrPrompt = $"Assign Manager      : {(project.ManagerId?.ToString() ?? "(Enter Manager ID)"),-20}(editable)";
                string mgrIdStr = ConsoleUI.ReadInput(mgrPrompt);
                
                string ptsPrompt = $"Total Story Points  : {project.TotalStoryPoints,-20}(editable)";
                string ptsStr = ConsoleUI.ReadInput(ptsPrompt);

                name = string.IsNullOrWhiteSpace(name) ? project.Name : name;
                desc = string.IsNullOrWhiteSpace(desc) ? project.Description : desc;
                DateTime startDate = DateTime.TryParse(startDateStr, out var sd) ? sd : project.StartDate;
                DateTime endDate = DateTime.TryParse(endDateStr, out var ed) ? ed : project.EndDate;
                ProjectStatus status = project.Status;
                if (int.TryParse(statusStr, out int sid) && sid >= 1 && sid <= 4)
                {
                    status = (ProjectStatus)(sid - 1);
                }
                HealthStatus health = project.HealthStatus; // Retaining health since it's removed from UI
                int? mgrId = int.TryParse(mgrIdStr, out int mid) ? mid : project.ManagerId;
                int pts = int.TryParse(ptsStr, out int p) ? p : project.TotalStoryPoints;

                Console.WriteLine("────────────────────────────────────────────");
                string confirm = ConsoleUI.ReadInput("[S] Save     [B] Back");

                if (confirm.ToUpper() == "S")
                {
                    var req = new UpdateProjectRequest(project.Id, name, desc, startDate, endDate, status, mgrId, pts, health);
                    await _api.PutAsync($"admin/projects/{project.Id}", req);
                    ConsoleUI.PrintSuccess("Project updated.");
                }
            }
            else
            {
                ConsoleUI.PrintError("Project not found.");
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError(ex.Message);
        }
    }

    private async Task ManageMilestonesAsync()
    {
        ConsoleUI.PrintHeader("MILESTONES");
        string idStr = ConsoleUI.ReadInput("Enter Project ID");

        if (!int.TryParse(idStr, out int id)) return;

        try
        {
            while (true)
            {
                var projects = await _api.GetAsync<List<ProjectDto>>("admin/projects");
                var project = projects?.Find(p => p.Id == id);
                
                if (project == null) 
                {
                    ConsoleUI.PrintError("Project not found");
                    return;
                }

                ConsoleUI.PrintHeader("MILESTONES");
                Console.WriteLine($"Enter Project ID: {id}\n");
                
                Console.WriteLine($"── {project.Name} ───────────────────────────────");
                Console.WriteLine($"{"#".PadRight(4)} {"Title".PadRight(19)} {"Due Date".PadRight(12)} {"Story Pts".PadRight(11)} {"Status"}");
                Console.WriteLine("────────────────────────────────────────────────────────");

                var milestones = project.Milestones ?? new List<MilestoneDto>();
                int totalSp = project.TotalStoryPoints;
                int completedSp = 0;

                if (milestones.Count > 0)
                {
                    for (int i = 0; i < milestones.Count; i++)
                    {
                        var m = milestones[i];
                        if (m.Status == MilestoneStatus.Done) completedSp += m.StoryPoints;
                        
                        string num = $"{i + 1}.";
                        Console.WriteLine($"{num.PadRight(4)} {m.Title.PadRight(19)} {m.DueDate.ToString("dd-MMM-yy").PadRight(12)} {m.StoryPoints.ToString().PadRight(11)} {m.Status}");
                    }
                }
                Console.WriteLine("────────────────────────────────────────────────────────");
                int remainingSp = totalSp - completedSp;
                Console.WriteLine($"Total: {totalSp} SP   |   Completed: {completedSp} SP   |   Remaining: {remainingSp} SP\n");
                
                Console.WriteLine("1. Add Milestone");
                Console.WriteLine("2. Update Milestone Status");
                Console.WriteLine("3. Back\n");

                string opt = ConsoleUI.ReadInput("Enter option");

                if (opt == "1")
                {
                    Console.WriteLine("\nAdd Milestone sub-prompt:\n");
                    Console.Write("Milestone Title : ");
                    string title = Console.ReadLine()?.Trim() ?? "";
                    Console.Write("Due Date        : (DD-MM-YYYY) ");
                    string dueStr = Console.ReadLine()?.Trim() ?? "";
                    Console.Write("Story Points    : ");
                    string ptsStr = Console.ReadLine()?.Trim() ?? "";

                    if (DateTime.TryParse(dueStr, out var dueDate) && int.TryParse(ptsStr, out int pts))
                    {
                        var req = new CreateMilestoneRequest(project.Id, title, dueDate, pts);
                        await _api.PostAsync($"admin/projects/{project.Id}/milestones", req);
                        ConsoleUI.PrintSuccess("Milestone added.");
                    }
                    else
                    {
                        ConsoleUI.PrintError("Invalid input formats.");
                    }
                }
                else if (opt == "2")
                {
                    Console.WriteLine("\nUpdate Milestone Status sub-prompt:\n");
                    Console.Write("Enter Milestone # : ");
                    string numStr = Console.ReadLine()?.Trim() ?? "";
                    
                    if (int.TryParse(numStr, out int num) && num > 0 && num <= milestones.Count)
                    {
                        var m = milestones[num - 1];
                        Console.WriteLine("New Status        : (1) NOT_STARTED   (2) IN_PROGRESS   (3) DONE");
                        string statStr = Console.ReadLine()?.Trim() ?? "";
                        if (int.TryParse(statStr, out int sid) && sid >= 1 && sid <= 3)
                        {
                            MilestoneStatus newStatus = sid switch {
                                1 => MilestoneStatus.NotStarted,
                                2 => MilestoneStatus.InProgress,
                                3 => MilestoneStatus.Done,
                                _ => MilestoneStatus.NotStarted
                            };
                            
                            await _api.PutAsync($"admin/projects/{project.Id}/milestones/{m.Id}/status", newStatus);
                            ConsoleUI.PrintSuccess("Milestone updated.");
                        }
                        else
                        {
                            ConsoleUI.PrintError("Invalid status choice.");
                        }
                    }
                    else
                    {
                        ConsoleUI.PrintError("Invalid milestone number.");
                    }
                }
                else if (opt == "3")
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError(ex.Message);
        }
    }
}
