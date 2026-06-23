using System;
using System.Collections.Generic;

namespace PRM.Application.DTOs;

public record TeamDashboardMemberDto(
    int Id,
    string FullName,
    string? Department,
    int AllocPercent,
    string Skills
);

public record TeamMemberDetailsDto(
    int Id,
    string FullName,
    string? Department,
    string Status,
    string Skills,
    List<ActiveAllocationDto> ActiveAllocations,
    List<string> RecentActivityTags
);

public record ActiveAllocationDto(
    string ProjectName,
    int Percent,
    DateTime FromDate,
    DateTime ToDate
);
