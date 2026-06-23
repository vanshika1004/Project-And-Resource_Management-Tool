using Moq;
using PRM.Application.Services;
using PRM.Core.Entities;
using PRM.Core.Interfaces;
using PRM.Application.Exceptions;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace PRM.Tests.Services;

public class AiServiceTests
{
    private readonly Mock<ILlmProvider> _mockLlmProvider;
    private readonly Mock<IProjectRepository> _mockProjectRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IAllocationRepository> _mockAllocationRepo;
    private readonly Mock<ITimesheetRepository> _mockTimesheetRepo;
    private readonly Mock<ISystemConfigRepository> _mockConfigRepo;
    private readonly AiService _aiService;

    public AiServiceTests()
    {
        _mockLlmProvider = new Mock<ILlmProvider>();
        _mockProjectRepo = new Mock<IProjectRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockAllocationRepo = new Mock<IAllocationRepository>();
        _mockTimesheetRepo = new Mock<ITimesheetRepository>();
        _mockConfigRepo = new Mock<ISystemConfigRepository>();

        _aiService = new AiService(
            _mockLlmProvider.Object,
            _mockProjectRepo.Object,
            _mockUserRepo.Object,
            _mockAllocationRepo.Object,
            _mockTimesheetRepo.Object,
            _mockConfigRepo.Object
        );
    }

    [Fact]
    public async Task GetSkillMatchRecommendationAsync_CallsLlmProvider()
    {
        // Arrange
        var users = new List<User> { new User { Id = 1, Username = "test", Email = "test@example.com", PasswordHash = "", FullName = "Test User", Role = new Role { Name = "Resource" } } };
        _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
        _mockAllocationRepo.Setup(r => r.GetActiveByResourceIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<Allocation>());
        _mockTimesheetRepo.Setup(r => r.GetByResourceIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Timesheet>());
        _mockConfigRepo.Setup(r => r.GetValueAsync(It.IsAny<string>())).ReturnsAsync("40");
        _mockLlmProvider.Setup(p => p.GetSkillMatchAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync("Mocked Match Result");

        // Act
        var result = await _aiService.GetSkillMatchRecommendationAsync("C# developer", 1);

        // Assert
        Assert.Equal("Mocked Match Result", result);
        _mockLlmProvider.Verify(p => p.GetSkillMatchAsync("C# developer", It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectRiskSummaryAsync_ThrowsNotFound_WhenProjectDoesNotExist()
    {
        // Arrange
        _mockProjectRepo.Setup(r => r.GetByIdWithMilestonesAsync(It.IsAny<int>())).ReturnsAsync((Project)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _aiService.GetProjectRiskSummaryAsync(1, 1));
    }

    [Fact]
    public async Task GetProjectRiskSummaryAsync_ThrowsBusinessRuleViolation_WhenNotManager()
    {
        // Arrange
        var project = new Project { Id = 1, Name = "Test", ManagerId = 2 };
        _mockProjectRepo.Setup(r => r.GetByIdWithMilestonesAsync(1)).ReturnsAsync(project);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => _aiService.GetProjectRiskSummaryAsync(1, 1));
    }

    [Fact]
    public async Task GetProjectRiskSummaryAsync_CallsLlmProvider()
    {
        // Arrange
        var project = new Project { Id = 1, Name = "Test", ManagerId = 1 };
        _mockProjectRepo.Setup(r => r.GetByIdWithMilestonesAsync(1)).ReturnsAsync(project);
        _mockLlmProvider.Setup(p => p.GetRiskSummaryAsync(It.IsAny<object>()))
            .ReturnsAsync("Mocked Risk Summary");

        // Act
        var result = await _aiService.GetProjectRiskSummaryAsync(1, 1);

        // Assert
        Assert.Equal("Mocked Risk Summary", result);
        _mockLlmProvider.Verify(p => p.GetRiskSummaryAsync(It.IsAny<object>()), Times.Once);
    }
}
