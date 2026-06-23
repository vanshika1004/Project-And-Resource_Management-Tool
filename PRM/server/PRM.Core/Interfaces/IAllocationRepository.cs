using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PRM.Core.Entities;

namespace PRM.Core.Interfaces;

public interface IAllocationRepository
{
    Task<Allocation?> GetByIdAsync(int id);
    Task<IEnumerable<Allocation>> GetAllAsync();
    Task<IEnumerable<Allocation>> GetByResourceIdAsync(int resourceId);
    Task<IEnumerable<Allocation>> GetByProjectIdAsync(int projectId);
    Task<IEnumerable<Allocation>> GetOverlappingAsync(int resourceId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<Allocation>> GetActiveForWeekAsync(int resourceId, DateTime weekStart);
    Task<IEnumerable<Allocation>> GetActiveByResourceIdsAsync(IEnumerable<int> resourceIds);
    Task AddAsync(Allocation allocation);
    Task EndAllocationAsync(int id, DateTime endDate);
}
