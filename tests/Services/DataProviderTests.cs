using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace AgentSquad.Runner.Tests.Services;

public class DataProviderTests : IDisposable
{
    private readonly Mock<IDataCache> _mockCache;
    private readonly Mock<IDataValidator> _mockValidator;
    private readonly Mock<ILogger<DataProvider>> _mockLogger;
    private readonly IDataProvider _dataProvider;
    private readonly string _testDataPath;

    public DataProviderTests()
    {
        _mockCache = new Mock<IDataCache>();
        _mockValidator = new Mock<IDataValidator>();
        _mockLogger = new Mock<ILogger<DataProvider>>();
        _dataProvider = new DataProvider(_mockCache.Object, _mockValidator.Object, _mockLogger.Object);
        _testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "data.json");
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidData_ReturnsProject()
    {
        var validProject = InvalidDataFixtures.ValidProject;
        var validationResult = new ValidationResult { IsValid = true };

        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);
        _mockValidator.Setup(x => x.ValidateProjectData(It.IsAny<Project>()))
            .Returns(validationResult);
        _mockCache.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        EnsureTestDataFile(InvalidDataFixtures.ValidJsonString);

        var result = await _dataProvider.LoadProjectDataAsync();

        Assert.NotNull(result);
        Assert.Equal("Test Project", result.Name);
        _mockCache.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithCachedData_ReturnsCachedProject()
    {
        var cachedProject = InvalidDataFixtures.ValidProject;
        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(cachedProject);

        var result = await _dataProvider.LoadProjectDataAsync();

        Assert.NotNull(result);
        Assert.Equal(cachedProject.Name, result.Name);
        _mockValidator.Verify(x => x.ValidateProjectData(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingFile_ThrowsFileNotFoundException()
    {
        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);

        DeleteTestDataFile();

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _dataProvider.LoadProjectDataAsync());
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithInvalidJson_ThrowsJsonException()
    {
        var invalidJson = InvalidDataFixtures.InvalidJsonString;
        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);

        EnsureTestDataFile(invalidJson);

        await Assert.ThrowsAsync<JsonException>(
            () => _dataProvider.LoadProjectDataAsync());
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidationFailure_ThrowsInvalidOperationException()
    {
        var validProject = InvalidDataFixtures.ValidProject;
        var validationResult = new ValidationResult { IsValid = false };
        validationResult.AddError("NAME_EMPTY", "Project name is required", nameof(Project.Name));

        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);
        _mockValidator.Setup(x => x.ValidateProjectData(It.IsAny<Project>()))
            .Returns(validationResult);

        EnsureTestDataFile(InvalidDataFixtures.ValidJsonString);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _dataProvider.LoadProjectDataAsync());

        Assert.Contains("Configuration validation failed", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNullProject_ThrowsInvalidOperationException()
    {
        var nullProjectJson = @"null";
        var validationResult = new ValidationResult { IsValid = false };
        validationResult.AddError("PROJECT_NULL", "Project data is null");

        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);
        _mockValidator.Setup(x => x.ValidateProjectData(null))
            .Returns(validationResult);

        EnsureTestDataFile(nullProjectJson);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _dataProvider.LoadProjectDataAsync());

        Assert.Contains("validation failed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingMilestones_ThrowsInvalidOperationException()
    {
        var project = InvalidDataFixtures.ProjectWithNullMilestones;
        var validationResult = new ValidationResult { IsValid = false };
        validationResult.AddError("MILESTONES_NULL", "Milestones cannot be null", nameof(Project.Milestones));

        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);
        _mockValidator.Setup(x => x.ValidateProjectData(It.IsAny<Project>()))
            .Returns(validationResult);

        var json = JsonSerializer.Serialize(project);
        EnsureTestDataFile(json);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _dataProvider.LoadProjectDataAsync());

        Assert.Contains("Milestones", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithZeroMilestones_ThrowsInvalidOperationException()
    {
        var project = InvalidDataFixtures.ProjectWithEmptyMilestones;
        var validationResult = new ValidationResult { IsValid = false };
        validationResult.AddError("MILESTONES_EMPTY", "At least one milestone required", nameof(Project.Milestones));

        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);
        _mockValidator.Setup(x => x.ValidateProjectData(It.IsAny<Project>()))
            .Returns(validationResult);

        var json = JsonSerializer.Serialize(project);
        EnsureTestDataFile(json);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _dataProvider.LoadProjectDataAsync());

        Assert.Contains("milestone", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNullMetrics_ThrowsInvalidOperationException()
    {
        var project = new Project
        {
            Name = "Test",
            Milestones = new() { new Milestone { Name = "M1", TargetDate = DateTime.Now, Status = MilestoneStatus.Future } },
            WorkItems = new(),
            CompletionPercentage = -1,
            HealthStatus = (HealthStatus)999
        };

        var validationResult = new ValidationResult { IsValid = false };
        validationResult.AddError("HEALTH_STATUS_INVALID", "Invalid health status");

        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);
        _mockValidator.Setup(x => x.ValidateProjectData(It.IsAny<Project>()))
            .Returns(validationResult);

        var json = JsonSerializer.Serialize(project);
        EnsureTestDataFile(json);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _dataProvider.LoadProjectDataAsync());

        Assert.NotNull(exception);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNullWorkItems_ThrowsInvalidOperationException()
    {
        var project = InvalidDataFixtures.ProjectWithNullWorkItems;
        var validationResult = new ValidationResult { IsValid = false };
        validationResult.AddError("WORKITEMS_NULL", "WorkItems cannot be null", nameof(Project.WorkItems));

        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);
        _mockValidator.Setup(x => x.ValidateProjectData(It.IsAny<Project>()))
            .Returns(validationResult);

        var json = JsonSerializer.Serialize(project);
        EnsureTestDataFile(json);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _dataProvider.LoadProjectDataAsync());

        Assert.Contains("WorkItem", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithInvalidCompletionPercentage_ThrowsInvalidOperationException()
    {
        var project = InvalidDataFixtures.ProjectWithInvalidCompletionPercentage;
        var validationResult = new ValidationResult { IsValid = false };
        validationResult.AddError("COMPLETION_PERCENTAGE_INVALID", "Percentage must be 0-100", nameof(Project.CompletionPercentage));

        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);
        _mockValidator.Setup(x => x.ValidateProjectData(It.IsAny<Project>()))
            .Returns(validationResult);

        var json = JsonSerializer.Serialize(project);
        EnsureTestDataFile(json);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _dataProvider.LoadProjectDataAsync());

        Assert.Contains("validation failed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InvalidateCache_RemovesProjectFromCache()
    {
        _dataProvider.InvalidateCache();

        _mockCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoadProjectDataAsync_LogsSuccessfulLoad()
    {
        var validProject = InvalidDataFixtures.ValidProject;
        var validationResult = new ValidationResult { IsValid = true };

        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);
        _mockValidator.Setup(x => x.ValidateProjectData(It.IsAny<Project>()))
            .Returns(validationResult);
        _mockCache.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        EnsureTestDataFile(InvalidDataFixtures.ValidJsonString);

        await _dataProvider.LoadProjectDataAsync();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Loading project data")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadProjectDataAsync_LogsValidationErrors()
    {
        var project = InvalidDataFixtures.ProjectWithEmptyName;
        var validationResult = new ValidationResult { IsValid = false };
        validationResult.AddError("NAME_EMPTY", "Name is required", nameof(Project.Name));

        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);
        _mockValidator.Setup(x => x.ValidateProjectData(It.IsAny<Project>()))
            .Returns(validationResult);

        var json = JsonSerializer.Serialize(project);
        EnsureTestDataFile(json);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _dataProvider.LoadProjectDataAsync());

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadProjectDataAsync_CachesProjectWithOneHourTTL()
    {
        var validProject = InvalidDataFixtures.ValidProject;
        var validationResult = new ValidationResult { IsValid = true };

        _mockCache.Setup(x => x.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project?)null);
        _mockValidator.Setup(x => x.ValidateProjectData(It.IsAny<Project>()))
            .Returns(validationResult);
        _mockCache.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        EnsureTestDataFile(InvalidDataFixtures.ValidJsonString);

        await _dataProvider.LoadProjectDataAsync();

        _mockCache.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<Project>(),
                It.Is<TimeSpan?>(ts => ts.HasValue && ts.Value.TotalHours == 1)),
            Times.Once);
    }

    private void EnsureTestDataFile(string content)
    {
        var directory = Path.GetDirectoryName(_testDataPath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(_testDataPath, content);
    }

    private void DeleteTestDataFile()
    {
        if (File.Exists(_testDataPath))
        {
            File.Delete(_testDataPath);
        }
    }

    public void Dispose()
    {
        DeleteTestDataFile();
    }
}