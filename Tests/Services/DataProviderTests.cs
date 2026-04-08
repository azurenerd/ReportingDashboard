using System.Text.Json;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Services;

public class DataProviderTests
{
    private readonly Mock<IDataCache> _mockCache;
    private readonly Mock<ILogger<DataProvider>> _mockLogger;
    private readonly DataProvider _dataProvider;

    public DataProviderTests()
    {
        _mockCache = new Mock<IDataCache>();
        _mockLogger = new Mock<ILogger<DataProvider>>();
        _dataProvider = new DataProvider(_mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WhenCacheHit_ReturnsCachedProject()
    {
        var cachedProject = new Project
        {
            Name = "Cached Project",
            Description = "From cache",
            Milestones = new List<Milestone> { new Milestone { Name = "M1" } }
        };

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(cachedProject);

        var result = await _dataProvider.LoadProjectDataAsync();

        Assert.NotNull(result);
        Assert.Equal("Cached Project", result.Name);
        Assert.Equal("From cache", result.Description);
        _mockCache.Verify(c => c.GetAsync<Project>(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WhenCacheMiss_ReadsAndCachesProject()
    {
        var testData = new Project
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = new DateTime(2024, 1, 1),
            TargetEndDate = new DateTime(2024, 12, 31),
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 10,
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Phase 1",
                    TargetDate = new DateTime(2024, 3, 31),
                    Status = MilestoneStatus.Completed,
                    Description = "Phase 1 Launch"
                }
            },
            WorkItems = new List<WorkItem>
            {
                new WorkItem
                {
                    Title = "Feature 1",
                    Description = "Implement feature 1",
                    Status = WorkItemStatus.Shipped,
                    AssignedTo = "Team A"
                }
            }
        };

        var json = JsonSerializer.Serialize(testData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        var tempDir = Path.Combine(Path.GetTempPath(), "dataprovider_test");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        var dataFilePath = Path.Combine(wwwrootDir, "data.json");

        try
        {
            await File.WriteAllTextAsync(dataFilePath, json);

            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                var result = await _dataProvider.LoadProjectDataAsync();

                Assert.NotNull(result);
                Assert.Equal("Test Project", result.Name);
                Assert.Equal("Test Description", result.Description);
                Assert.Equal(50, result.CompletionPercentage);
                Assert.Equal(HealthStatus.OnTrack, result.HealthStatus);
                Assert.Single(result.Milestones);
                Assert.Single(result.WorkItems);

                _mockCache.Verify(c => c.GetAsync<Project>(It.IsAny<string>()), Times.Once);
                _mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan?>()), Times.Once);
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDir);
            }
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WhenFileNotFound_ThrowsInvalidOperationException()
    {
        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        var tempDir = Path.Combine(Path.GetTempPath(), "dataprovider_test_notfound");
        Directory.CreateDirectory(tempDir);

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(tempDir);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WhenJsonIsInvalid_ThrowsInvalidOperationException()
    {
        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        var tempDir = Path.Combine(Path.GetTempPath(), "dataprovider_test_invalid");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        var dataFilePath = Path.Combine(wwwrootDir, "data.json");

        try
        {
            await File.WriteAllTextAsync(dataFilePath, "{ invalid json }");

            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDir);
            }
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void InvalidateCache_CallsRemoveOnCache()
    {
        _dataProvider.InvalidateCache();

        _mockCache.Verify(c => c.Remove(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMultipleMilestones_DeserializesCorrectly()
    {
        var cachedProject = new Project
        {
            Name = "Multi-Milestone Project",
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", Status = MilestoneStatus.Completed },
                new Milestone { Name = "M2", Status = MilestoneStatus.InProgress },
                new Milestone { Name = "M3", Status = MilestoneStatus.Future }
            }
        };

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(cachedProject);

        var result = await _dataProvider.LoadProjectDataAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.Milestones.Count);
        Assert.Equal("M1", result.Milestones[0].Name);
        Assert.Equal("M2", result.Milestones[1].Name);
        Assert.Equal("M3", result.Milestones[2].Name);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMultipleWorkItems_DeserializesCorrectly()
    {
        var cachedProject = new Project
        {
            Name = "Multi-WorkItem Project",
            Milestones = new List<Milestone> { new Milestone { Name = "M1" } },
            WorkItems = new List<WorkItem>
            {
                new WorkItem { Title = "WI1", Status = WorkItemStatus.Shipped },
                new WorkItem { Title = "WI2", Status = WorkItemStatus.InProgress },
                new WorkItem { Title = "WI3", Status = WorkItemStatus.CarriedOver }
            }
        };

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(cachedProject);

        var result = await _dataProvider.LoadProjectDataAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.WorkItems.Count);
        Assert.Equal("WI1", result.WorkItems[0].Title);
        Assert.Equal("WI2", result.WorkItems[1].Title);
        Assert.Equal("WI3", result.WorkItems[2].Title);
    }

    [Fact]
    public async Task LoadProjectDataAsync_CacheHit_DoesNotCallSetAsync()
    {
        var cachedProject = new Project
        {
            Name = "Cached Project",
            Milestones = new List<Milestone> { new Milestone { Name = "M1" } }
        };

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(cachedProject);

        await _dataProvider.LoadProjectDataAsync();

        _mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan?>()), Times.Never);
    }

    [Fact]
    public void Constructor_WithNullCache_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DataProvider(null, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DataProvider(_mockCache.Object, null));
    }
}