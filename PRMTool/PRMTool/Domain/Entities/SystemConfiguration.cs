using Domain.Common;

namespace Domain.Entities;

public class SystemConfiguration : BaseEntity
{
    public string LLMProvider { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public int SchedulerInterval { get; set; }

    public int MaxWeeklyHours { get; set; }
}