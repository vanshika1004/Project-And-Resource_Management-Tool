using System.Threading.Tasks;

namespace ConsoleClient.ApiClients;

public class SystemApiClient : ApiClientBase
{
    public static async Task TriggerMaintenanceAsync()
    {
        await PostAsync("system/run-maintenance", new { });
    }

    public static async Task<SystemConfigurationDto> GetConfigAsync()
    {
        return await GetAsync<SystemConfigurationDto>("system/config") ?? new SystemConfigurationDto();
    }

    public static async Task UpdateConfigAsync(SystemConfigurationDto dto)
    {
        await PutAsync("system/config", dto);
    }
}

public class SystemConfigurationDto
{
    public string LLMProvider { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int SchedulerInterval { get; set; }
    public int MaxWeeklyHours { get; set; }
}
