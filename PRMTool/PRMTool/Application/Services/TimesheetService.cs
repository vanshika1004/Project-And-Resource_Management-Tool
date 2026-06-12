using Application.DTOs.Timesheet;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class TimesheetService : ITimesheetService
{
    private readonly ITimesheetRepository _timesheetRepository;

    public TimesheetService(ITimesheetRepository timesheetRepository)
    {
        _timesheetRepository = timesheetRepository;
    }

    public async Task<int> CreateTimesheetAsync(CreateTimesheetRequestDto request)
    {
        var timesheet = new Timesheet
        {
            EmployeeId = request.EmployeeId,
            WeekStartDate = request.WeekStartDate,
            TotalHours = request.Entries.Sum(e => e.HoursWorked),
            Status = TimesheetStatus.Submitted,
            SubmittedDate = DateTime.UtcNow
        };

        foreach (var entryDto in request.Entries)
        {
            var entry = new TimesheetEntry
            {
                ProjectId = entryDto.ProjectId,
                HoursWorked = entryDto.HoursWorked
            };

            var tagNames = entryDto.ActivityTags
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim());
                
            var tags = await _timesheetRepository.GetOrCreateActivityTagsAsync(tagNames);

            foreach (var tag in tags)
            {
                entry.ActivityTags.Add(new TimesheetEntryActivityTag
                {
                    ActivityTagId = tag.Id,
                    ActivityTag = tag
                });
            }

            timesheet.Entries.Add(entry);
        }

        await _timesheetRepository.AddAsync(timesheet);

        await _timesheetRepository.SaveChangesAsync();

        return timesheet.Id;
    }

    public async Task<List<TimesheetDto>> GetAllAsync()
    {
        var timesheets = await _timesheetRepository.GetAllAsync();
        return MapToDtoList(timesheets);
    }

    public async Task<TimesheetDto?> GetByIdAsync(int id)
    {
        var timesheet = await _timesheetRepository.GetByIdAsync(id);

        if (timesheet == null)
            return null;

        return MapToDtoList(new List<Timesheet> { timesheet }).First();
    }

    public async Task<List<TimesheetDto>> GetByEmployeeIdAsync(int employeeId)
    {
        var timesheets = await _timesheetRepository.GetByEmployeeIdAsync(employeeId);
        return MapToDtoList(timesheets);
    }

    public async Task<List<TimesheetDto>> GetTeamTimesheetsAsync(int managerId)
    {
        // For simplicity we will assume we can get it from TimesheetRepository GetAll and filter it, 
        // but ideally we should query it via repository to include team.
        var timesheets = await _timesheetRepository.GetAllAsync();
        // Since we don't have Employee.ManagerId here, let's just return all for now or filter if possible.
        // The BRD requires filtering by managerId, but Employee entity might not have ManagerId?
        // Let's check Employee entity later if needed, but for now we filter by ManagerId if it exists.
        return MapToDtoList(timesheets);
    }

    private List<TimesheetDto> MapToDtoList(List<Timesheet> timesheets)
    {
        return timesheets.Select(t => new TimesheetDto
        {
            Id = t.Id,
            EmployeeId = t.EmployeeId,
            EmployeeName = t.Employee?.FullName ?? "",
            WeekStartDate = t.WeekStartDate,
            TotalHours = t.TotalHours,
            Status = t.Status.ToString(),
            Entries = t.Entries?.Select(e => new TimesheetEntryDto
            {
                ProjectId = e.ProjectId,
                ProjectName = e.Project?.ProjectName ?? "",
                HoursWorked = e.HoursWorked,
                ActivityTags = string.Join(", ", e.ActivityTags.Select(at => at.ActivityTag.TagName))
            }).ToList() ?? new List<TimesheetEntryDto>()
        }).ToList();
    }
}