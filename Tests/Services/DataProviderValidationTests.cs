using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgentSquad.Runner.Tests.Services;

public class DataProviderValidationTests
{
    private readonly Mock<IDataCache> _mockCache;
    private readonly Mock<ILogger<DataProvider>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly DataProvider _dataProvider;

    public DataProviderValidationTests()
    {
        _mockCache = new Mock<IDataCache>();
        _mockLogger = new Mock<ILogger<DataProvider>>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(e => e.WebRootPath).Returns("/wwwroot");
        
        _dataProvider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNullProject_ThrowsInvalidOperation()
    {
        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null)
            .Verifiable();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.NotNull(exception);
        Assert.Contains("null", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithEmptyProjectName_ThrowsValidationError()
    {
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

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("Project name", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNullMilestones_ThrowsValidationError()
    {
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = null,
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("milestones collection is null", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithEmptyMilestones_ThrowsValidationError()
    {
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

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("at least one milestone", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNullMilestoneInCollection_ThrowsValidationError()
    {
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone> { null },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("Milestone at index 0 is null", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithEmptyMilestoneName_ThrowsValidationError()
    {
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "", Status = MilestoneStatus.InProgress, TargetDate = DateTime.Now }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("empty or null name", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithInvalidMilestoneStatus_ThrowsValidationError()
    {
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

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("invalid status", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNullWorkItems_ThrowsValidationError()
    {
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", Status = MilestoneStatus.InProgress, TargetDate = DateTime.Now }
            },
            WorkItems = null
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("work items collection is null", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNullWorkItemInCollection_ThrowsValidationError()
    {
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", Status = MilestoneStatus.InProgress, TargetDate = DateTime.Now }
            },
            WorkItems = new List<WorkItem> { null }
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("Work item at index 0 is null", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithEmptyWorkItemTitle_ThrowsValidationError()
    {
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", Status = MilestoneStatus.InProgress, TargetDate = DateTime.Now }
            },
            WorkItems = new List<WorkItem>
            {
                new WorkItem { Title = "", Status = WorkItemStatus.Shipped }
            }
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("empty or null title", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithInvalidWorkItemStatus_ThrowsValidationError()
    {
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", Status = MilestoneStatus.InProgress, TargetDate = DateTime.Now }
            },
            WorkItems = new List<WorkItem>
            {
                new WorkItem { Title = "Item", Status = (WorkItemStatus)999 }
            }
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("invalid status", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithCompletionPercentageNegative_ThrowsValidationError()
    {
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = -5,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", Status = MilestoneStatus.InProgress, TargetDate = DateTime.Now }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("completion percentage", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("0", exception.Message);
        Assert.Contains("100", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithCompletionPercentageOverHundred_ThrowsValidationError()
    {
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 150,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", Status = MilestoneStatus.InProgress, TargetDate = DateTime.Now }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("completion percentage", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("0", exception.Message);
        Assert.Contains("100", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithInvalidHealthStatus_ThrowsValidationError()
    {
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 45,
            HealthStatus = (HealthStatus)999,
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", Status = MilestoneStatus.InProgress, TargetDate = DateTime.Now }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("invalid health status", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNegativeVelocity_ThrowsValidationError()
    {
        var invalidProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = -5,
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", Status = MilestoneStatus.InProgress, TargetDate = DateTime.Now }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(invalidProject);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        Assert.Contains("velocity", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("non-negative", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidData_SucceedsValidation()
    {
        var validProject = new Project
        {
            Name = "Valid Project",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 10,
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
                new WorkItem { Title = "Item1", Status = WorkItemStatus.Shipped }
            }
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(validProject);

        var result = await _dataProvider.LoadProjectDataAsync();
        Assert.NotNull(result);
        Assert.Equal("Valid Project", result.Name);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithCompletionPercentageBoundary_Zero_Succeeds()
    {
        var validProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 0,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", Status = MilestoneStatus.Future, TargetDate = DateTime.Now }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(validProject);

        var result = await _dataProvider.LoadProjectDataAsync();
        Assert.NotNull(result);
        Assert.Equal(0, result.CompletionPercentage);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithCompletionPercentageBoundary_Hundred_Succeeds()
    {
        var validProject = new Project
        {
            Name = "Project",
            CompletionPercentage = 100,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", Status = MilestoneStatus.Completed, TargetDate = DateTime.Now }
            },
            WorkItems = new List<WorkItem>()
        };

        _mockCache
            .Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(validProject);

        var result = await _dataProvider.LoadProjectDataAsync();
        Assert.NotNull(result);
        Assert.Equal(100, result.CompletionPercentage);
    }
}