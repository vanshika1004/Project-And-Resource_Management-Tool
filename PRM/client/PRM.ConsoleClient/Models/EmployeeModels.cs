using System;
using System.Collections.Generic;

namespace PRM.ConsoleClient.Models;

public record TimesheetDto(int Id, int UserId, DateTime WeekStartDate, string Status, DateTime? SubmittedAt, IEnumerable<TimesheetEntryDto> Entries);
public record TimesheetEntryDto(int Id, int ProjectId, string ProjectName, decimal HoursWorked, string? ActivityTags);

public record SubmitTimesheetRequest(int UserId, DateTime WeekStartDate, IEnumerable<CreateTimesheetEntryRequest> Entries);
public record CreateTimesheetEntryRequest(int ProjectId, decimal HoursWorked, string? ActivityTags);
