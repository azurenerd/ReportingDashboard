using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace AgentSquad.Runner.Tests;

public class DataServiceTests
{
    private readonly Mock<ILogger<DataService>> _mockLogger;

    public DataServiceTests()
    {
        _mockLogger = new Mock<ILogger<DataService>>();
    }

    #region Valid Data Tests

    [Fact]
    public async Task ReadProjectDataAsync_WithValidData_ReturnsProjectStatus()
    {
        // Arrange
        var validJson = """
        {
            "milestones": [
                {
                    "id": "m1",
                    "name": "Project Kickoff",
                    "targetDate": "2026-04-01T00:00:00Z",
                    "status": "Completed"
                },
                {
                    "id": "m2",
                    "name": "Design Review",
                    "targetDate": "2026-04-15T00:00:00Z",
                    "status": "OnTrack"
                }
            ],
            "tasks": [
                {
                    "id": "t1",
                    "title": "Design Phase",
                    "description": "Complete design mockups",
                    "status": "Completed",
                    "assignedTo": "Alice Johnson",
                    "dueDate": "2026-04-05T00:00:00Z"
                },
                {
                    "id": "t2",
                    "title": "Development",
                    "description": "Build core features",
                    "status": "InProgress",
                    "assignedTo": "Bob Smith",
                    "dueDate": "2026-04-20T00:00:00Z"
                }
            ]
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, validJson);

        try
        {
            var dataService = new DataService(_mockLogger.Object);

            // Act
            var result = await dataService.ReadProjectDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Milestones);
            Assert.NotNull(result.Tasks);
            Assert.Equal(2, result.Milestones.Count);
            Assert.Equal(2, result.Tasks.Count);
            Assert.Equal("m1", result.Milestones[0].Id);
            Assert.Equal("Project Kickoff", result.Milestones[0].Name);
            Assert.Equal("t1", result.Tasks[0].Id);
            Assert.Equal("Design Phase", result.Tasks[0].Title);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadProjectDataAsync_WithEmptyMilestonesAndTasks_ReturnsProjectStatus()
    {
        // Arrange
        var emptyJson = """
        {
            "milestones": [],
            "tasks": []
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, emptyJson);

        try
        {
            var dataService = new DataService(_mockLogger.Object);

            // Act
            var result = await dataService.ReadProjectDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Milestones);
            Assert.Empty(result.Tasks);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion

    #region File I/O Error Tests

    [Fact]
    public async Task ReadProjectDataAsync_WhenFileNotFound_ThrowsFileReadException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.json");
        var dataService = new DataService(_mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileReadException>(
            () => dataService.ReadProjectDataAsync());

        Assert.NotNull(exception);
        Assert.Contains("data.json not found in wwwroot/data directory", exception.Message);
    }

    #endregion

    #region JSON Parse Error Tests

    [Fact]
    public async Task ReadProjectDataAsync_WithMalformedJson_ThrowsJsonParseException()
    {
        // Arrange
        var malformedJson = """
        {
            "milestones": [
                {
                    "id": "m1",
                    "name": "Project Kickoff",
                    "targetDate": "2026-04-01T00:00:00Z",
                    "status": "Completed"
        """; // Missing closing braces

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, malformedJson);

        try
        {
            var dataService = new DataService(_mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<JsonParseException>(
                () => dataService.ReadProjectDataAsync());

            Assert.NotNull(exception);
            Assert.Contains("Invalid JSON format in data.json", exception.Message);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadProjectDataAsync_WithInvalidJsonStructure_ThrowsJsonParseException()
    {
        // Arrange
        var invalidJson = "not valid json at all {[]}";

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, invalidJson);

        try
        {
            var dataService = new DataService(_mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<JsonParseException>(
                () => dataService.ReadProjectDataAsync());

            Assert.NotNull(exception);
            Assert.Contains("Invalid JSON format in data.json", exception.Message);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion

    #region Validation Error Tests

    [Fact]
    public async Task ReadProjectDataAsync_WithDuplicateMilestoneIds_ThrowsValidationException()
    {
        // Arrange
        var duplicateIdJson = """
        {
            "milestones": [
                {
                    "id": "m1",
                    "name": "Project Kickoff",
                    "targetDate": "2026-04-01T00:00:00Z",
                    "status": "Completed"
                },
                {
                    "id": "m1",
                    "name": "Design Review",
                    "targetDate": "2026-04-15T00:00:00Z",
                    "status": "OnTrack"
                }
            ],
            "tasks": []
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, duplicateIdJson);

        try
        {
            var dataService = new DataService(_mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => dataService.ReadProjectDataAsync());

            Assert.NotNull(exception);
            Assert.Contains("Duplicate milestone ID: m1", exception.Message);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadProjectDataAsync_WithDuplicateTaskIds_ThrowsValidationException()
    {
        // Arrange
        var duplicateIdJson = """
        {
            "milestones": [],
            "tasks": [
                {
                    "id": "t1",
                    "title": "Design Phase",
                    "description": "Complete design mockups",
                    "status": "Completed",
                    "assignedTo": "Alice Johnson",
                    "dueDate": "2026-04-05T00:00:00Z"
                },
                {
                    "id": "t1",
                    "title": "Development",
                    "description": "Build core features",
                    "status": "InProgress",
                    "assignedTo": "Bob Smith",
                    "dueDate": "2026-04-20T00:00:00Z"
                }
            ]
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, duplicateIdJson);

        try
        {
            var dataService = new DataService(_mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => dataService.ReadProjectDataAsync());

            Assert.NotNull(exception);
            Assert.Contains("Duplicate task ID: t1", exception.Message);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadProjectDataAsync_WithInvalidTaskStatus_ThrowsValidationException()
    {
        // Arrange
        var invalidStatusJson = """
        {
            "milestones": [],
            "tasks": [
                {
                    "id": "t1",
                    "title": "Design Phase",
                    "description": "Complete design mockups",
                    "status": "InvalidStatus",
                    "assignedTo": "Alice Johnson",
                    "dueDate": "2026-04-05T00:00:00Z"
                }
            ]
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, invalidStatusJson);

        try
        {
            var dataService = new DataService(_mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => dataService.ReadProjectDataAsync());

            Assert.NotNull(exception);
            Assert.Contains("Invalid task status", exception.Message);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadProjectDataAsync_WithInvalidMilestoneStatus_ThrowsValidationException()
    {
        // Arrange
        var invalidStatusJson = """
        {
            "milestones": [
                {
                    "id": "m1",
                    "name": "Project Kickoff",
                    "targetDate": "2026-04-01T00:00:00Z",
                    "status": "InvalidStatus"
                }
            ],
            "tasks": []
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, invalidStatusJson);

        try
        {
            var dataService = new DataService(_mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => dataService.ReadProjectDataAsync());

            Assert.NotNull(exception);
            Assert.Contains("Invalid milestone status", exception.Message);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadProjectDataAsync_WithInvalidTaskDate_ThrowsValidationException()
    {
        // Arrange
        var invalidDateJson = """
        {
            "milestones": [],
            "tasks": [
                {
                    "id": "t1",
                    "title": "Design Phase",
                    "description": "Complete design mockups",
                    "status": "Completed",
                    "assignedTo": "Alice Johnson",
                    "dueDate": "0001-01-01T00:00:00"
                }
            ]
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, invalidDateJson);

        try
        {
            var dataService = new DataService(_mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => dataService.ReadProjectDataAsync());

            Assert.NotNull(exception);
            Assert.Contains("invalid DueDate", exception.Message);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadProjectDataAsync_WithInvalidMilestoneDate_ThrowsValidationException()
    {
        // Arrange
        var invalidDateJson = """
        {
            "milestones": [
                {
                    "id": "m1",
                    "name": "Project Kickoff",
                    "targetDate": "0001-01-01T00:00:00",
                    "status": "Completed"
                }
            ],
            "tasks": []
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, invalidDateJson);

        try
        {
            var dataService = new DataService(_mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => dataService.ReadProjectDataAsync());

            Assert.NotNull(exception);
            Assert.Contains("invalid TargetDate", exception.Message);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task ReadProjectDataAsync_PerformsWithinTimeLimit()
    {
        // Arrange
        var largeJson = """
        {
            "milestones": [
                {"id": "m1", "name": "M1", "targetDate": "2026-04-01T00:00:00Z", "status": "Completed"},
                {"id": "m2", "name": "M2", "targetDate": "2026-04-05T00:00:00Z", "status": "OnTrack"},
                {"id": "m3", "name": "M3", "targetDate": "2026-04-10T00:00:00Z", "status": "AtRisk"},
                {"id": "m4", "name": "M4", "targetDate": "2026-04-15T00:00:00Z", "status": "Completed"},
                {"id": "m5", "name": "M5", "targetDate": "2026-04-20T00:00:00Z", "status": "OnTrack"}
            ],
            "tasks": [
                {"id": "t1", "title": "T1", "description": "Task 1", "status": "Completed", "assignedTo": "Alice", "dueDate": "2026-04-02T00:00:00Z"},
                {"id": "t2", "title": "T2", "description": "Task 2", "status": "InProgress", "assignedTo": "Bob", "dueDate": "2026-04-06T00:00:00Z"},
                {"id": "t3", "title": "T3", "description": "Task 3", "status": "Completed", "assignedTo": "Charlie", "dueDate": "2026-04-08T00:00:00Z"},
                {"id": "t4", "title": "T4", "description": "Task 4", "status": "CarriedOver", "assignedTo": "Diana", "dueDate": "2026-04-12T00:00:00Z"},
                {"id": "t5", "title": "T5", "description": "Task 5", "status": "InProgress", "assignedTo": "Eve", "dueDate": "2026-04-16T00:00:00Z"},
                {"id": "t6", "title": "T6", "description": "Task 6", "status": "Completed", "assignedTo": "Frank", "dueDate": "2026-04-18T00:00:00Z"},
                {"id": "t7", "title": "T7", "description": "Task 7", "status": "InProgress", "assignedTo": "Grace", "dueDate": "2026-04-19T00:00:00Z"},
                {"id": "t8", "title": "T8", "description": "Task 8", "status": "Completed", "assignedTo": "Henry", "dueDate": "2026-04-21T00:00:00Z"},
                {"id": "t9", "title": "T9", "description": "Task 9", "status": "CarriedOver", "assignedTo": "Iris", "dueDate": "2026-04-22T00:00:00Z"},
                {"id": "t10", "title": "T10", "description": "Task 10", "status": "InProgress", "assignedTo": "Jack", "dueDate": "2026-04-23T00:00:00Z"},
                {"id": "t11", "title": "T11", "description": "Task 11", "status": "Completed", "assignedTo": "Karen", "dueDate": "2026-04-24T00:00:00Z"},
                {"id": "t12", "title": "T12", "description": "Task 12", "status": "InProgress", "assignedTo": "Leo", "dueDate": "2026-04-25T00:00:00Z"}
            ]
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, largeJson);

        try
        {
            var dataService = new DataService(_mockLogger.Object);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var result = await dataService.ReadProjectDataAsync();

            stopwatch.Stop();

            // Assert
            Assert.NotNull(result);
            Assert.True(stopwatch.ElapsedMilliseconds < 1000,
                $"ReadProjectDataAsync took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
            Assert.Equal(5, result.Milestones.Count);
            Assert.Equal(12, result.Tasks.Count);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion
}