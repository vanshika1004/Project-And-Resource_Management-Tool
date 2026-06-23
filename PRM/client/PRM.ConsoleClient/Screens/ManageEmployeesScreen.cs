using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRM.ConsoleClient.Models;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class ManageEmployeesScreen
{
    private readonly ApiClient _api;

    public ManageEmployeesScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ConsoleUI.PrintHeader("MANAGE ResourceS");
            Console.WriteLine("1. View All Resources");
            Console.WriteLine("2. Update Resource");
            Console.WriteLine("3. Deactivate Resource");
            Console.WriteLine("4. Manage Resource Skills");
            Console.WriteLine("5. Assign Manager");
            Console.WriteLine("6. Back");
            Console.WriteLine();

            string option = ConsoleUI.ReadInput("Enter option");

            switch (option)
            {
                case "1":
                    await ViewAllResourcesAsync();
                    break;
                case "2":
                    await UpdateResourceAsync();
                    break;
                case "3":
                    await DeactivateResourceAsync();
                    break;
                case "4":
                    await ManageResourceskillsAsync();
                    break;
                case "5":
                    await AssignManagerAsync();
                    break;
                case "6":
                    return;
                default:
                    ConsoleUI.PrintError("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private async Task ViewAllResourcesAsync()
    {
        string filter = "";
        while (true)
        {
            Console.Clear();
            ConsoleUI.PrintHeader("ALL ResourceS");

            try
            {
                var users = await _api.GetAsync<List<UserDto>>("admin/users");
                var allocations = await _api.GetAsync<List<AllocationDto>>("admin/allocations");

                if (users != null)
                {
                    var Resources = users.Where(u => u.Role == "Resource" || u.Role == "Resource").ToList();
                    
                    var today = DateTime.Today;
                    var activeUserIds = allocations?
                        .Where(a => a.FromDate.Date <= today && a.ToDate.Date >= today)
                        .Select(a => a.UserId)
                        .ToHashSet() ?? new HashSet<int>();

                    var ResourceData = Resources.Select(emp => new 
                    {
                        Emp = emp,
                        Status = activeUserIds.Contains(emp.Id) ? "ALLOCATED" : "BENCH",
                        Dept = emp.Department ?? "N/A"
                    }).ToList();

                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        var lowerFilter = filter.ToLower();
                        ResourceData = ResourceData.Where(e => 
                            e.Dept.ToLower().Contains(lowerFilter) || 
                            e.Status.ToLower().Contains(lowerFilter)).ToList();
                    }

                    Console.WriteLine($"{"ID".PadRight(5)} {"Name".PadRight(20)} {"Department".PadRight(15)} {"Status"}");
                    Console.WriteLine("────────────────────────────────────────────────────────────");
                    
                    int allocatedCount = 0;
                    int benchCount = 0;

                    foreach (var e in ResourceData)
                    {
                        if (e.Status == "ALLOCATED") allocatedCount++;
                        else benchCount++;

                        Console.WriteLine($"{e.Emp.Id.ToString().PadRight(5)} {e.Emp.FullName.PadRight(20)} {e.Dept.PadRight(15)} {e.Status}");
                    }
                    Console.WriteLine("────────────────────────────────────────────────────────────");
                    Console.WriteLine($"Total: {ResourceData.Count}    |    Allocated: {allocatedCount}    |    Bench: {benchCount}");
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintError(ex.Message);
            }

            Console.WriteLine("\n[F] Filter by Status / Department    [B] Back");
            string opt = ConsoleUI.ReadInput("Enter option");
            
            if (opt.ToUpper() == "B")
                break;
            else if (opt.ToUpper() == "F")
                filter = ConsoleUI.ReadInput("Enter filter text (leave empty to clear)");
        }
    }

    private async Task UpdateResourceAsync()
    {
        ConsoleUI.PrintHeader("UPDATE Resource");
        string idStr = ConsoleUI.ReadInput("Enter Resource ID");

        if (int.TryParse(idStr, out int id))
        {
            try
            {
                var user = await _api.GetAsync<UserDto>($"admin/users/{id}");
                if (user != null)
                {
                    Console.WriteLine($"\n── {user.FullName} ─────────────────────────────────");
                    string name = ConsoleUI.ReadInput($"Full Name [{user.FullName}]");
                    string email = ConsoleUI.ReadInput($"Email [{user.Email}]");
                    string dept = ConsoleUI.ReadInput($"Department [{user.Department}]");

                    name = string.IsNullOrWhiteSpace(name) ? user.FullName : name;
                    email = string.IsNullOrWhiteSpace(email) ? user.Email : email;
                    dept = string.IsNullOrWhiteSpace(dept) ? user.Department ?? "" : dept;

                    Console.WriteLine("\n──────────────────────────────────────────────");
                    string confirm = ConsoleUI.ReadInput("[S] Save     [B] Back");
                    
                    if (confirm.ToUpper() == "S")
                    {
                        var req = new UpdateUserRequest(user.Id, name, email, dept, user.ManagerId, user.IsActive);
                        await _api.PutAsync($"admin/users/{user.Id}", req);
                        ConsoleUI.PrintSuccess("Resource updated.");
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintError(ex.Message);
            }
        }
    }

    private async Task DeactivateResourceAsync()
    {
        ConsoleUI.PrintHeader("DEACTIVATE Resource");
        string idStr = ConsoleUI.ReadInput("Enter Resource ID");

        if (int.TryParse(idStr, out int id))
        {
            try
            {
                var user = await _api.GetAsync<UserDto>($"admin/users/{id}");
                if (user != null)
                {
                    Console.WriteLine($"\n── {user.FullName} ─────────────────────────────────");
                    Console.WriteLine($"Department : {user.Department}");
                    Console.WriteLine($"Status     : {(user.IsActive ? "Active" : "Inactive")}");
                    Console.WriteLine("\nAre you sure you want to deactivate this Resource?");
                    Console.WriteLine("This will set is_active = false, end all active allocations today,\nand block their login account.");

                    string confirm = ConsoleUI.ReadInput("\n[Y] Yes, Deactivate     [B] Cancel");
                    if (confirm.ToUpper() == "Y")
                    {
                        await _api.PutAsync($"admin/users/{id}/deactivate", new { });
                        ConsoleUI.PrintSuccess("Resource deactivated.");
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintError(ex.Message);
            }
        }
    }

    private async Task ManageResourceskillsAsync()
    {
        ConsoleUI.PrintHeader("MANAGE SKILLS");
        string idStr = ConsoleUI.ReadInput("Enter Resource ID");

        if (!int.TryParse(idStr, out int id)) return;

        try
        {
            var user = await _api.GetAsync<UserDto>($"admin/users/{id}");
            if (user == null) return;

            while (true)
            {
                ConsoleUI.PrintHeader("MANAGE SKILLS", user.FullName);
                var skills = await _api.GetAsync<List<UserSkillDto>>($"admin/users/{id}/skills");

                Console.WriteLine($"── {user.FullName} ─────────────────────────────────");
                Console.WriteLine("Current Skills:");
                if (skills != null && skills.Count > 0)
                {
                    for (int i = 0; i < skills.Count; i++)
                    {
                        Console.WriteLine($"  {i + 1}.  {skills[i].SkillName.PadRight(18)} {skills[i].ProficiencyLevel}");
                    }
                }
                else
                {
                    Console.WriteLine("  No skills found.");
                }
                Console.WriteLine("──────────────────────────────────────────────\n");
                
                Console.WriteLine("1. Add Skill");
                Console.WriteLine("2. Update Proficiency Level");
                Console.WriteLine("3. Remove Skill");
                Console.WriteLine("4. Back");
                Console.WriteLine();

                string opt = ConsoleUI.ReadInput("Enter option");

                if (opt == "1")
                {
                    string name = ConsoleUI.ReadInput("\nSkill Name        ");
                    Console.WriteLine("Category          : (1) Backend  (2) Frontend  (3) DevOps  (4) QA  (5) Other");
                    string catStr = ConsoleUI.ReadInput("Enter choice      ");
                    Console.WriteLine("Proficiency Level : (1) Beginner  (2) Intermediate  (3) Advanced");
                    string profStr = ConsoleUI.ReadInput("Enter choice      ");

                    if (int.TryParse(catStr, out int catId) && int.TryParse(profStr, out int profId))
                    {
                        var req = new AddSkillRequest(name, (SkillCategory)(catId - 1), (ProficiencyLevel)(profId - 1));
                        await _api.PostAsync($"admin/users/{id}/skills", req);
                        ConsoleUI.PrintSuccess("Skill added.");
                    }
                }
                else if (opt == "2")
                {
                    string skillNumStr = ConsoleUI.ReadInput("\nEnter skill # to update");
                    if (int.TryParse(skillNumStr, out int num) && num > 0 && num <= skills?.Count)
                    {
                        var skill = skills[num - 1];
                        Console.WriteLine("Proficiency Level : (1) Beginner  (2) Intermediate  (3) Advanced");
                        string profStr = ConsoleUI.ReadInput("Enter choice      ");
                        if (int.TryParse(profStr, out int profId))
                        {
                            var req = new UpdateSkillRequest((ProficiencyLevel)(profId - 1));
                            await _api.PutAsync($"admin/users/{id}/skills/{skill.SkillId}", req);
                            ConsoleUI.PrintSuccess("Skill updated.");
                        }
                    }
                }
                else if (opt == "3")
                {
                    string skillNumStr = ConsoleUI.ReadInput("\nEnter skill # to remove");
                    if (int.TryParse(skillNumStr, out int num) && num > 0 && num <= skills?.Count)
                    {
                        var skill = skills[num - 1];
                        await _api.DeleteAsync($"admin/users/{id}/skills/{skill.SkillId}");
                        ConsoleUI.PrintSuccess("Skill removed.");
                    }
                }
                else if (opt == "4")
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

    private async Task AssignManagerAsync()
    {
        ConsoleUI.PrintHeader("ASSIGN MANAGER");
        string empIdStr = ConsoleUI.ReadInput("Resource User ID");
        string mgrIdStr = ConsoleUI.ReadInput("Manager User ID ");

        Console.WriteLine("\n──────────────────────────────────────────────");
        string confirm = ConsoleUI.ReadInput("[S] Save     [B] Back");

        if (confirm.ToUpper() == "S")
        {
            if (int.TryParse(empIdStr, out int empId) && int.TryParse(mgrIdStr, out int mgrId))
            {
                try
                {
                    var user = await _api.GetAsync<UserDto>($"admin/users/{empId}");
                    if (user != null)
                    {
                        var req = new UpdateUserRequest(user.Id, user.FullName, user.Email, user.Department, mgrId, user.IsActive);
                        await _api.PutAsync($"admin/users/{user.Id}", req);
                        ConsoleUI.PrintSuccess("Manager assigned.");
                    }
                }
                catch (Exception ex)
                {
                    ConsoleUI.PrintError(ex.Message);
                }
            }
        }
    }
}
