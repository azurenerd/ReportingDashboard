using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using AgentSquad.Dashboard.Models;
using AgentSquad.Dashboard.Services;

namespace AgentSquad.Dashboard.Tests.Services;

public class ProjectDataServiceTests : IDisposable
{
    private readonly ProjectDataService _service;
    private readonly Mock<ILogger<ProjectDataService>> _mockLogger;
    private readonly string _testDataDir;

    public ProjectDataServiceTests()
    {
        _mockLogger = new Mock<ILogger<ProjectDataService>>();
        _service = new ProjectDataService(_mockLogger.Object);
        _testDataDir = Path.Combine(AppContext.BaseDirectory, "wwwroot", "data");
        Directory.CreateDirectory(_testDataDir);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidJson_ShouldReturnProjectData()
    {
        var json = JsonSerializer.Serialize(new
        {
            project = new { name = "Test", description = "Test", startDate = DateTime.Now, endDate = DateTime.Now.AddDays(30), status = "Active", sponsor = "Sponsor", projectManager = "PM" },
            milestones = new[] { new { id = "M1", name = "M1", targetDate = DateTime.Now.AddDays(15), status = "Completed", completionPercentage = 100 } },
            tasks = new[] { new { id = "T1", name = "T1", status = "Shipped", assignedTo = "User", dueDate = DateTime.Now, estimatedDays = 5, relatedMilestone = "M1" } },
            metrics = new { totalTasks = 1, completedTasks = 1, inProgressTasks = 0, carriedOverTasks = 0, completionPercentage = 100, estimatedBurndownRate = 0.1, projectStartDate = DateTime.Now, projectEndDate = DateTime.Now.AddDays(30), daysRemaining = 30 }
        });

        var testFile = Path.Combine(_testDataDir, "valid_data.json");
        await File.WriteAllTextAsync(testFile, json);

        var result = await _service.LoadProjectDataAsync("data/valid_data.json");

        Assert.NotNull(result);
        Assert.NotNull(result.Project);
        Assert.NotNull(result.Milestones);
        Assert.NotNull(result.Tasks);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingFile_ShouldThrowFileNotFoundException()
    {
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(
            () => _service.LoadProjectDataAsync("data/nonexistent.json"));

        Assert.Contains("data.json not found", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithInvalidJson_ShouldThrowInvalidOperationException()
    {
        var invalidJson = "{ invalid json content }";
        var testFile = Path.Combine(_testDataDir, "invalid.json");
        await File.WriteAllTextAsync(testFile, invalidJson);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.LoadProjectDataAsync("data/invalid.json"));

        Assert.Contains("Invalid JSON format", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingRequiredFields_ShouldThrowInvalidOperationException()
    {
        var jsonMissingProject = JsonSerializer.Serialize(new
        {
            milestones = new[] { },
            tasks = new[] { }
        });

        var testFile = Path.Combine(_testDataDir, "missing_fields.json");
        await File.WriteAllTextAsync(testFile, jsonMissingProject);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.LoadProjectDataAsync("data/missing_fields.json"));

        Assert.Contains("Missing required fields", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_ShouldCacheData()
    {
        var json = JsonSerializer.Serialize(new
        {
            project = new { name = "Test", description = "Test", startDate = DateTime.Now, endDate = DateTime.Now.AddDays(30), status = "Active", sponsor = "Sponsor", projectManager = "PM" },
            milestones = new[] { },
            tasks = new[] { },
            metrics = new { totalTasks = 0, completedTasks = 0, inProgressTasks = 0, carriedOverTasks = 0, completionPercentage = 0, estimatedBurndownRate = 0.0, projectStartDate = DateTime.Now, projectEndDate = DateTime.Now.AddDays(30), daysRemaining = 30 }
        });

        var testFile = Path.Combine(_testDataDir, "cache_test.json");
        await File.WriteAllTextAsync(testFile, json);

        var data1 = await _service.LoadProjectDataAsync("data/cache_test.json");
        var cachedData = _service.GetCachedData();

        Assert.NotNull(cachedData);
        Assert.Equal(data1.Project.Name, cachedData.Project.Name);
    }

    [Fact]
    public void GetCachedData_WithoutLoadingFirst_ShouldReturnNull()
    {
        var freshService = new ProjectDataService(_mockLogger.Object);
        var cached = freshService.GetCachedData();

        Assert.Null(cached);
    }

    [Fact]
    public void RefreshData_ShouldClearCache()
    {
        var projectData = new ProjectData
        {
            Project = new ProjectInfo { Name = "Test" },
            Milestones = new(),
            Tasks = new()
        };

        _service.RefreshData();
        var cached = _service.GetCachedData();

        Assert.Null(cached);
    }

    [Fact]
    public void ValidateJsonSchema_WithValidJson_ShouldReturnTrue()
    {
        var validJson = JsonSerializer.Serialize(new
        {
            project = new { name = "Test", description = "Test", startDate = DateTime.Now, endDate = DateTime.Now.AddDays(30), status = "Active", sponsor = "Sponsor", projectManager = "PM" },
            milestones = new[] { },
            tasks = new[] { }
        });

        var isValid = _service.ValidateJsonSchema(validJson);

        Assert.True(isValid);
    }

    [Fact]
    public void ValidateJsonSchema_WithInvalidJson_ShouldReturnFalse()
    {
        var invalidJson = "{ not valid json }";
        var isValid = _service.ValidateJsonSchema(invalidJson);

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateJsonSchema_WithMissingFields_ShouldReturnFalse()
    {
        var jsonMissingProject = JsonSerializer.Serialize(new
        {
            milestones = new[] { },
            tasks = new[] { }
        });

        var isValid = _service.ValidateJsonSchema(jsonMissingProject);

        Assert.False(isValid);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataDir))
        {
            Directory.Delete(_testDataDir, true);
        }
    }
}