using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin.Project;

public static class ManageMilestonesScreen
{
    public static async Task ShowAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("MANAGE MILESTONES");

        var projIdStr = ConsoleUIHelper.Prompt("Select Project (ID)");
        if (!int.TryParse(projIdStr, out var projId)) return;

        while (true)
        {
            try
            {
                var proj = await ProjectApiClient.GetByIdAsync(projId);
                if (proj == null)
                {
                    ConsoleUIHelper.ShowError("Project not found.");
                    ConsoleUIHelper.PressAnyKey();
                    return;
                }
                
                ConsoleUIHelper.ClearScreen();
                ConsoleUIHelper.DrawHeader("MANAGE MILESTONES", $"Project: {proj.ProjectName} ({proj.Id})");

                var milestones = await MilestoneApiClient.GetByProjectIdAsync(projId);

                Console.WriteLine();
                Console.WriteLine($"── {proj.ProjectName} ".PadRight(54, '─'));
                Console.WriteLine($"{"#",-4} {"Title",-18} {"Due Date",-12} {"Story Pts",-11} {"Status"}");
                Console.WriteLine(new string('─', 54));

                int completedSp = 0;
                int totalSp = proj.TotalStoryPoints;

                for (int i = 0; i < milestones.Count; i++)
                {
                    var m = milestones[i];
                    Console.WriteLine($"{i + 1 + ".",-4} {m.Title,-18} {m.DueDate:dd-MMM-yy,-12} {m.StoryPoints,-11} {m.Status}");
                    
                    if (m.Status.Equals("Done", StringComparison.OrdinalIgnoreCase))
                    {
                        completedSp += m.StoryPoints;
                    }
                }

                Console.WriteLine(new string('─', 54));
                var remainingSp = totalSp - completedSp;
                Console.WriteLine($"Total: {totalSp} SP   |   Completed: {completedSp} SP   |   Remaining: {remainingSp} SP");
                Console.WriteLine();
                Console.WriteLine("1. Add Milestone");
                Console.WriteLine("2. Update Milestone Status");
                Console.WriteLine("3. Back");
                Console.WriteLine();

                var option = ConsoleUIHelper.Prompt("Enter option");

                if (option == "3")
                {
                    return;
                }
                else if (option == "1")
                {
                    await AddMilestoneAsync(projId);
                }
                else if (option == "2")
                {
                    Console.WriteLine("\nUpdate Milestone Status sub-prompt:\n");
                    var numStr = ConsoleUIHelper.Prompt("Enter Milestone #");
                    if (int.TryParse(numStr, out var index) && index > 0 && index <= milestones.Count)
                    {
                        await UpdateMilestoneStatusAsync(milestones[index - 1]);
                    }
                    else
                    {
                        ConsoleUIHelper.ShowError("Invalid Milestone #.");
                        ConsoleUIHelper.PressAnyKey();
                    }
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

    private static async Task AddMilestoneAsync(int projectId)
    {
        Console.WriteLine("\nAdd Milestone sub-prompt:\n");
        var title = ConsoleUIHelper.Prompt("Milestone Title");
        
        DateTime due;
        var dueStr = ConsoleUIHelper.Prompt("Due Date         : (DD-MMM-YYYY)");
        if (!DateTime.TryParseExact(dueStr, "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out due))
        {
            if (!DateTime.TryParse(dueStr, out due))
            {
                ConsoleUIHelper.ShowError("Invalid date format.");
                ConsoleUIHelper.PressAnyKey();
                return;
            }
        }

        var spStr = ConsoleUIHelper.Prompt("Story Points");
        if (!int.TryParse(spStr, out var sp)) sp = 0;

        try
        {
            await MilestoneApiClient.CreateAsync(new CreateMilestoneDto
            {
                ProjectId = projectId,
                Title = title,
                DueDate = due,
                StoryPoints = sp
            });
            Console.WriteLine("\nMilestone added. ✓");
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError(ex.Message);
        }
        ConsoleUIHelper.PressAnyKey();
    }

    private static async Task UpdateMilestoneStatusAsync(MilestoneDto milestone)
    {
        var statusOptions = new[] { "NotStarted", "InProgress", "Done" };
        
        var optStr = ConsoleUIHelper.Prompt("New Status        : (1) Not Started   (2) In Progress   (3) Done");
        if (int.TryParse(optStr, out var opt) && opt >= 1 && opt <= 3)
        {
            try
            {
                await MilestoneApiClient.UpdateAsync(milestone.Id, new UpdateMilestoneDto
                {
                    Title = milestone.Title,
                    DueDate = milestone.DueDate,
                    StoryPoints = milestone.StoryPoints,
                    Status = statusOptions[opt - 1]
                });
                Console.WriteLine("\nMilestone updated. ✓");
            }
            catch (Exception ex)
            {
                ConsoleUIHelper.ShowError(ex.Message);
            }
        }
        else
        {
            ConsoleUIHelper.ShowError("Invalid option.");
        }
        ConsoleUIHelper.PressAnyKey();
    }
}
