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
    private const string TestDataPath = "wwwroot/data.json";

    public DataProviderTests()
    {
        _mockCache = new Mock<IDataCache>();
        _mockLogger = new Mock<ILogger<DataProvider>>();
        _dataProvider = new DataProvider(_mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidJsonFile_ReturnsProjectObject()
    {
        // Arrange
        var testJson = """
        {
            "name": "Test Project",
            "description": "A test project",
            "startDate": "2024-01-01T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 50,
            "healthStatus": "OnTrack",
            "velocityThisMonth": 5,
            "milestones": [
                {
                    "name": "M1",
                    "targetDate": "2024-06-30T00:00:00Z",
                    "status": "Future"
                }
            ],
            "workItems": []
        }
        """;

        var testFilePath = Path.Combine(Path.GetTempPath(), "test_data.json");
        await File.WriteAllTextAsync(testFilePath, testJson);

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        try
        {
            // Act
            var result = await _dataProvider.LoadProjectDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Project", result.Name);
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithCachedData_ReturnsCachedValueWithoutReading()
    {
        // Arrange
        var cachedProject = new Project { Name = "Cached Project", Milestones = new List<Milestone> { new() { Name = "M1" } } };

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(cachedProject);

        // Act
        var result = await _dataProvider.LoadProjectDataAsync();

        // Assert
        Assert.Equal(cachedProject, result);
        _mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonexistentPath = Path.Combine(Path.GetTempPath(), "nonexistent_" + Guid.NewGuid() + ".json");

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _dataProvider.LoadProjectDataAsync());
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithInvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var testFilePath = Path.Combine(Path.GetTempPath(), "invalid_data.json");
        await File.WriteAllTextAsync(testFilePath, invalidJson);

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingProjectName_ThrowsInvalidOperationException()
    {
        // Arrange
        var json = """
        {
            "startDate": "2024-01-01T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 50,
            "healthStatus": "OnTrack",
            "velocityThisMonth": 5,
            "milestones": [{"name": "M1", "targetDate": "2024-06-30T00:00:00Z", "status": "Future"}],
            "workItems": []
        }
        """;

        var testFilePath = Path.Combine(Path.GetTempPath(), "missing_name_data.json");
        await File.WriteAllTextAsync(testFilePath, json);

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNoMilestones_ThrowsInvalidOperationException()
    {
        // Arrange
        var json = """
        {
            "name": "No Milestones Project",
            "startDate": "2024-01-01T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 50,
            "healthStatus": "OnTrack",
            "velocityThisMonth": 5,
            "milestones": [],
            "workItems": []
        }
        """;

        var testFilePath = Path.Combine(Path.GetTempPath(), "no_milestones_data.json");
        await File.WriteAllTextAsync(testFilePath, json);

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public void InvalidateCache_RemovesProjectDataFromCache()
    {
        // Act
        _dataProvider.InvalidateCache();

        // Assert
        _mockCache.Verify(c => c.Remove(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithInvalidCompletionPercentage_ThrowsInvalidOperationException()
    {
        // Arrange - completion percentage > 100
        var json = """
        {
            "name": "Invalid Percentage",
            "startDate": "2024-01-01T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 150,
            "healthStatus": "OnTrack",
            "velocityThisMonth": 5,
            "milestones": [{"name": "M1", "targetDate": "2024-06-30T00:00:00Z", "status": "Future"}],
            "workItems": []
        }
        """;

        var testFilePath = Path.Combine(Path.GetTempPath(), "invalid_percentage_data.json");
        await File.WriteAllTextAsync(testFilePath, json);

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dataProvider.LoadProjectDataAsync());
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_CachesResultAfterLoading()
    {
        // Arrange
        var json = """
        {
            "name": "Cache Test Project",
            "startDate": "2024-01-01T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 50,
            "healthStatus": "OnTrack",
            "velocityThisMonth": 5,
            "milestones": [{"name": "M1", "targetDate": "2024-06-30T00:00:00Z", "status": "Future"}],
            "workItems": []
        }
        """;

        var testFilePath = Path.Combine(Path.GetTempPath(), "cache_test_data.json");
        await File.WriteAllTextAsync(testFilePath, json);

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        try
        {
            // Act
            await _dataProvider.LoadProjectDataAsync();

            // Assert
            _mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan>()), Times.Once);
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }
}