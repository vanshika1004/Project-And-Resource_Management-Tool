using Application.DTOs.Allocation;

namespace Application.Interfaces.Services;

public interface IAllocationService
{
    Task<int> CreateAllocationAsync(CreateAllocationRequestDto request);

    Task<List<AllocationDto>> GetAllAsync();

    Task<List<AllocationDto>> GetByProjectIdAsync(int projectId);

    Task UpdateAllocationAsync(int allocationId, UpdateAllocationRequestDto request);

    Task EndAllocationAsync(int allocationId);
}