using Domain.Common;

namespace Domain.Entities;

public class ActivityTag : BaseEntity
{
    public string TagName { get; set; } = string.Empty;

    // Navigation Properties

    public ICollection<TimesheetEntryActivityTag> TimesheetEntryTags { get; set; }
        = new List<TimesheetEntryActivityTag>();
}