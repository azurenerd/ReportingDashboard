using System.Text.Json;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Services;

public class DataProviderValidationTests
{
    private readonly Mock<IDataCache> _mockCache;
    private readonly Mock<ILogger<DataProvider>> _mockLogger;
    private readonly DataProvider _dataProvider;

    public DataProviderValidationTests()
    {
        _mockCache = new Mock<IDataCache>();
        _mockLogger = new Mock<ILogger<DataProvider>>();
        _dataProvider = new DataProvider(_mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WhenProjectNameIsNull_ThrowsInvalidOperationException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "validation_test_null_name");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        var dataFilePath = Path.Combine(wwwrootDir, "data.json");

        try
        {
            var projectData = new { name = (string)null, milestones = new[] { new { name = "M1" } } };
            var json = JsonSerializer.Serialize(projectData);
            await File.WriteAllTextAsync(dataFilePath, json);

            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
                .ReturnsAsync((Project)null);

            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
                Assert.Contains("Project name is required", ex.Message);
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
    public async Task LoadProjectDataAsync_WhenProjectNameIsEmpty_ThrowsInvalidOperationException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "validation_test_empty_name");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        var dataFilePath = Path.Combine(wwwrootDir, "data.json");

        try
        {
            var projectData = new { name = "", milestones = new[] { new { name = "M1" } } };
            var json = JsonSerializer.Serialize(projectData);
            await File.WriteAllTextAsync(dataFilePath, json);

            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
                .ReturnsAsync((Project)null);

            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
                Assert.Contains("Project name is required", ex.Message);
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
    public async Task LoadProjectDataAsync_WhenMilestonesIsEmpty_ThrowsInvalidOperationException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "validation_test_empty_milestones");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        var dataFilePath = Path.Combine(wwwrootDir, "data.json");

        try
        {
            var projectData = new { name = "Test Project", milestones = new object[] { } };
            var json = JsonSerializer.Serialize(projectData);
            await File.WriteAllTextAsync(dataFilePath, json);

            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
                .ReturnsAsync((Project)null);

            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
                Assert.Contains("At least one milestone is required", ex.Message);
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
    public async Task LoadProjectDataAsync_WhenCompletionPercentageBelowZero_ThrowsInvalidOperationException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "validation_test_negative_percentage");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        var dataFilePath = Path.Combine(wwwrootDir, "data.json");

        try
        {
            var projectData = new
            {
                name = "Test Project",
                completionPercentage = -5,
                milestones = new[] { new { name = "M1", status = "Completed" } }
            };
            var json = JsonSerializer.Serialize(projectData);
            await File.WriteAllTextAsync(dataFilePath, json);

            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
                .ReturnsAsync((Project)null);

            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
                Assert.Contains("CompletionPercentage must be between 0 and 100", ex.Message);
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
    public async Task LoadProjectDataAsync_WhenCompletionPercentageAbove100_ThrowsInvalidOperationException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "validation_test_over_100_percentage");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        var dataFilePath = Path.Combine(wwwrootDir, "data.json");

        try
        {
            var projectData = new
            {
                name = "Test Project",
                completionPercentage = 150,
                milestones = new[] { new { name = "M1", status = "Completed" } }
            };
            var json = JsonSerializer.Serialize(projectData);
            await File.WriteAllTextAsync(dataFilePath, json);

            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
                .ReturnsAsync((Project)null);

            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
                Assert.Contains("CompletionPercentage must be between 0 and 100", ex.Message);
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
    public async Task LoadProjectDataAsync_WhenInvalidHealthStatus_ThrowsInvalidOperationException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "validation_test_invalid_health_status");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        var dataFilePath = Path.Combine(wwwrootDir, "data.json");

        try
        {
            var projectData = new
            {
                name = "Test Project",
                healthStatus = "InvalidStatus",
                milestones = new[] { new { name = "M1", status = "Completed" } }
            };
            var json = JsonSerializer.Serialize(projectData);
            await File.WriteAllTextAsync(dataFilePath, json);

            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
                .ReturnsAsync((Project)null);

            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
                Assert.Contains("Invalid HealthStatus", ex.Message);
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
    public async Task LoadProjectDataAsync_WhenMilestoneHasEmptyName_ThrowsInvalidOperationException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "validation_test_empty_milestone_name");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        var dataFilePath = Path.Combine(wwwrootDir, "data.json");

        try
        {
            var projectData = new
            {
                name = "Test Project",
                milestones = new[] { new { name = "", status = "Completed" } }
            };
            var json = JsonSerializer.Serialize(projectData);
            await File.WriteAllTextAsync(dataFilePath, json);

            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
                .ReturnsAsync((Project)null);

            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
                Assert.Contains("empty name", ex.Message);
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
    public async Task LoadProjectDataAsync_WhenMilestoneHasInvalidStatus_ThrowsInvalidOperationException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "validation_test_invalid_milestone_status");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        var dataFilePath = Path.Combine(wwwrootDir, "data.json");

        try
        {
            var projectData = new
            {
                name = "Test Project",
                milestones = new[] { new { name = "M1", status = "InvalidMilestoneStatus" } }
            };
            var json = JsonSerializer.Serialize(projectData);
            await File.WriteAllTextAsync(dataFilePath, json);

            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
                .ReturnsAsync((Project)null);

            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
                Assert.Contains("invalid status", ex.Message);
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
    public async Task LoadProjectDataAsync_WhenWorkItemHasEmptyTitle_ThrowsInvalidOperationException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "validation_test_empty_workitem_title");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        var dataFilePath = Path.Combine(wwwrootDir, "data.json");

        try
        {
            var projectData = new
            {
                name = "Test Project",
                milestones = new[] { new { name = "M1", status = "Completed" } },
                workItems = new[] { new { title = "", status = "Shipped" } }
            };
            var json = JsonSerializer.Serialize(projectData);
            await File.WriteAllTextAsync(dataFilePath, json);

            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
                .ReturnsAsync((Project)null);

            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
                Assert.Contains("empty title", ex.Message);
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
    public async Task LoadProjectDataAsync_WhenWorkItemHasInvalidStatus_ThrowsInvalidOperationException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "validation_test_invalid_workitem_status");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        var dataFilePath = Path.Combine(wwwrootDir, "data.json");

        try
        {
            var projectData = new
            {
                name = "Test Project",
                milestones = new[] { new { name = "M1", status = "Completed" } },
                workItems = new[] { new { title = "WI1", status = "InvalidWorkItemStatus" } }
            };
            var json = JsonSerializer.Serialize(projectData);
            await File.WriteAllTextAsync(dataFilePath, json);

            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
                .ReturnsAsync((Project)null);

            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
                Assert.Contains("invalid status", ex.Message);
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
    public async Task LoadProjectDataAsync_WhenValidDataProvided_SkipsValidationAndCaches()
    {
        var validProject = new Project
        {
            Name = "Valid Project",
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.OnTrack,
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Phase 1",
                    Status = MilestoneStatus.Completed,
                    TargetDate = new DateTime(2024, 3, 31)
                }
            },
            WorkItems = new List<WorkItem>
            {
                new WorkItem
                {
                    Title = "Feature 1",
                    Status = WorkItemStatus.Shipped
                }
            }
        };

        var json = JsonSerializer.Serialize(validProject, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        var tempDir = Path.Combine(Path.GetTempPath(), "validation_test_valid_data");
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
                Assert.Equal("Valid Project", result.Name);
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
    public async Task LoadProjectDataAsync_WhenCompletionPercentageIs0_ValidatesSuccessfully()
    {
        var projectData = new
        {
            name = "Test Project",
            completionPercentage = 0,
            milestones = new[] { new { name = "M1", status = "Completed" } }
        };
        var json = JsonSerializer.Serialize(projectData);

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        var tempDir = Path.Combine(Path.GetTempPath(), "validation_test_zero_percentage");
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
    public async Task LoadProjectDataAsync_WhenCompletionPercentageIs100_ValidatesSuccessfully()
    {
        var projectData = new
        {
            name = "Test Project",
            completionPercentage = 100,
            milestones = new[] { new { name = "M1", status = "Completed" } }
        };
        var json = JsonSerializer.Serialize(projectData);

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        var tempDir = Path.Combine(Path.GetTempPath(), "validation_test_100_percentage");
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
}