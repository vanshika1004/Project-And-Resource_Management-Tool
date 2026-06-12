using ConsoleClient.ApiClients;


namespace ConsoleClient.Screens.Manager;

public static class AIAssistantScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║    AI ASSISTANT                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("1. Skill Match    — Find best employees for a project requirement");
            Console.WriteLine("2. Risk Summary   — Get a health analysis for a project");
            Console.WriteLine("3. Back");
            Console.WriteLine();
            Console.Write("Enter option: ");

            var input = Console.ReadLine()?.Trim();
            switch (input)
            {
                case "1":
                    await ShowSkillMatchAsync();
                    break;
                case "2":
                    await ShowRiskSummaryAsync();
                    break;
                case "3":
                    return;
                default:
                    Console.WriteLine("Invalid option.");
                    await Task.Delay(1000);
                    break;
            }
        }
    }

    private static async Task ShowSkillMatchAsync()
    {
        Console.Clear();
        Console.WriteLine("── Skill Match ────────────────────────────────\n");
        Console.Write("Enter Project ID: ");
        if (!int.TryParse(Console.ReadLine(), out int projectId)) return;

        Console.WriteLine("\nDescribe your project requirement in plain English:");
        Console.Write("> ");
        var requirement = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(requirement)) return;

        Console.WriteLine("\nSearching... (calling AI)");

        var result = await AIApiClient.GetSkillMatchAsync(requirement, projectId);

        Console.Clear();
        Console.WriteLine("──────────────────────────────────────────────");
        Console.WriteLine("AI-MATCHED RESULTS");
        Console.WriteLine("──────────────────────────────────────────────");

        if (result == null || result.Candidates == null || !result.Candidates.Any())
        {
            Console.WriteLine("No matching resources found or an error occurred.");
        }
        else
        {
            for (int i = 0; i < result.Candidates.Count; i++)
            {
                var c = result.Candidates[i];
                Console.WriteLine($"{i + 1}.  {c.Name}");
                Console.WriteLine($"    Reason: {c.Reason}");
                Console.WriteLine($"    Availability: {c.Availability} (Suggested allocation: {c.SuggestedAllocationPercent}%)");
                Console.WriteLine();
            }
            Console.WriteLine($"Note: {result.Note}");
        }

        Console.WriteLine("──────────────────────────────────────────────");
        Console.WriteLine("[A] Go to Allocate Resource     [B] Back");
        
        while (true)
        {
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.A)
            {
                await AllocateResourceScreen.ShowAsync();
                return;
            }
            else if (key == ConsoleKey.B)
            {
                return;
            }
        }
    }

    private static async Task ShowRiskSummaryAsync()
    {
        Console.Clear();
        Console.WriteLine("── Risk Summary ───────────────────────────────\n");
        
        var projects = await ProjectApiClient.GetAllAsync();

        if (projects == null || !projects.Any())
        {
            Console.WriteLine("You have no projects.");
            Console.WriteLine("\n[B] Back");
            while (Console.ReadKey(true).Key != ConsoleKey.B) { }
            return;
        }

        Console.WriteLine("Select project:");
        foreach (var p in projects)
        {
            Console.WriteLine($"  {p.Id}.  {p.ProjectName}    {p.HealthStatus}");
        }
        Console.WriteLine();
        Console.Write("Enter project number: ");
        
        if (!int.TryParse(Console.ReadLine(), out int projectId)) return;

        Console.WriteLine("\nGenerating AI summary...");

        var summary = await AIApiClient.GetRiskSummaryAsync(projectId);

        Console.Clear();
        Console.WriteLine($"── AI Risk Summary ────────────────────────────\n");
        
        if (string.IsNullOrWhiteSpace(summary))
        {
            Console.WriteLine("Could not generate risk summary.");
        }
        else
        {
            Console.WriteLine($"\"{summary}\"\n");
            Console.WriteLine("  Note: This summary is AI-generated from milestone and timesheet data.");
        }

        Console.WriteLine("\n[B] Back");
        while (Console.ReadKey(true).Key != ConsoleKey.B) { }
    }
}
