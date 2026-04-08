using System.Text.Json;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Services.Models;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace AgentSquad.Runner.Tests.Services;

public class ProjectDataServiceTests
{
    private readonly Mock<ILogger<ProjectDataService>> _mockLogger;
    private readonly string _testDataPath;

    public ProjectDataServiceTests()
    {
        _mockLogger = new Mock<ILogger<ProjectDataService>>();
        _testDataPath = Path.Combine(Path.GetTempPath(), "test-data.json");
    }

    private void CreateTestDataFile(object dataObject)
    {
        var json = JsonSerializer.Serialize(dataObject, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_testDataPath, json);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidJson_ReturnsProjectData()
    {
        var testData = new
        {
            project = new { name = "Test Project", startDate = "2024-01-01T00:00:00Z", endDate = "2024-12-31T00:00:00Z", overallCompletionPercentage = 50 },
            milestones = new object[] { },
            tasks = new object[] { }
        };

        CreateTestDataFile(testData);
        var service = new ProjectDataService(_mockLogger.Object, _testDataPath);

        var result = await service.LoadProjectDataAsync();

        Assert.NotNull(result);
        Assert.NotNull(result.Project);
        Assert.Equal("Test Project", result.Project.Name);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingFile_ReturnsEmptyProjectData()
    {
        var service = new ProjectDataService(_mockLogger.Object, "/nonexistent/path/data.json");

        var result = await service.LoadProjectDataAsync();

        Assert.NotNull(result);
        Assert.Null(result.Project);
        Assert.Empty(result.Milestones);
        Assert.Empty(result.Tasks);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMalformedJson_ReturnsEmptyProjectDataAndLogsError()
    {
        File.WriteAllText(_testDataPath, "{ invalid json }");
        var service = new ProjectDataService(_mockLogger.Object, _testDataPath);

        var result = await service.LoadProjectDataAsync();

        Assert.NotNull(result);
        Assert.Null(result.Project);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidTaskData_PopulatesTasksWithCorrectFields()
    {
        var testData = new
        {
            project = new { name = "Test", startDate = "2024-01-01T00:00:00Z", endDate = "2024-12-31T00:00:00Z", overallCompletionPercentage = 50 },
            milestones = new object[] { },
            tasks = new[] {
                new { id = "1", name = "Task 1", assignedTo = "John Doe", dueDate = "2024-03-01T00:00:00Z", estimatedDays = 5, relatedMilestone = "Phase 1", status = "Shipped" }
            }
        };

        CreateTestDataFile(testData);
        var service = new ProjectDataService(_mockLogger.Object, _testDataPath);

        var result = await service.LoadProjectDataAsync();

        Assert.NotNull(result);
        Assert.Single(result.Tasks);
        Assert.Equal("Task 1", result.Tasks[0].Name);
        Assert.Equal("John Doe", result.Tasks[0].AssignedTo);
        Assert.Equal(5, result.Tasks[0].EstimatedDays);
    }

    [Fact]
    public void GetTaskStatusSummary_CountsTasksByStatus()
    {
        var projectData = new ProjectData
        {
            Tasks = new List<ProjectTask>
            {
                new() { Status = TaskStatus.Shipped },
                new() { Status = TaskStatus.Shipped },
                new() { Status = TaskStatus.InProgress },
                new() { Status = TaskStatus.CarriedOver }
            }
        };

        var summary = new ProjectDataService(_mockLogger.Object).GetTaskStatusSummary(projectData);

        Assert.Equal(2, summary.ShippedCount);
        Assert.Equal(1, summary.InProgressCount);
        Assert.Equal(1, summary.CarriedOverCount);
    }

    [Fact]
    public async Task OnInitializedAsync_WithValidData_DoesNotThrowException()
    {
        var testData = new
        {
            project = new { name = "Dashboard", startDate = "2024-01-01T00:00:00Z", endDate = "2024-12-31T00:00:00Z", overallCompletionPercentage = 75 },
            milestones = new[] { new { name = "M1", targetDate = "2024-06-01T00:00:00Z", status = "Completed", completionPercentage = 100 } },
            tasks = new[] { new { id = "t1", name = "Task", assignedTo = "User", dueDate = "2024-05-01T00:00:00Z", estimatedDays = 3, relatedMilestone = "M1", status = "Shipped" } }
        };

        CreateTestDataFile(testData);
        var service = new ProjectDataService(_mockLogger.Object, _testDataPath);

        var exception = await Record.ExceptionAsync(async () => await service.LoadProjectDataAsync());

        Assert.Null(exception);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNullFields_HandlesMissingValuesGracefully()
    {
        var testData = new
        {
            project = (object?)null,
            milestones = (object[]?)null,
            tasks = (object[]?)null
        };

        CreateTestDataFile(testData);
        var service = new ProjectDataService(_mockLogger.Object, _testDataPath);

        var result = await service.LoadProjectDataAsync();

        Assert.NotNull(result);
        Assert.Null(result.Project);
        Assert.Empty(result.Milestones);
        Assert.Empty(result.Tasks);
    }

    public void Dispose()
    {
        if (File.Exists(_testDataPath))
            File.Delete(_testDataPath);
    }
}