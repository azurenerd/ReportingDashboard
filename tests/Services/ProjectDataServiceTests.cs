using AgentSquad.Runner.Data;
using AgentSquad.Runner.Services;
using System.Text.Json;
using Xunit;

namespace AgentSquad.Runner.Tests.Services;

public class ProjectDataServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _testDataPath;
    private readonly MockWebHostEnvironment _mockEnvironment;
    private readonly ProjectDataService _service;

    public ProjectDataServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _testDataPath = Path.Combine(_tempDirectory, "data.json");
        _mockEnvironment = new MockWebHostEnvironment { WebRootPath = _tempDirectory };
        _service = new ProjectDataService(_mockEnvironment);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, true);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidJson_ReturnsProjectData()
    {
        // Arrange
        var validData = new
        {
            project = new { name = "Q2 Mobile App", description = "Release", startDate = "2024-01-01", endDate = "2024-06-30" },
            milestones = new[] { new { id = "m1", name = "Design Complete", targetDate = "2024-02-01", status = 1, completionPercentage = 100 } },
            tasks = new[] { new { id = "t1", name = "Setup UI", owner = "Team A", status = 0, dueDate = "2024-02-15" } },
            summary = new { completionPercentage = 25, tasksShipped = 1, tasksInProgress = 2, tasksCarriedOver = 1 }
        };
        await File.WriteAllTextAsync(_testDataPath, JsonSerializer.Serialize(validData));

        // Act
        var result = await _service.LoadProjectDataAsync(_testDataPath);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Project);
        Assert.Equal("Q2 Mobile App", result.Project.Name);
        Assert.Single(result.Milestones);
        Assert.Single(result.Tasks);
        Assert.NotNull(result.Summary);
        Assert.Equal(25, result.Summary.CompletionPercentage);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNonexistentFile_ThrowsDataLoadException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<DataLoadException>(
            () => _service.LoadProjectDataAsync("/nonexistent/path/data.json"));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithEmptyFile_ThrowsDataLoadException()
    {
        // Arrange
        await File.WriteAllTextAsync(_testDataPath, string.Empty);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DataLoadException>(
            () => _service.LoadProjectDataAsync(_testDataPath));
        Assert.Contains("empty", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithInvalidJson_ThrowsDataLoadException()
    {
        // Arrange
        await File.WriteAllTextAsync(_testDataPath, "{ invalid json }");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DataLoadException>(
            () => _service.LoadProjectDataAsync(_testDataPath));
        Assert.Contains("Invalid JSON", exception.Message);
    }

    [Fact]
    public async Task ValidateJsonSchema_WithMissingProjectName_ThrowsDataLoadException()
    {
        // Arrange
        var invalidData = JsonSerializer.Serialize(new
        {
            project = new { name = "", description = "Release" },
            milestones = new object[] { },
            tasks = new object[] { },
            summary = new { }
        });

        // Act & Assert
        var exception = Assert.Throws<DataLoadException>(
            () => _service.ValidateJsonSchema(invalidData));
        Assert.Contains("Project name is required", exception.Message);
    }

    [Fact]
    public async Task ValidateJsonSchema_WithMissingMilestoneName_ThrowsDataLoadException()
    {
        // Arrange
        var invalidData = JsonSerializer.Serialize(new
        {
            project = new { name = "Test Project", description = "Test" },
            milestones = new[] { new { id = "m1", name = "", targetDate = "2024-02-01", status = 0, completionPercentage = 0 } },
            tasks = new object[] { },
            summary = new { }
        });

        // Act & Assert
        var exception = Assert.Throws<DataLoadException>(
            () => _service.ValidateJsonSchema(invalidData));
        Assert.Contains("Milestone name is required", exception.Message);
    }

    [Fact]
    public async Task ValidateJsonSchema_WithMissingTaskOwner_ThrowsDataLoadException()
    {
        // Arrange
        var invalidData = JsonSerializer.Serialize(new
        {
            project = new { name = "Test Project", description = "Test" },
            milestones = new object[] { },
            tasks = new[] { new { id = "t1", name = "Task 1", owner = "", status = 0, dueDate = "2024-02-01" } },
            summary = new { }
        });

        // Act & Assert
        var exception = Assert.Throws<DataLoadException>(
            () => _service.ValidateJsonSchema(invalidData));
        Assert.Contains("Task owner is required", exception.Message);
    }

    [Fact]
    public async Task GetCachedData_WhenNotCached_ReturnsNull()
    {
        // Act
        var result = _service.GetCachedData();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCachedData_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var validData = new
        {
            project = new { name = "Test", description = "Test", startDate = "2024-01-01", endDate = "2024-06-30" },
            milestones = new object[] { },
            tasks = new object[] { },
            summary = new { }
        };
        await File.WriteAllTextAsync(_testDataPath, JsonSerializer.Serialize(validData));
        await _service.LoadProjectDataAsync(_testDataPath);

        // Act
        var result = _service.GetCachedData();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Project);
    }

    [Fact]
    public async Task RefreshData_ClearsCacheAndReloads()
    {
        // Arrange
        var validData = new
        {
            project = new { name = "Original", description = "Test", startDate = "2024-01-01", endDate = "2024-06-30" },
            milestones = new object[] { },
            tasks = new object[] { },
            summary = new { }
        };
        await File.WriteAllTextAsync(_testDataPath, JsonSerializer.Serialize(validData));
        var firstLoad = await _service.LoadProjectDataAsync(_testDataPath);

        // Modify file
        var updatedData = new
        {
            project = new { name = "Updated", description = "Test", startDate = "2024-01-01", endDate = "2024-06-30" },
            milestones = new object[] { },
            tasks = new object[] { },
            summary = new { }
        };
        await File.WriteAllTextAsync(_testDataPath, JsonSerializer.Serialize(updatedData));

        // Act
        var reloaded = await _service.RefreshData(_testDataPath);

        // Assert
        Assert.Equal("Updated", reloaded.Project!.Name);
    }

    private class MockWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Test";
    }
}