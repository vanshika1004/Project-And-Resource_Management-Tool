using System;
using System.Threading.Tasks;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class SystemConfigScreen
{
    private readonly ApiClient _api;

    public SystemConfigScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.Clear();
            ConsoleUI.PrintHeader("SYSTEM CONFIGURATION");

            try
            {
                var configs = await _api.GetAsync<System.Collections.Generic.List<PRM.ConsoleClient.Models.SystemConfigDto>>("SystemConfig");
                if (configs != null && configs.Count > 0)
                {
                    Console.WriteLine("CURRENT SETTINGS:");
                    foreach (var config in configs)
                    {
                        string val = config.Value;
                        if (config.Key.ToLower().Contains("key") || config.Key.ToLower().Contains("password"))
                        {
                            val = new string('*', val.Length > 0 ? 8 : 0);
                        }
                        Console.WriteLine($"  {config.Key,-20} : {val}");
                    }
                    Console.WriteLine("──────────────────────────────────────────────\n");
                }
            }
            catch
            {
                // Ignore API error for display
            }

            Console.WriteLine("1. Update LLM API Key");
            Console.WriteLine("2. Change LLM Provider (Gemini/Groq/Custom)");
            Console.WriteLine("3. Update Custom LLM Endpoint URL");
            Console.WriteLine("4. Update Custom LLM Model Name");
            Console.WriteLine("5. Update Scheduler Interval");
            Console.WriteLine("6. Update Max Weekly Hours");
            Console.WriteLine("7. Configure SMTP Settings");
            Console.WriteLine("8. Back");
            Console.WriteLine();

            string option = ConsoleUI.ReadInput("Enter option");

            if (option == "8") return;

            string key = "";
            string value = "";

            switch (option)
            {
                case "1":
                    key = "LlmApiKey";
                    value = ConsoleUI.ReadInput("Enter new API Key");
                    break;
                case "2":
                    key = "LlmProvider";
                    Console.WriteLine("Provider: (1) Gemini  (2) Groq  (3) Custom");
                    string prov = ConsoleUI.ReadInput("Enter choice");
                    value = prov == "1" ? "Gemini" : prov == "2" ? "Groq" : "Custom";
                    break;
                case "3":
                    key = "LlmEndpointUrl";
                    value = ConsoleUI.ReadInput("Enter Custom Endpoint URL");
                    break;
                case "4":
                    key = "LlmModel";
                    value = ConsoleUI.ReadInput("Enter Custom Model Name (e.g. gemma)");
                    break;
                case "5":
                    key = "SchedulerInterval";
                    value = ConsoleUI.ReadInput("Enter interval in hours");
                    break;
                case "6":
                    key = "MaxWeeklyHours";
                    value = ConsoleUI.ReadInput("Enter max hours [Default: 40]");
                    break;
                case "7":
                    Console.WriteLine("Select Setting:");
                    Console.WriteLine("1. SMTP Host");
                    Console.WriteLine("2. SMTP Port");
                    Console.WriteLine("3. SMTP Username");
                    Console.WriteLine("4. SMTP Password");
                    Console.WriteLine("5. From Email");
                    var smtpOpt = ConsoleUI.ReadInput("Enter choice");
                    key = smtpOpt switch
                    {
                        "1" => "SmtpHost",
                        "2" => "SmtpPort",
                        "3" => "SmtpUser",
                        "4" => "SmtpPassword",
                        "5" => "FromEmail",
                        _ => ""
                    };
                    if (string.IsNullOrEmpty(key)) continue;
                    value = ConsoleUI.ReadInput($"Enter new value for {key}");
                    break;
                default:
                    ConsoleUI.PrintError("Invalid option. Please try again.");
                    continue;
            }

            try
            {
                var payload = new { Key = key, Value = value };
                await _api.PostAsync<object>("SystemConfig", payload);
                ConsoleUI.PrintSuccess($"{key} updated to {value}.");
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintError($"Failed to update configuration: {ex.Message}");
                Console.ReadLine();
            }
        }
    }
}
