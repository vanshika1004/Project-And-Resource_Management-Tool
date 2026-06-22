using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PRM.Application.DTOs;

namespace PRM.Application.Interfaces;

public interface ITimesheetService
{
    Task<TimesheetDto> SubmitTimesheetAsync(SubmitTimesheetRequest request, int requestedByUserId);
    Task<IEnumerable<TimesheetDto>> GetMyTimesheetsAsync(int resourceId);
    Task<IEnumerable<TimesheetDto>> GetTeamTimesheetsAsync(int managerId, DateTime weekStartDate);
}
