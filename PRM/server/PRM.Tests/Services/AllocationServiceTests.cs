using Moq;
using PRM.Application.DTOs;
using PRM.Application.Services;
using PRM.Core.Entities;
using PRM.Core.Interfaces;
using PRM.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PRM.Tests.Services;

public class AllocationServiceTests
{
    private readonly Mock<IAllocationRepository> _mockAllocationRepo;
    private readonly Mock<IProjectRepository> _mockProjectRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly AllocationService _allocationService;

    public AllocationServiceTests()
    {
        _mockAllocationRepo = new Mock<IAllocationRepository>();
        _mockProjectRepo = new Mock<IProjectRepository>();
        _mockUserRepo = new Mock<IUserRepository>();

        _allocationService = new AllocationService(
            _mockAllocationRepo.Object,
            _mockProjectRepo.Object,
            _mockUserRepo.Object
        );
    }

    [Fact]
    public async Task AllocateResourceAsync_ThrowsBusinessRuleViolation_WhenOverAllocated()
    {
        // Arrange
        var request = new CreateAllocationRequest(1, 1, 60, DateTime.Today, DateTime.Today.AddDays(10));
        
        var user = new User { Id = 1, Username = "u", Email = "e", PasswordHash = "", FullName = "U", IsActive = true };
        var project = new Project { Id = 1, Name = "P", ManagerId = 2 };
        var requester = new User { Id = 2, Username = "manager", Email = "m@m.com", PasswordHash = "", FullName = "Manager", Role = new Role { Name = "Manager" }, IsActive = true };
        
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(requester);
        _mockProjectRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);

        var existingAllocations = new List<Allocation> 
        { 
            new Allocation { UtilisationPercent = 50, FromDate = DateTime.Today.AddDays(-5), ToDate = DateTime.Today.AddDays(15) } 
        };
        _mockAllocationRepo.Setup(r => r.GetOverlappingAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(existingAllocations);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => _allocationService.AllocateResourceAsync(request, 2));
    }

    [Fact]
    public async Task AllocateResourceAsync_Succeeds_WhenValid()
    {
        // Arrange
        var request = new CreateAllocationRequest(1, 1, 40, DateTime.Today, DateTime.Today.AddDays(10));
        
        var user = new User { Id = 1, Username = "u", Email = "e", PasswordHash = "", FullName = "U", IsActive = true };
        var project = new Project { Id = 1, Name = "P", ManagerId = 2 };
        var requester = new User { Id = 2, Username = "manager", Email = "m@m.com", PasswordHash = "", FullName = "Manager", Role = new Role { Name = "Manager" }, IsActive = true };
        
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(requester);
        _mockProjectRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
        _mockAllocationRepo.Setup(r => r.GetOverlappingAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<Allocation>());

        // Act
        var result = await _allocationService.AllocateResourceAsync(request, 2);

        // Assert
        _mockAllocationRepo.Verify(r => r.AddAsync(It.IsAny<Allocation>()), Times.Once);
        Assert.Equal(40, result.UtilisationPercent);
    }
}
