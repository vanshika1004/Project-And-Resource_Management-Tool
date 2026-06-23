using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PRM.Core.Entities;

namespace PRM.Core.Interfaces;

public interface ITimesheetRepository
{
    Task<Timesheet?> GetByResourceAndWeekAsync(int resourceId, DateTime weekStartDate);
    Task<IEnumerable<Timesheet>> GetByResourceIdAsync(int resourceId);
    Task<IEnumerable<Timesheet>> GetTeamTimesheetsAsync(int managerId, DateTime weekStartDate);
    Task<IEnumerable<Timesheet>> GetRecentByProjectAsync(int projectId, int weeks = 4);
    Task AddAsync(Timesheet timesheet);
    Task UpdateAsync(Timesheet timesheet);
}
