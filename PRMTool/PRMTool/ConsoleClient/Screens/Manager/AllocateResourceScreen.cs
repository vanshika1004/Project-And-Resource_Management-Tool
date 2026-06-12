using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Manager;

public static class AllocateResourceScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();
            ConsoleUIHelper.DrawHeader("ALLOCATE RESOURCE");
            
            var choice = ConsoleUIHelper.ShowMenu(new[]
            {
                "Find resource using AI (recommended)",
                "Allocate directly (I already know who I want)",
                "End an existing allocation",
                "Back"
            });

            switch (choice)
            {
                case 1:
                    await AIAssistedSearchAsync();
                    break;
                case 2:
                    await DirectAllocationAsync();
                    break;
                case 3:
                    await EndAllocationAsync();
                    break;
                case 4:
                    return;
                default:
                    ConsoleUIHelper.ShowError("Invalid option.");
                    ConsoleUIHelper.PressAnyKey();
                    break;
            }
        }
    }

    private static async Task AIAssistedSearchAsync()
    {
        ConsoleUIHelper.ClearScreen();
        Console.WriteLine("Step 1 — Select Project");
        var projIdStr = ConsoleUIHelper.Prompt("Enter project name or ID");
        if (!int.TryParse(projIdStr, out var projId)) return;

        Console.WriteLine("\nStep 2 — Describe your requirement");
        Console.WriteLine("Type what kind of resource you need:");
        Console.Write("> ");
        var requirement = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(requirement)) return;

        Console.WriteLine("\nSearching... (AI matching in progress)");

        var result = await AIApiClient.GetSkillMatchAsync(requirement, projId);

        if (result == null || result.Candidates == null || !result.Candidates.Any())
        {
            ConsoleUIHelper.ShowError("No matching resources found.");
            ConsoleUIHelper.PressAnyKey();
            return;
        }

        Console.WriteLine("──────────────────────────────────────────────");
        Console.WriteLine("AI-MATCHED RESULTS");
        Console.WriteLine("──────────────────────────────────────────────");
        Console.WriteLine($"{"#",-3} {"Name",-14} {"Skills Match",-22} {"Availability"}");
        
        for (int i = 0; i < result.Candidates.Count; i++)
        {
            var c = result.Candidates[i];
            var skills = c.Skills.Length > 20 ? c.Skills.Substring(0, 17) + "..." : c.Skills;
            Console.WriteLine($"{i + 1,-3} {c.Name,-14} {skills,-22} {c.Availability}");
        }
        
        Console.WriteLine();
        Console.WriteLine($"Note: {result.Note}");
        Console.WriteLine("──────────────────────────────────────────────");

        var optStr = ConsoleUIHelper.Prompt("Select employee (enter #, or 0 to search again)");
        if (!int.TryParse(optStr, out var opt) || opt <= 0 || opt > result.Candidates.Count)
        {
            return;
        }

        var candidate = result.Candidates[opt - 1];
        await DirectAllocateEmployeeAsync(projId, candidate.EmployeeId, candidate.SuggestedAllocationPercent);
    }

    private static async Task DirectAllocateEmployeeAsync(int projId, int empId, int defaultUtil = 0)
    {
        try
        {
            var emp = await EmployeeApiClient.GetByIdAsync(empId);
            var proj = await ProjectApiClient.GetByIdAsync(projId);

            if (emp == null || proj == null) return;

            Console.WriteLine();
            Console.WriteLine($"── {emp.FullName} ".PadRight(46, '─'));
            
            var allAllocations = await AllocationApiClient.GetAllAsync();
            var allocations = allAllocations.Where(a => a.EmployeeId == empId && a.ToDate >= DateTime.Now.Date).ToList();
            var currentUtil = allocations.Sum(a => a.UtilizationPercent);
            
            Console.WriteLine($"Current Utilisation: {currentUtil:0}%   ({emp.Status.ToLower()})");
            Console.WriteLine();
            
            Console.WriteLine("Set Allocation:");
            var utilStr = ConsoleUIHelper.Prompt($"  Utilisation % (Suggested: {defaultUtil})");
            var utilPercent = string.IsNullOrWhiteSpace(utilStr) ? defaultUtil : decimal.Parse(utilStr);
            
            var fromStr = ConsoleUIHelper.Prompt("  From Date      ");
            if (!DateTime.TryParseExact(fromStr, "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out var fromDate))
            {
                if (!DateTime.TryParse(fromStr, out fromDate)) return;
            }
            
            var toStr = ConsoleUIHelper.Prompt("  To Date        ");
            if (!DateTime.TryParseExact(toStr, "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out var toDate))
            {
                if (!DateTime.TryParse(toStr, out toDate)) return;
            }

            Console.WriteLine();
            Console.WriteLine("Validating...");
            var newTotal = currentUtil + utilPercent;
            var validationIcon = newTotal <= 100 ? "✓ Valid" : "⚠ Exceeds 100%";
            Console.WriteLine($"  {emp.FullName} total in this period: {currentUtil:0}% + {utilPercent:0}% = {newTotal:0}%   {validationIcon}");
            
            if (newTotal > 100)
            {
                ConsoleUIHelper.ShowError("Allocation rejected by validation.");
                ConsoleUIHelper.PressAnyKey();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("[C] Confirm Allocation     [B] Back");
            Console.WriteLine();
            
            var option = ConsoleUIHelper.Prompt("Enter option").ToUpper();
            if (option == "C")
            {
                await AllocationApiClient.CreateAsync(new CreateAllocationDto
                {
                    EmployeeId = empId,
                    ProjectId = projId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    UtilizationPercent = utilPercent
                });
                ConsoleUIHelper.ShowSuccess($"Allocation saved. {emp.FullName} → {proj.ProjectName} ({utilPercent:0}%) ✓");
                ConsoleUIHelper.PressAnyKey();
            }
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError(ex.Message);
            ConsoleUIHelper.PressAnyKey();
        }
    }

    private static async Task DirectAllocationAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("DIRECT ALLOCATION");
        
        var projIdStr = ConsoleUIHelper.Prompt("Select Project (ID)");
        var empIdStr = ConsoleUIHelper.Prompt("Enter Employee ID");

        if (!int.TryParse(projIdStr, out var projId) || !int.TryParse(empIdStr, out var empId)) return;

        try
        {
            var emp = await EmployeeApiClient.GetByIdAsync(empId);
            var proj = await ProjectApiClient.GetByIdAsync(projId);

            if (emp == null || proj == null)
            {
                ConsoleUIHelper.ShowError("Employee or Project not found.");
                ConsoleUIHelper.PressAnyKey();
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"── {emp.FullName} ".PadRight(46, '─'));
            
            var allAllocations = await AllocationApiClient.GetAllAsync();
            var allocations = allAllocations.Where(a => a.EmployeeId == empId && a.ToDate >= DateTime.Now.Date).ToList();
            var currentUtil = allocations.Sum(a => a.UtilizationPercent);
            
            Console.WriteLine($"Current Utilisation: {currentUtil:0}%   ({emp.Status.ToLower()})");
            Console.WriteLine();
            
            Console.WriteLine("Set Allocation:");
            var utilStr = ConsoleUIHelper.Prompt("  Utilisation %  ");
            if (!decimal.TryParse(utilStr, out var utilPercent)) return;
            
            var fromStr = ConsoleUIHelper.Prompt("  From Date      ");
            if (!DateTime.TryParseExact(fromStr, "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out var fromDate))
            {
                // Fallback to simpler parsing if they don't use strict format
                if (!DateTime.TryParse(fromStr, out fromDate)) return;
            }
            
            var toStr = ConsoleUIHelper.Prompt("  To Date        ");
            if (!DateTime.TryParseExact(toStr, "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out var toDate))
            {
                if (!DateTime.TryParse(toStr, out toDate)) return;
            }

            Console.WriteLine();
            Console.WriteLine("Validating...");
            var newTotal = currentUtil + utilPercent;
            var validationIcon = newTotal <= 100 ? "✓ Valid" : "⚠ Exceeds 100%";
            Console.WriteLine($"  {emp.FullName} total in this period: {currentUtil:0}% + {utilPercent:0}% = {newTotal:0}%   {validationIcon}");
            
            if (newTotal > 100)
            {
                ConsoleUIHelper.ShowError("Allocation rejected by validation.");
                ConsoleUIHelper.PressAnyKey();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("[C] Confirm     [B] Back");
            Console.WriteLine();
            
            var option = ConsoleUIHelper.Prompt("Enter option").ToUpper();
            if (option == "C")
            {
                await AllocationApiClient.CreateAsync(new CreateAllocationDto
                {
                    EmployeeId = empId,
                    ProjectId = projId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    UtilizationPercent = utilPercent
                });
                ConsoleUIHelper.ShowSuccess("Resource allocated successfully!");
                ConsoleUIHelper.PressAnyKey();
            }
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError(ex.Message);
            ConsoleUIHelper.PressAnyKey();
        }
    }

    private static async Task EndAllocationAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("END ALLOCATION");
        
        var projIdStr = ConsoleUIHelper.Prompt("Select Project (ID)");
        if (!int.TryParse(projIdStr, out var projId)) return;

        try
        {
            var proj = await ProjectApiClient.GetByIdAsync(projId);
            if (proj == null) return;
            
            Console.WriteLine();
            Console.WriteLine($"Active Allocations on this project ({proj.ProjectName}):");
            Console.WriteLine($"  {"#",-3} {"Employee",-14} {"%",-5} {"From",-12} {"To"}");
            
            var allAllocations = await AllocationApiClient.GetAllAsync();
            var activeAllocations = allAllocations.Where(a => a.ProjectId == projId && a.ToDate >= DateTime.Now.Date).ToList();
            
            for (int i = 0; i < activeAllocations.Count; i++)
            {
                var alloc = activeAllocations[i];
                var empName = alloc.EmployeeName.Length > 13 ? alloc.EmployeeName.Substring(0, 10) + "..." : alloc.EmployeeName;
                Console.WriteLine($"  {i + 1 + ".",-3} {empName,-14} {$"{alloc.UtilizationPercent:0}%",-5} {alloc.FromDate:dd-MMM-yy,-12} {alloc.ToDate:dd-MMM-yy}");
            }
            Console.WriteLine(new string('─', 46));
            Console.WriteLine();
            
            var optStr = ConsoleUIHelper.Prompt("Select allocation to end");
            if (int.TryParse(optStr, out var opt) && opt > 0 && opt <= activeAllocations.Count)
            {
                var alloc = activeAllocations[opt - 1];
                Console.WriteLine();
                Console.WriteLine($"End {alloc.EmployeeName}'s allocation on {proj.ProjectName}?");
                Console.WriteLine($"Set end date to today ({DateTime.Now:dd-MMM-yyyy})?");
                Console.WriteLine();
                Console.WriteLine("[Y] Yes, End Now    [B] Back");
                Console.WriteLine();
                
                var input = ConsoleUIHelper.Prompt("Enter option").ToUpper();
                if (input == "Y")
                {
                    await AllocationApiClient.EndAllocationAsync(alloc.Id);
                    ConsoleUIHelper.ShowSuccess($"Allocation ended. {alloc.EmployeeName} freed from {proj.ProjectName} as of {DateTime.Now:dd-MMM-yyyy}. ✓");
                    ConsoleUIHelper.PressAnyKey();
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError(ex.Message);
            ConsoleUIHelper.PressAnyKey();
        }
    }
}
