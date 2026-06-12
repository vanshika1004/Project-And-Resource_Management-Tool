namespace Application.DTOs.System;

public class SystemConfigurationDto
{
    public string LLMProvider { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int SchedulerInterval { get; set; }
    public int MaxWeeklyHours { get; set; }
}
