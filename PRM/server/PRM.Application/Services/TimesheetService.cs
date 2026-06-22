using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRM.Application.DTOs;
using PRM.Application.Exceptions;
using PRM.Application.Interfaces;
using PRM.Core.Constants;
using PRM.Core.Entities;
using PRM.Core.Enums;
using PRM.Core.Interfaces;

namespace PRM.Application.Services;

public class TimesheetService : ITimesheetService
{
    private readonly ITimesheetRepository _timesheetRepo;
    private readonly IAllocationRepository _allocationRepo;
    private readonly IUserRepository _userRepo;

    public TimesheetService(ITimesheetRepository timesheetRepo, IAllocationRepository allocationRepo, IUserRepository userRepo)
    {
        _timesheetRepo = timesheetRepo;
        _allocationRepo = allocationRepo;
        _userRepo = userRepo;
    }

    public async Task<TimesheetDto> SubmitTimesheetAsync(SubmitTimesheetRequest request, int requestedByUserId)
    {
        // Security Rule
        if (request.UserId != requestedByUserId)
            throw new BusinessRuleViolationException("You can only submit timesheets for yourself.");

        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user != null && user.IsTimesheetFrozen)
        {
            throw new BusinessRuleViolationException("Your timesheet access is frozen due to missing submissions. Please contact your manager.");
        }

        var weekStart = request.WeekStartDate.Date;
        var totalHours = request.Entries.Sum(e => e.HoursWorked);
        
        // BRD Rule: Maximum 40 hours 
        if (totalHours > PrmConstants.DefaultMaxWeeklyHours)
            throw new BusinessRuleViolationException($"Total weekly hours cannot exceed {PrmConstants.DefaultMaxWeeklyHours}. Submitted: {totalHours}");

        // BRD Rule: Can only log hours against allocated projects
        var activeAllocations = await _allocationRepo.GetActiveForWeekAsync(request.UserId, weekStart);
        var activeProjectIds = activeAllocations.Select(a => a.ProjectId).ToHashSet();

        foreach (var entry in request.Entries)
        {
            if (!activeProjectIds.Contains(entry.ProjectId))
                throw new BusinessRuleViolationException($"Cannot log hours against Project ID {entry.ProjectId}. You are not allocated to this project for the week of {weekStart:yyyy-MM-dd}.");
        }

        var existing = await _timesheetRepo.GetByResourceAndWeekAsync(request.UserId, weekStart);
        if (existing != null)
            throw new BusinessRuleViolationException("A timesheet for this week has already been submitted.");

        var timesheet = new Timesheet
        {
            UserId = request.UserId,
            WeekStartDate = weekStart,
            Status = TimesheetStatus.Submitted,
            SubmittedAt = DateTime.UtcNow,
            Entries = request.Entries.Select(e => new TimesheetEntry
            {
                ProjectId = e.ProjectId,
                HoursWorked = e.HoursWorked,
                ActivityTags = e.ActivityTags
            }).ToList()
        };

        await _timesheetRepo.AddAsync(timesheet);

        return new TimesheetDto(
            timesheet.Id, timesheet.UserId, user?.FullName ?? "Unknown", timesheet.WeekStartDate, timesheet.Status, timesheet.SubmittedAt,
            timesheet.Entries.Select(e => new TimesheetEntryDto(e.Id, e.ProjectId, e.Project?.Name ?? "Unknown", e.HoursWorked, e.ActivityTags))
        );
    }

    public async Task<IEnumerable<TimesheetDto>> GetMyTimesheetsAsync(int resourceId)
    {
        var timesheets = await _timesheetRepo.GetByResourceIdAsync(resourceId);
        return MapToDto(timesheets);
    }

    public async Task<IEnumerable<TimesheetDto>> GetTeamTimesheetsAsync(int managerId, DateTime weekStartDate)
    {
        var timesheets = await _timesheetRepo.GetTeamTimesheetsAsync(managerId, weekStartDate.Date);
        return MapToDto(timesheets);
    }

    private IEnumerable<TimesheetDto> MapToDto(IEnumerable<Timesheet> timesheets)
    {
        return timesheets.Select(t => new TimesheetDto(
            t.Id, t.UserId, t.User?.FullName ?? "Unknown", t.WeekStartDate, t.Status, t.SubmittedAt,
            t.Entries.Select(e => new TimesheetEntryDto(e.Id, e.ProjectId, e.Project?.Name ?? "Unknown", e.HoursWorked, e.ActivityTags))
        ));
    }
}
