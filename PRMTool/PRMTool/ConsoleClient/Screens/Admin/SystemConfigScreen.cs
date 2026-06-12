using System;
using System.Threading.Tasks;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin;

public static class SystemConfigScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();
            ConsoleUIHelper.DrawHeader("SYSTEM CONFIGURATION");

            SystemConfigurationDto config;
            try
            {
                config = await SystemApiClient.GetConfigAsync();
            }
            catch (Exception ex)
            {
                ConsoleUIHelper.ShowError($"Failed to load configuration: {ex.Message}");
                ConsoleUIHelper.PressAnyKey();
                return;
            }

            Console.WriteLine("Current Settings:");
            Console.WriteLine($"  LLM Provider         : {config.LLMProvider}");
            
            string maskedApiKey = string.IsNullOrEmpty(config.ApiKey) 
                ? "(Not Set)" 
                : new string('*', Math.Min(24, Math.Max(8, config.ApiKey.Length)));
            Console.WriteLine($"  LLM API Key          : {maskedApiKey}");
            
            Console.WriteLine($"  Scheduler Interval   : {config.SchedulerInterval} hours");
            Console.WriteLine($"  Max Weekly Hours     : {config.MaxWeeklyHours}");
            
            ConsoleUIHelper.DrawSeparator();
            
            Console.WriteLine("1. Update LLM API Key");
            Console.WriteLine("2. Change LLM Provider  (Gemini / Groq)");
            Console.WriteLine("3. Update Scheduler Interval");
            Console.WriteLine("4. Update Max Weekly Hours");
            Console.WriteLine("5. Back");
            Console.WriteLine();
            
            var choiceStr = ConsoleUIHelper.Prompt("Enter option");

            if (!int.TryParse(choiceStr, out int choice))
            {
                ConsoleUIHelper.ShowError("Invalid option.");
                ConsoleUIHelper.PressAnyKey();
                continue;
            }

            switch (choice)
            {
                case 1:
                    await UpdateApiKeyAsync(config);
                    break;
                case 2:
                    await ChangeLlmProviderAsync(config);
                    break;
                case 3:
                    await UpdateSchedulerIntervalAsync(config);
                    break;
                case 4:
                    await UpdateMaxWeeklyHoursAsync(config);
                    break;
                case 5:
                    return;
                default:
                    ConsoleUIHelper.ShowError("Invalid option.");
                    ConsoleUIHelper.PressAnyKey();
                    break;
            }
        }
    }

    private static async Task UpdateApiKeyAsync(SystemConfigurationDto config)
    {
        Console.WriteLine();
        var newKey = ConsoleUIHelper.PromptPassword("Enter new API Key");
        
        if (string.IsNullOrWhiteSpace(newKey))
        {
            ConsoleUIHelper.ShowWarning("API Key update cancelled.");
            ConsoleUIHelper.PressAnyKey();
            return;
        }

        config.ApiKey = newKey;
        await SaveConfigAsync(config);
    }

    private static async Task ChangeLlmProviderAsync(SystemConfigurationDto config)
    {
        Console.WriteLine();
        Console.WriteLine("Select LLM Provider:");
        Console.WriteLine("1. Google Gemini");
        Console.WriteLine("2. Groq");
        var providerChoice = ConsoleUIHelper.Prompt("Enter choice");

        if (providerChoice == "1")
        {
            config.LLMProvider = "Google Gemini";
        }
        else if (providerChoice == "2")
        {
            config.LLMProvider = "Groq";
        }
        else
        {
            ConsoleUIHelper.ShowWarning("Invalid selection. Provider update cancelled.");
            ConsoleUIHelper.PressAnyKey();
            return;
        }

        await SaveConfigAsync(config);
    }

    private static async Task UpdateSchedulerIntervalAsync(SystemConfigurationDto config)
    {
        Console.WriteLine();
        var intervalStr = ConsoleUIHelper.Prompt("Enter Scheduler Interval in hours");
        
        if (int.TryParse(intervalStr, out int interval) && interval > 0)
        {
            config.SchedulerInterval = interval;
            await SaveConfigAsync(config);
        }
        else
        {
            ConsoleUIHelper.ShowError("Invalid interval. Must be a positive integer.");
            ConsoleUIHelper.PressAnyKey();
        }
    }

    private static async Task UpdateMaxWeeklyHoursAsync(SystemConfigurationDto config)
    {
        Console.WriteLine();
        var hoursStr = ConsoleUIHelper.Prompt("Enter Max Weekly Hours");
        
        if (int.TryParse(hoursStr, out int hours) && hours > 0)
        {
            config.MaxWeeklyHours = hours;
            await SaveConfigAsync(config);
        }
        else
        {
            ConsoleUIHelper.ShowError("Invalid hours. Must be a positive integer.");
            ConsoleUIHelper.PressAnyKey();
        }
    }

    private static async Task SaveConfigAsync(SystemConfigurationDto config)
    {
        try
        {
            await SystemApiClient.UpdateConfigAsync(config);
            ConsoleUIHelper.ShowSuccess("Configuration updated successfully.");
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError($"Failed to update configuration: {ex.Message}");
        }
        ConsoleUIHelper.PressAnyKey();
    }
}
