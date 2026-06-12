using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleClient.ApiClients;

public class TimesheetApiClient : ApiClientBase
{
    public static async Task<List<TimesheetDto>> GetMyTimesheetsAsync()
    {
        return await GetAsync<List<TimesheetDto>>("timesheets/my") ?? new List<TimesheetDto>();
    }

    public static async Task<List<TimesheetDto>> GetTeamTimesheetsAsync()
    {
        return await GetAsync<List<TimesheetDto>>("timesheets/team") ?? new List<TimesheetDto>();
    }

    public static async Task<TimesheetDto> SubmitTimesheetAsync(SubmitTimesheetDto dto)
    {
        var result = await PostAsync<TimesheetDto>("timesheets", dto);
        return result ?? throw new Exception("Empty response from server.");
    }

    public static async Task UpdateStatusAsync(int id, string status)
    {
        await PutAsync($"timesheets/{id}/status", new { Status = status });
    }
}

public class TimesheetDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime WeekStartDate { get; set; }
    public decimal TotalHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<TimesheetEntryDto> Entries { get; set; } = new();
}

public class TimesheetEntryDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal HoursWorked { get; set; }
    public string ActivityTags { get; set; } = string.Empty;
}

public class SubmitTimesheetDto
{
    public int EmployeeId { get; set; }
    public DateTime WeekStartDate { get; set; }
    public List<SubmitTimesheetEntryDto> Entries { get; set; } = new();
}

public class SubmitTimesheetEntryDto
{
    public int ProjectId { get; set; }
    public decimal HoursWorked { get; set; }
    public string ActivityTags { get; set; } = string.Empty;
}
