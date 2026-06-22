using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRM.Application.DTOs;
using PRM.Application.Exceptions;
using PRM.Application.Interfaces;
using PRM.Core.Entities;
using PRM.Core.Interfaces;

namespace PRM.Application.Services;

public class AllocationService : IAllocationService
{
    private readonly IAllocationRepository _allocationRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly IUserRepository _userRepo;

    public AllocationService(
        IAllocationRepository allocationRepo,
        IProjectRepository projectRepo,
        IUserRepository userRepo)
    {
        _allocationRepo = allocationRepo;
        _projectRepo = projectRepo;
        _userRepo = userRepo;
    }

    public async Task<IEnumerable<AllocationDto>> GetAllAllocationsAsync()
    {
        var allocations = await _allocationRepo.GetAllAsync();
        // Assuming Allocation includes User. We might need to ensure User is included in the query.
        return allocations.Select(a => new AllocationDto(
            a.Id, a.UserId, a.User?.FullName ?? "Unknown", a.ProjectId, a.Project!.Name, a.UtilisationPercent, a.FromDate, a.ToDate));
    }

    public async Task<IEnumerable<AllocationDto>> GetAllocationsByResourceAsync(int resourceId)
    {
        var allocations = await _allocationRepo.GetByResourceIdAsync(resourceId);
        return allocations.Select(a => new AllocationDto(
            a.Id, a.UserId, a.User?.FullName ?? "Unknown", a.ProjectId, a.Project!.Name, a.UtilisationPercent, a.FromDate, a.ToDate));
    }

    public async Task<AllocationDto> AllocateResourceAsync(CreateAllocationRequest request, int requestedByUserId)
    {
        var project = await _projectRepo.GetByIdAsync(request.ProjectId);
        if (project == null) throw new NotFoundException("Project", request.ProjectId);
        
        var requester = await _userRepo.GetByIdAsync(requestedByUserId);
        if (requester == null) throw new NotFoundException("User", requestedByUserId);
        
        // Access Control Rule from BRD
        if (requester.Role!.Name == "Manager" && project.ManagerId != requestedByUserId)
        {
            throw new BusinessRuleViolationException("You can only allocate resources to your own assigned projects.");
        }

        var resource = await _userRepo.GetByIdAsync(request.UserId);
        if (resource == null || !resource.IsActive)
            throw new BusinessRuleViolationException("Cannot allocate an inactive or non-existent resource.");

        // BRD Rule: Total utilisation across overlapping allocations cannot exceed 100%
        var overlapping = await _allocationRepo.GetOverlappingAsync(request.UserId, request.FromDate, request.ToDate);
        int sumUtilisation = overlapping.Sum(a => a.UtilisationPercent);
        
        if (sumUtilisation + request.UtilisationPercent > 100)
        {
            throw new BusinessRuleViolationException($"Allocation failed: Resource utilisation would exceed 100%. Current overlapping utilisation is {sumUtilisation}%.");
        }

        var allocation = new Allocation
        {
            UserId = request.UserId,
            ProjectId = request.ProjectId,
            UtilisationPercent = request.UtilisationPercent,
            FromDate = request.FromDate,
            ToDate = request.ToDate
        };

        await _allocationRepo.AddAsync(allocation);

        return new AllocationDto(
            allocation.Id, allocation.UserId, resource.FullName, allocation.ProjectId, project.Name, allocation.UtilisationPercent, allocation.FromDate, allocation.ToDate);
    }

    public async Task DeallocateResourceAsync(int allocationId, DateTime endDate, int requestedByUserId)
    {
        var allocation = await _allocationRepo.GetByIdAsync(allocationId);
        if (allocation == null) throw new NotFoundException("Allocation", allocationId);

        var requester = await _userRepo.GetByIdAsync(requestedByUserId);
        if (requester == null) throw new NotFoundException("User", requestedByUserId);

        // Access Control Rule from BRD
        if (requester.Role!.Name == "Manager" && allocation.Project!.ManagerId != requestedByUserId)
        {
            throw new BusinessRuleViolationException("You can only deallocate resources from your own assigned projects.");
        }

        if (endDate < allocation.FromDate)
            throw new BusinessRuleViolationException("End date cannot be earlier than the start date of the allocation.");

        await _allocationRepo.EndAllocationAsync(allocationId, endDate);
    }
}
