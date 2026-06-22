using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PRM.Application.DTOs;

namespace PRM.Application.Interfaces;

public interface IAllocationService
{
    Task<IEnumerable<AllocationDto>> GetAllAllocationsAsync();
    Task<IEnumerable<AllocationDto>> GetAllocationsByResourceAsync(int resourceId);
    Task<AllocationDto> AllocateResourceAsync(CreateAllocationRequest request, int requestedByUserId);
    Task DeallocateResourceAsync(int allocationId, DateTime endDate, int requestedByUserId);
}
