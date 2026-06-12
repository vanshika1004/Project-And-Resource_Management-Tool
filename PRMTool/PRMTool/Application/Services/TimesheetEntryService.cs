using Application.DTOs.Timesheet;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services;

public class TimesheetEntryService
    : ITimesheetEntryService
{
    private readonly ITimesheetEntryRepository
        _entryRepository;

    private readonly ITimesheetRepository
        _timesheetRepository;

    private readonly IAllocationRepository
    _allocationRepository;

    public TimesheetEntryService(
        ITimesheetEntryRepository entryRepository,
        ITimesheetRepository timesheetRepository,
        IAllocationRepository allocationRepository)
    {
        _entryRepository = entryRepository;
        _timesheetRepository = timesheetRepository;
        _allocationRepository = allocationRepository;
    }

    public async Task<int> CreateEntryAsync(
        CreateTimesheetEntryRequestDto request)
    {
        if (request.HoursWorked <= 0)
        {
            throw new Exception(
                "Hours worked must be greater than zero.");
        }

        var timesheet =
            await _timesheetRepository
                .GetByIdAsync(request.TimesheetId);

        if (timesheet == null)
        {
            throw new Exception(
                "Timesheet not found.");
        }

        var employeeAllocations =
            await _allocationRepository
                .GetByEmployeeIdAsync(
                    timesheet.EmployeeId);

        var allocationExists =
            employeeAllocations.Any(a =>
                a.ProjectId == request.ProjectId);

        if (!allocationExists)
        {
            throw new Exception(
                "Employee is not allocated to this project.");
        }

        var entry = new TimesheetEntry
        {
            TimesheetId = request.TimesheetId,
            ProjectId = request.ProjectId,
            HoursWorked = request.HoursWorked
        };

        await _entryRepository.AddAsync(entry);

        await _entryRepository.SaveChangesAsync();

            timesheet.TotalHours += request.HoursWorked;

            _timesheetRepository.Update(timesheet);

            await _timesheetRepository
                .SaveChangesAsync();

        return entry.Id;
    }

    public async Task<List<TimesheetEntryDto>>
        GetByTimesheetIdAsync(int timesheetId)
    {
        var entries =
            await _entryRepository
                .GetByTimesheetIdAsync(timesheetId);

        return entries.Select(e =>
            new TimesheetEntryDto
            {
                Id = e.Id,
                ProjectId = e.ProjectId,
                ProjectName = e.Project.ProjectName,
                HoursWorked = e.HoursWorked
            }).ToList();
    }
}