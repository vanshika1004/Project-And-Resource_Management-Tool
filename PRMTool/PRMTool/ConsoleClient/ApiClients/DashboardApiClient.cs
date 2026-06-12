using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleClient.ApiClients;

public class DashboardApiClient : ApiClientBase
{
    public static async Task<ManagerDashboardDto> GetManagerDashboardAsync()
    {
        var result = await GetAsync<ManagerDashboardDto>("dashboard/resources");
        return result ?? new ManagerDashboardDto();
    }

    public static async Task<EmployeeDetailDto> GetEmployeeDetailAsync(int employeeId)
    {
        var result = await GetAsync<EmployeeDetailDto>($"dashboard/resources/{employeeId}");
        return result ?? throw new Exception("Employee not found.");
    }

    public static async Task<ProjectDashboardDto> GetProjectDashboardAsync()
    {
        var result = await GetAsync<ProjectDashboardDto>("dashboard/projects");
        return result ?? new ProjectDashboardDto();
    }
}

public class ManagerDashboardDto
{
    public List<ActiveEmployeeDto> ActiveEmployees { get; set; } = new();
    public List<BenchEmployeeDto> BenchEmployees { get; set; } = new();
    public int BenchCount { get; set; }
    public int PartiallyAllocatedCount { get; set; }
}

public class ActiveEmployeeDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public decimal TotalAllocationPercent { get; set; }
    public string Availability { get; set; } = string.Empty;
}

public class BenchEmployeeDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
}

public class EmployeeAllocationDto
{
    public string ProjectName { get; set; } = string.Empty;
    public decimal UtilizationPercent { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class EmployeeDetailDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAllocationPercent { get; set; }
    public List<string> ProfileSkills { get; set; } = new();
    public List<EmployeeAllocationDto> ActiveAllocations { get; set; } = new();
    public List<string> RecentActivityTags { get; set; } = new();
}

public class ProjectDashboardItemDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public int TotalStoryPoints { get; set; }
    public int CompletedStoryPoints { get; set; }
    public decimal ProgressPercentage { get; set; }
    public int ResourceCount { get; set; }
    public List<ProjectMilestoneSummaryDto> Milestones { get; set; } = new();
}

public class ProjectDashboardDto
{
    public List<ProjectDashboardItemDto> Projects { get; set; } = new();
}

public class ProjectMilestoneSummaryDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int StoryPoints { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
}
