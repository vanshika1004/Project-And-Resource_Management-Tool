using Moq;
using PRM.Application.DTOs;
using PRM.Application.Services;
using PRM.Core.Entities;
using PRM.Core.Interfaces;
using PRM.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using PRM.Core.Enums;

namespace PRM.Tests.Services;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _mockProjectRepo;
    private readonly ProjectService _projectService;

    public ProjectServiceTests()
    {
        _mockProjectRepo = new Mock<IProjectRepository>();
        _projectService = new ProjectService(_mockProjectRepo.Object);
    }

    [Fact]
    public async Task GetAllProjectsAsync_ReturnsProjects()
    {
        // Arrange
        var projects = new List<Project> { new Project { Id = 1, Name = "Test Project" } };
        _mockProjectRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(projects);

        // Act
        var result = await _projectService.GetAllProjectsAsync();
        var resultList = result.ToList();

        // Assert
        Assert.Single(resultList);
        Assert.Equal("Test Project", resultList[0].Name);
    }

    [Fact]
    public async Task GetProjectByIdAsync_ThrowsNotFound_WhenProjectDoesNotExist()
    {
        // Arrange
        _mockProjectRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Project?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _projectService.GetProjectByIdAsync(1));
    }

    [Fact]
    public async Task CreateProjectAsync_CreatesProjectSuccessfully()
    {
        // Arrange
        var request = new CreateProjectRequest("New Project", "Desc", DateTime.Today, DateTime.Today.AddDays(10), 100, 1);
        
        // Act
        // _projectService.CreateProjectAsync will call _projectRepo.AddAsync, then GetProjectByIdAsync
        // So we need to mock GetByIdAsync to return something
        var createdProject = new Project { Id = 1, Name = "New Project", Description = "Desc" };
        _mockProjectRepo.Setup(r => r.GetByIdWithMilestonesAsync(It.IsAny<int>())).ReturnsAsync(createdProject);
        
        var result = await _projectService.CreateProjectAsync(request);

        // Assert
        _mockProjectRepo.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Once);
        Assert.Equal("New Project", result.Name);
    }
}
