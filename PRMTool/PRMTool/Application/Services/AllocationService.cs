using Application.DTOs.Allocation;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services;

public class AllocationService : IAllocationService
{
    private readonly IAllocationRepository _allocationRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public AllocationService(IAllocationRepository allocationRepository, IEmployeeRepository employeeRepository)
    {
        _allocationRepository = allocationRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<int> CreateAllocationAsync(CreateAllocationRequestDto request)
    {
        if (request.ToDate < request.FromDate)
        {
            throw new Exception("To Date cannot be before From Date.");
        }

        var existingAllocations = await _allocationRepository.GetByEmployeeIdAsync(request.EmployeeId);

        var currentUtilization = existingAllocations.Sum(a => a.UtilizationPercent);

        if (currentUtilization + request.UtilizationPercent > 100)
        {
            throw new Exception("Employee allocation cannot exceed 100%.");
        }

        var allocation = new Allocation
        {
            EmployeeId = request.EmployeeId,
            ProjectId = request.ProjectId,
            UtilizationPercent = request.UtilizationPercent,
            FromDate = request.FromDate,
            ToDate = request.ToDate
        };

        await _allocationRepository.AddAsync(allocation);

        await _allocationRepository.SaveChangesAsync();

        return allocation.Id;
    }

    public async Task<List<AllocationDto>> GetAllAsync()
    {
        var allocations = await _allocationRepository.GetAllAsync();

        return allocations.Select(a =>
            new AllocationDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.FullName,

                ProjectId = a.ProjectId,
                ProjectName = a.Project.ProjectName,

                UtilizationPercent = a.UtilizationPercent,

                FromDate = a.FromDate,
                ToDate = a.ToDate
            }).ToList();
    }

    public async Task<List<AllocationDto>> GetByProjectIdAsync(int projectId)
    {
        var allocations = await _allocationRepository.GetByProjectIdAsync(projectId);

        return allocations.Select(a =>
            new AllocationDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.FullName,

                ProjectId = a.ProjectId,
                ProjectName = a.Project.ProjectName,

                UtilizationPercent = a.UtilizationPercent,

                FromDate = a.FromDate,
                ToDate = a.ToDate
            }).ToList();
    }

    public async Task UpdateAllocationAsync(int allocationId, UpdateAllocationRequestDto request)
    {
        var allocation = await _allocationRepository.GetByIdAsync(allocationId);

        if (allocation == null)
        {
            throw new Exception("Allocation not found.");
        }

        allocation.UtilizationPercent = request.UtilizationPercent;
        allocation.FromDate = request.FromDate;
        allocation.ToDate = request.ToDate;

        _allocationRepository.Update(allocation);
        await _allocationRepository.SaveChangesAsync();
    }

    public async Task EndAllocationAsync(int allocationId)
    {
        var allocation = await _allocationRepository.GetByIdAsync(allocationId);
        if (allocation == null)
            throw new Exception("Allocation not found.");

        allocation.ToDate = DateTime.UtcNow.Date;
        _allocationRepository.Update(allocation);
        await _allocationRepository.SaveChangesAsync();

        // Check if employee has any other active allocations
        var employeeAllocations = await _allocationRepository.GetByEmployeeIdAsync(allocation.EmployeeId);
        var hasActive = employeeAllocations.Any(a => a.ToDate.Date >= DateTime.UtcNow.Date && a.Id != allocationId);
        
        if (!hasActive)
        {
            var emp = await _employeeRepository.GetByIdAsync(allocation.EmployeeId);
            if (emp != null)
            {
                emp.Status = Domain.Enums.EmployeeStatus.Bench;
                _employeeRepository.Update(emp);
                await _employeeRepository.SaveChangesAsync();
            }
        }
    }
}