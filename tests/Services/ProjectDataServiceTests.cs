using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using AgentSquad.Dashboard.Services;
using System.Text.Json;

namespace AgentSquad.Tests.Services;

public class ProjectDataServiceTests
{
    private readonly ProjectDataService _service;
    private readonly Mock<ILogger<ProjectDataService>> _loggerMock;

    public ProjectDataServiceTests()
    {
        _loggerMock = new Mock<ILogger<ProjectDataService>>();
        _service = new ProjectDataService(_loggerMock.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task LoadProjectDataAsync_WithValidJsonFile_ReturnsProjectData()
    {
        var validJson = """
        {
            "project": {
                "name": "Q2 Mobile App Release",
                "description": "Mobile app redesign and launch",
                "startDate": "2024-01-01",
                "endDate": "2024-06-30",
                "status": "OnTrack",
                "sponsor": "VP Engineering",
                "projectManager": "Alice Smith"
            },
            "milestones": [
                {
                    "id": "m1",
                    "name": "Design Complete",
                    "targetDate": "2024-02-15",
                    "actualDate": "2024-02-14",
                    "status": 0,
                    "completionPercentage": 100
                }
            ],
            "tasks": [
                {
                    "id": "t1",
                    "name": "API Integration",
                    "status": 0,
                    "assignedTo": "Bob Johnson",
                    "dueDate": "2024-03-01",
                    "estimatedDays": 5,
                    "relatedMilestone": "m1"
                }
            ],
            "metrics": {
                "totalTasks": 10,
                "completedTasks": 5,
                "inProgressTasks": 3,
                "carriedOverTasks": 2,
                "estimatedBurndownRate": 1.5
            }
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), "valid_data.json");
        await File.WriteAllTextAsync(tempFile, validJson);

        try
        {
            var result = await _service.LoadProjectDataAsync(tempFile);

            Assert.NotNull(result);
            Assert.NotNull(result.Project);
            Assert.Equal("Q2 Mobile App Release", result.Project.Name);
            Assert.Single(result.Milestones);
            Assert.Single(result.Tasks);
            Assert.NotNull(result.Metrics);
            Assert.Equal(10, result.Metrics.TotalTasks);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidData_CachesData()
    {
        var validJson = """
        {
            "project": { "name": "Test Project", "description": "", "startDate": "2024-01-01", "endDate": "2024-12-31", "status": "OnTrack", "sponsor": "", "projectManager": "" },
            "milestones": [],
            "tasks": [],
            "metrics": { "totalTasks": 0, "completedTasks": 0, "inProgressTasks": 0, "carriedOverTasks": 0, "estimatedBurndownRate": 0 }
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), "cache_test.json");
        await File.WriteAllTextAsync(tempFile, validJson);

        try
        {
            await _service.LoadProjectDataAsync(tempFile);
            var cached = _service.GetCachedData();

            Assert.NotNull(cached);
            Assert.Equal("Test Project", cached.Project?.Name);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetCachedData_AfterLoad_ReturnsProjectData()
    {
        var validJson = """
        {
            "project": { "name": "Cached Project", "description": "", "startDate": "2024-01-01", "endDate": "2024-12-31", "status": "OnTrack", "sponsor": "", "projectManager": "" },
            "milestones": [],
            "tasks": [],
            "metrics": { "totalTasks": 5, "completedTasks": 2, "inProgressTasks": 2, "carriedOverTasks": 1, "estimatedBurndownRate": 1.0 }
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), "get_cached.json");
        await File.WriteAllTextAsync(tempFile, validJson);

        try
        {
            await _service.LoadProjectDataAsync(tempFile);
            var cached = _service.GetCachedData();

            Assert.Equal("Cached Project", cached.Project?.Name);
            Assert.Equal(5, cached.Metrics?.TotalTasks);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task LoadProjectDataAsync_WithNonExistentFile_ThrowsDataLoadException()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "does_not_exist_" + Guid.NewGuid() + ".json");

        var exception = await Assert.ThrowsAsync<DataLoadException>(
            () => _service.LoadProjectDataAsync(nonExistentPath));

        Assert.Contains("data.json not found", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMalformedJson_ThrowsDataLoadException()
    {
        var malformedJson = "{ invalid json }";
        var tempFile = Path.Combine(Path.GetTempPath(), "malformed.json");
        await File.WriteAllTextAsync(tempFile, malformedJson);

        try
        {
            var exception = await Assert.ThrowsAsync<DataLoadException>(
                () => _service.LoadProjectDataAsync(tempFile));

            Assert.Contains("Invalid JSON format", exception.Message);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingProjectField_ThrowsDataLoadException()
    {
        var jsonMissingProject = """
        {
            "milestones": [],
            "tasks": [],
            "metrics": { "totalTasks": 0, "completedTasks": 0, "inProgressTasks": 0, "carriedOverTasks": 0, "estimatedBurndownRate": 0 }
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), "missing_project.json");
        await File.WriteAllTextAsync(tempFile, jsonMissingProject);

        try
        {
            var exception = await Assert.ThrowsAsync<DataLoadException>(
                () => _service.LoadProjectDataAsync(tempFile));

            Assert.Contains("Missing 'project' field", exception.Message);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithEmptyProjectName_ThrowsDataLoadException()
    {
        var jsonEmptyName = """
        {
            "project": { "name": "", "description": "", "startDate": "2024-01-01", "endDate": "2024-12-31", "status": "OnTrack", "sponsor": "", "projectManager": "" },
            "milestones": [],
            "tasks": [],
            "metrics": { "totalTasks": 0, "completedTasks": 0, "inProgressTasks": 0, "carriedOverTasks": 0, "estimatedBurndownRate": 0 }
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), "empty_name.json");
        await File.WriteAllTextAsync(tempFile, jsonEmptyName);

        try
        {
            var exception = await Assert.ThrowsAsync<DataLoadException>(
                () => _service.LoadProjectDataAsync(tempFile));

            Assert.Contains("Project name is required", exception.Message);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingMilestonesArray_ThrowsDataLoadException()
    {
        var jsonMissingMilestones = """
        {
            "project": { "name": "Test", "description": "", "startDate": "2024-01-01", "endDate": "2024-12-31", "status": "OnTrack", "sponsor": "", "projectManager": "" },
            "tasks": [],
            "metrics": { "totalTasks": 0, "completedTasks": 0, "inProgressTasks": 0, "carriedOverTasks": 0, "estimatedBurndownRate": 0 }
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), "missing_milestones.json");
        await File.WriteAllTextAsync(tempFile, jsonMissingMilestones);

        try
        {
            var exception = await Assert.ThrowsAsync<DataLoadException>(
                () => _service.LoadProjectDataAsync(tempFile));

            Assert.Contains("Missing 'milestones' array", exception.Message);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingTasksArray_ThrowsDataLoadException()
    {
        var jsonMissingTasks = """
        {
            "project": { "name": "Test", "description": "", "startDate": "2024-01-01", "endDate": "2024-12-31", "status": "OnTrack", "sponsor": "", "projectManager": "" },
            "milestones": [],
            "metrics": { "totalTasks": 0, "completedTasks": 0, "inProgressTasks": 0, "carriedOverTasks": 0, "estimatedBurndownRate": 0 }
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), "missing_tasks.json");
        await File.WriteAllTextAsync(tempFile, jsonMissingTasks);

        try
        {
            var exception = await Assert.ThrowsAsync<DataLoadException>(
                () => _service.LoadProjectDataAsync(tempFile));

            Assert.Contains("Missing 'tasks' array", exception.Message);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingMetricsObject_ThrowsDataLoadException()
    {
        var jsonMissingMetrics = """
        {
            "project": { "name": "Test", "description": "", "startDate": "2024-01-01", "endDate": "2024-12-31", "status": "OnTrack", "sponsor": "", "projectManager": "" },
            "milestones": [],
            "tasks": []
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), "missing_metrics.json");
        await File.WriteAllTextAsync(tempFile, jsonMissingMetrics);

        try
        {
            var exception = await Assert.ThrowsAsync<DataLoadException>(
                () => _service.LoadProjectDataAsync(tempFile));

            Assert.Contains("Missing 'metrics' object", exception.Message);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetCachedData_WithoutPriorLoad_ThrowsInvalidOperationException()
    {
        var newService = new ProjectDataService(_loggerMock.Object);

        var exception = Assert.Throws<InvalidOperationException>(
            () => newService.GetCachedData());

        Assert.Contains("No cached data available", exception.Message);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task LoadProjectDataAsync_WithCaseInsensitiveJson_DeserializesCorrectly()
    {
        var lowerCaseJson = """
        {
            "project": { "name": "CaseTest", "description": "", "startdate": "2024-01-01", "enddate": "2024-12-31", "status": "OnTrack", "sponsor": "", "projectmanager": "" },
            "milestones": [],
            "tasks": [],
            "metrics": { "totaltasks": 0, "completedtasks": 0, "inprogresstasks": 0, "carriedovertasks": 0, "estimatedburndownrate": 0 }
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), "case_insensitive.json");
        await File.WriteAllTextAsync(tempFile, lowerCaseJson);

        try
        {
            var result = await _service.LoadProjectDataAsync(tempFile);

            Assert.NotNull(result);
            Assert.Equal("CaseTest", result.Project?.Name);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithEmptyMilestonesAndTasks_ReturnsValidData()
    {
        var emptyJson = """
        {
            "project": { "name": "Empty Project", "description": "", "startDate": "2024-01-01", "endDate": "2024-12-31", "status": "OnTrack", "sponsor": "", "projectManager": "" },
            "milestones": [],
            "tasks": [],
            "metrics": { "totalTasks": 0, "completedTasks": 0, "inProgressTasks": 0, "carriedOverTasks": 0, "estimatedBurndownRate": 0 }
        }
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), "empty_data.json");
        await File.WriteAllTextAsync(tempFile, emptyJson);

        try
        {
            var result = await _service.LoadProjectDataAsync(tempFile);

            Assert.NotNull(result);
            Assert.Empty(result.Milestones);
            Assert.Empty(result.Tasks);
            Assert.Equal(0, result.Metrics?.TotalTasks);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithLargeDataSet_LoadsSuccessfully()
    {
        var milestones = string.Join(",", Enumerable.Range(1, 50)
            .Select(i => $"""
            {{
                "id": "m{i}",
                "name": "Milestone {i}",
                "targetDate": "2024-{(i % 12) + 1:D2}-15",
                "actualDate": null,
                "status": 1,
                "completionPercentage": {i * 2}
            }}
            """));

        var tasks = string.Join(",", Enumerable.Range(1, 100)
            .Select(i => $"""
            {{
                "id": "t{i}",
                "name": "Task {i}",
                "status": {i % 3},
                "assignedTo": "Team Member",
                "dueDate": "2024-06-30",
                "estimatedDays": 5,
                "relatedMilestone": "m1"
            }}
            """));

        var largeJson = $"""
        {{
            "project": {{ "name": "Large Project", "description": "", "startDate": "2024-01-01", "endDate": "2024-12-31", "status": "OnTrack", "sponsor": "", "projectManager": "" }},
            "milestones": [{milestones}],
            "tasks": [{tasks}],
            "metrics": {{ "totalTasks": 100, "completedTasks": 33, "inProgressTasks": 33, "carriedOverTasks": 34, "estimatedBurndownRate": 2.0 }}
        }}
        """;

        var tempFile = Path.Combine(Path.GetTempPath(), "large_data.json");
        await File.WriteAllTextAsync(tempFile, largeJson);

        try
        {
            var result = await _service.LoadProjectDataAsync(tempFile);

            Assert.NotNull(result);
            Assert.Equal(50, result.Milestones.Count);
            Assert.Equal(100, result.Tasks.Count);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    #endregion
}