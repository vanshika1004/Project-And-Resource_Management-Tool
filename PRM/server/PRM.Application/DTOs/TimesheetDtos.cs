using System;
using System.Collections.Generic;
using PRM.Core.Enums;

namespace PRM.Application.DTOs;

public record TimesheetDto(int Id, int UserId, string UserFullName, DateTime WeekStartDate, TimesheetStatus Status, DateTime? SubmittedAt, IEnumerable<TimesheetEntryDto> Entries);
public record TimesheetEntryDto(int Id, int ProjectId, string ProjectName, decimal HoursWorked, string? ActivityTags);

public record SubmitTimesheetRequest(int UserId, DateTime WeekStartDate, IEnumerable<CreateTimesheetEntryRequest> Entries);
public record CreateTimesheetEntryRequest(int ProjectId, decimal HoursWorked, string? ActivityTags);
