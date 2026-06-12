using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin.Employee;

public static class ManageEmployeeSkillsScreen
{
    public static async Task ShowAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("MANAGE SKILLS");

        var empIdStr = ConsoleUIHelper.Prompt("Enter Employee ID");
        if (!int.TryParse(empIdStr, out var empId)) return;

        while (true)
        {
            try
            {
                var emp = await EmployeeApiClient.GetByIdAsync(empId);
                if (emp == null)
                {
                    ConsoleUIHelper.ShowError("Employee not found.");
                    ConsoleUIHelper.PressAnyKey();
                    return;
                }

                Console.WriteLine();
                Console.WriteLine($"── {emp.FullName} ".PadRight(46, '─'));
                Console.WriteLine("Current Skills:");
                
                var skills = await EmployeeSkillApiClient.GetByEmployeeIdAsync(empId);
                
                if (!skills.Any())
                {
                    Console.WriteLine("  No skills found.");
                }
                else
                {
                    for (int i = 0; i < skills.Count; i++)
                    {
                        var skill = skills[i];
                        Console.WriteLine($"  {i + 1}.  {skill.SkillName,-18} {skill.ProficiencyLevel}");
                    }
                }
                
                ConsoleUIHelper.DrawSeparator();
                
                var choice = ConsoleUIHelper.ShowMenu(new[]
                {
                    "Add Skill",
                    "Update Proficiency Level",
                    "Remove Skill",
                    "Back"
                });

                if (choice == 1)
                {
                    await AddSkillAsync(empId);
                }
                else if (choice == 2)
                {
                    ConsoleUIHelper.ShowInfo("[Update Proficiency Level — Coming Soon]");
                    ConsoleUIHelper.PressAnyKey();
                }
                else if (choice == 3)
                {
                    await RemoveSkillAsync(empId, skills);
                }
                else if (choice == 4)
                {
                    return;
                }
                else
                {
                    ConsoleUIHelper.ShowError("Invalid option.");
                    ConsoleUIHelper.PressAnyKey();
                }
            }
            catch (Exception ex)
            {
                ConsoleUIHelper.ShowError(ex.Message);
                ConsoleUIHelper.PressAnyKey();
                return;
            }
        }
    }

    private static async Task AddSkillAsync(int employeeId)
    {
        Console.WriteLine();
        var skillName = ConsoleUIHelper.Prompt("Skill Name");
        Console.WriteLine("Category          : (1) Backend  (2) Frontend  (3) DevOps  (4) QA  (5) Other");
        var categoryChoice = ConsoleUIHelper.Prompt("Enter choice");
        
        string category = categoryChoice switch
        {
            "1" => "Backend",
            "2" => "Frontend",
            "3" => "DevOps",
            "4" => "QA",
            _ => "Other"
        };
        
        Console.WriteLine("Proficiency Level : (1) Beginner  (2) Intermediate  (3) Advanced");
        var profChoice = ConsoleUIHelper.Prompt("Enter choice");
        
        string proficiency = profChoice switch
        {
            "1" => "Beginner",
            "2" => "Intermediate",
            "3" => "Advanced",
            _ => "Beginner"
        };
        
        try
        {
            var allSkills = await SkillApiClient.GetAllAsync();
            var existingSkill = allSkills.FirstOrDefault(s => s.SkillName.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            
            int skillId;
            if (existingSkill != null)
            {
                skillId = existingSkill.Id;
            }
            else
            {
                skillId = await SkillApiClient.CreateAsync(new CreateSkillDto
                {
                    SkillName = skillName,
                    Category = category
                });
            }

            await EmployeeSkillApiClient.AddAsync(new CreateEmployeeSkillDto
            {
                EmployeeId = employeeId,
                SkillId = skillId,
                ProficiencyLevel = proficiency
            });

            Console.WriteLine();
            ConsoleUIHelper.ShowSuccess("Skill added.");
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError($"Failed to add skill: {ex.Message}");
        }
        
        ConsoleUIHelper.PressAnyKey();
    }

    private static async Task RemoveSkillAsync(int employeeId, System.Collections.Generic.List<EmployeeSkillDto> skills)
    {
        if (!skills.Any())
        {
            ConsoleUIHelper.ShowWarning("No skills to remove.");
            ConsoleUIHelper.PressAnyKey();
            return;
        }

        Console.WriteLine();
        var numStr = ConsoleUIHelper.Prompt("Enter the number of the skill to remove (or 0 to cancel)");
        if (!int.TryParse(numStr, out var num) || num < 0 || num > skills.Count)
        {
            ConsoleUIHelper.ShowError("Invalid selection.");
            ConsoleUIHelper.PressAnyKey();
            return;
        }

        if (num == 0) return;

        var skillToRemove = skills[num - 1];

        Console.WriteLine($"Are you sure you want to remove '{skillToRemove.SkillName}'? (Y/N)");
        var confirm = Console.ReadLine()?.Trim().ToUpper();

        if (confirm == "Y")
        {
            try
            {
                await EmployeeSkillApiClient.RemoveAsync(skillToRemove.Id);
                ConsoleUIHelper.ShowSuccess("Skill removed successfully.");
            }
            catch (Exception ex)
            {
                ConsoleUIHelper.ShowError($"Failed to remove skill: {ex.Message}");
            }
            ConsoleUIHelper.PressAnyKey();
        }
    }
}
