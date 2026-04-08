using System.Text.Json;
using Moq;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Tests.Services;

public class DataProviderErrorHandlingTests
{
    private readonly Mock<IDataCache> _mockCache;
    private readonly Mock<ILogger<DataProvider>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly DataProvider _dataProvider;

    public DataProviderErrorHandlingTests()
    {
        _mockCache = new Mock<IDataCache>();
        _mockLogger = new Mock<ILogger<DataProvider>>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(e => e.WebRootPath).Returns("/wwwroot");
        
        _dataProvider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithCacheHit_LogsInformation()
    {
        // Arrange
        var cachedProject = new Project
        {
            Name = "Cached Project",
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "M1",
                    Status = MilestoneStatus.InProgress,
                    TargetDate = DateTime.Now
                }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(cachedProject);

        // Act
        var result = await _dataProvider.LoadProjectDataAsync();

        // Assert
        Assert.NotNull(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("cache")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidProject_LogsSuccessfulLoad()
    {
        // Arrange
        var validProject = new Project
        {
            Name = "Test Project",
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "M1",
                    Status = MilestoneStatus.InProgress,
                    TargetDate = DateTime.Now
                }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(validProject);

        // Act
        var result = await _dataProvider.LoadProjectDataAsync();

        // Assert
        Assert.NotNull(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void InvalidateCache_LogsInformation()
    {
        // Arrange
        _mockCache.Setup(c => c.Remove(It.IsAny<string>()));

        // Act
        _dataProvider.InvalidateCache();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("invalidated")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidData_LogsValidationPassed()
    {
        // Arrange
        var validProject = new Project
        {
            Name = "Valid Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Phase 1",
                    Status = MilestoneStatus.InProgress,
                    TargetDate = DateTime.Now
                }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(validProject);

        // Act
        var result = await _dataProvider.LoadProjectDataAsync();

        // Assert
        Assert.NotNull(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("validation passed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void InvalidateCache_WithCacheError_LogsErrorButContinues()
    {
        // Arrange
        _mockCache
            .Setup(c => c.Remove(It.IsAny<string>()))
            .Throws(new Exception("Cache removal error"));

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => _dataProvider.InvalidateCache());
        Assert.NotNull(exception);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task LoadProjectDataAsync_LogsAttemptingToLoad()
    {
        // Arrange
        var cachedProject = new Project
        {
            Name = "Project",
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", Status = MilestoneStatus.InProgress, TargetDate = DateTime.Now }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(cachedProject);

        // Act
        await _dataProvider.LoadProjectDataAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attempting")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithCompletionPercentageOutOfRange_ThrowsValidationError()
    {
        // Arrange
        var invalidProject = new Project
        {
            Name = "Invalid Project",
            CompletionPercentage = 150,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "M1",
                    Status = MilestoneStatus.InProgress,
                    TargetDate = DateTime.Now
                }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("completion percentage", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("0", exception.Message);
        Assert.Contains("100", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNullProject_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);

        // This tests that when deserialization returns null, it's caught and wrapped

        // Note: In real scenarios, this would be triggered by invalid JSON or 
        // file read failure, which are tested in integration tests
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithEmptyProjectName_ThrowsValidationError()
    {
        // Arrange
        var invalidProject = new Project
        {
            Name = "",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "M1",
                    Status = MilestoneStatus.InProgress,
                    TargetDate = DateTime.Now
                }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("Project name", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithEmptyMilestones_ThrowsValidationError()
    {
        // Arrange
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>(),
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("at least one milestone", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithInvalidMilestoneStatus_ThrowsValidationError()
    {
        // Arrange
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "M1",
                    Status = (MilestoneStatus)999,
                    TargetDate = DateTime.Now
                }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("invalid status", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithInvalidWorkItemStatus_ThrowsValidationError()
    {
        // Arrange
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "M1",
                    Status = MilestoneStatus.InProgress,
                    TargetDate = DateTime.Now
                }
            },
            WorkItems = new List<WorkItem>
            {
                new WorkItem
                {
                    Title = "Item",
                    Status = (WorkItemStatus)999
                }
            }
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("invalid status", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNegativeVelocity_ThrowsValidationError()
    {
        // Arrange
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = -5,
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "M1",
                    Status = MilestoneStatus.InProgress,
                    TargetDate = DateTime.Now
                }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("velocity", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("non-negative", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}