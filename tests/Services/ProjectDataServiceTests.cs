using System.Text.Json;
using AgentSquad.Runner.Data;
using AgentSquad.Runner.Data.Exceptions;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Services;

public class ProjectDataServiceTests
{
    private readonly Mock<ILogger<ProjectDataService>> _mockLogger;
    private readonly ProjectDataService _service;

    public ProjectDataServiceTests()
    {
        _mockLogger = new Mock<ILogger<ProjectDataService>>();
        _service = new ProjectDataService(_mockLogger.Object);
    }

    private string CreateValidProjectDataJson()
    {
        return @"{
            ""project"": {
                ""name"": ""Test Project"",
                ""description"": ""Test Description"",
                ""startDate"": ""2026-04-01"",
                ""endDate"": ""2026-06-30"",
                ""status"": ""OnTrack"",
                ""sponsor"": ""Test Sponsor"",
                ""projectManager"": ""Test PM""
            },
            ""milestones"": [
                {
                    ""id"": ""m1"",
                    ""name"": ""Design Complete"",
                    ""targetDate"": ""2026-04-15"",
                    ""actualDate"": ""2026-04-12"",
                    ""status"": ""Completed"",
                    ""completionPercentage"": 100
                }
            ],
            ""tasks"": [
                {
                    ""id"": ""t1"",
                    ""name"": ""Task 1"",
                    ""status"": ""Shipped"",
                    ""assignedTo"": ""John Doe"",
                    ""dueDate"": ""2026-04-20"",
                    ""estimatedDays"": 5,
                    ""relatedMilestone"": ""m1""
                }
            ],
            ""metrics"": {
                ""totalTasks"": 10,
                ""completedTasks"": 3,
                ""inProgressTasks"": 5,
                ""carriedOverTasks"": 2,
                ""estimatedBurndownRate"": 1.2
            }
        }";
    }

    private string CreateComplexProjectDataJson()
    {
        return @"{
            ""project"": {
                ""name"": ""Complex Project"",
                ""description"": ""Complex project with multiple items"",
                ""startDate"": ""2026-04-01"",
                ""endDate"": ""2026-06-30"",
                ""status"": ""AtRisk"",
                ""sponsor"": ""VP Product"",
                ""projectManager"": ""Jane Smith""
            },
            ""milestones"": [
                {
                    ""id"": ""m1"",
                    ""name"": ""Milestone 1"",
                    ""targetDate"": ""2026-04-15"",
                    ""actualDate"": ""2026-04-12"",
                    ""status"": ""Completed"",
                    ""completionPercentage"": 100
                },
                {
                    ""id"": ""m2"",
                    ""name"": ""Milestone 2"",
                    ""targetDate"": ""2026-05-15"",
                    ""actualDate"": null,
                    ""status"": ""InProgress"",
                    ""completionPercentage"": 50
                },
                {
                    ""id"": ""m3"",
                    ""name"": ""Milestone 3"",
                    ""targetDate"": ""2026-06-15"",
                    ""actualDate"": null,
                    ""status"": ""Pending"",
                    ""completionPercentage"": 0
                }
            ],
            ""tasks"": [
                {
                    ""id"": ""t1"",
                    ""name"": ""Task 1"",
                    ""status"": ""Shipped"",
                    ""assignedTo"": ""John Doe"",
                    ""dueDate"": ""2026-04-20"",
                    ""estimatedDays"": 5,
                    ""relatedMilestone"": ""m1""
                },
                {
                    ""id"": ""t2"",
                    ""name"": ""Task 2"",
                    ""status"": ""InProgress"",
                    ""assignedTo"": ""Alice Brown"",
                    ""dueDate"": ""2026-05-10"",
                    ""estimatedDays"": 8,
                    ""relatedMilestone"": ""m2""
                },
                {
                    ""id"": ""t3"",
                    ""name"": ""Task 3"",
                    ""status"": ""CarriedOver"",
                    ""assignedTo"": ""Bob Wilson"",
                    ""dueDate"": ""2026-05-15"",
                    ""estimatedDays"": 6,
                    ""relatedMilestone"": ""m2""
                }
            ],
            ""metrics"": {
                ""totalTasks"": 15,
                ""completedTasks"": 5,
                ""inProgressTasks"": 7,
                ""carriedOverTasks"": 3,
                ""estimatedBurndownRate"": 1.5
            }
        }";
    }

    private string CreateMixedCaseJson()
    {
        return @"{
            ""Project"": {
                ""Name"": ""Mixed Case Project"",
                ""Description"": ""Project with mixed case properties"",
                ""StartDate"": ""2026-04-01"",
                ""EndDate"": ""2026-06-30"",
                ""Status"": ""OnTrack"",
                ""Sponsor"": ""Executive"",
                ""ProjectManager"": ""Manager Name""
            },
            ""Milestones"": [
                {
                    ""Id"": ""m1"",
                    ""Name"": ""Design"",
                    ""TargetDate"": ""2026-04-15"",
                    ""ActualDate"": null,
                    ""Status"": ""Completed"",
                    ""CompletionPercentage"": 100
                }
            ],
            ""Tasks"": [
                {
                    ""Id"": ""t1"",
                    ""Name"": ""Task"",
                    ""Status"": ""Shipped"",
                    ""AssignedTo"": ""Person"",
                    ""DueDate"": ""2026-04-20"",
                    ""EstimatedDays"": 5,
                    ""RelatedMilestone"": ""m1""
                }
            ],
            ""Metrics"": {
                ""TotalTasks"": 10,
                ""CompletedTasks"": 5,
                ""InProgressTasks"": 3,
                ""CarriedOverTasks"": 2,
                ""EstimatedBurndownRate"": 1.0
            }
        }";
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidJson_ReturnsProjectData()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = CreateValidProjectDataJson();
            await File.WriteAllTextAsync(tempFile, json);

            var result = await _service.LoadProjectDataAsync(tempFile);

            Assert.NotNull(result);
            Assert.NotNull(result.Project);
            Assert.Equal("Test Project", result.Project.Name);
            Assert.NotNull(result.Milestones);
            Assert.Single(result.Milestones);
            Assert.NotNull(result.Tasks);
            Assert.Single(result.Tasks);
            Assert.NotNull(result.Metrics);
            Assert.Equal(10, result.Metrics.TotalTasks);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingFile_ThrowsDataLoadException()
    {
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent_" + Guid.NewGuid() + ".json");

        var exception = await Assert.ThrowsAsync<DataLoadException>(
            () => _service.LoadProjectDataAsync(nonExistentFile));

        Assert.Contains("not found in wwwroot directory", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMalformedJson_ThrowsDataLoadException()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "{ invalid json }");

            var exception = await Assert.ThrowsAsync<DataLoadException>(
                () => _service.LoadProjectDataAsync(tempFile));

            Assert.Contains("Invalid JSON format", exception.Message);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithComplexNestedJson_DeserializesCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = CreateComplexProjectDataJson();
            await File.WriteAllTextAsync(tempFile, json);

            var result = await _service.LoadProjectDataAsync(tempFile);

            Assert.Equal(3, result.Milestones.Count);
            Assert.Equal(3, result.Tasks.Count);
            Assert.Equal(15, result.Metrics.TotalTasks);
            Assert.Equal("Milestone 1", result.Milestones[0].Name);
            Assert.Equal("Task 2", result.Tasks[1].Name);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithCaseInsensitiveProperties_DeserializesCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = CreateMixedCaseJson();
            await File.WriteAllTextAsync(tempFile, json);

            var result = await _service.LoadProjectDataAsync(tempFile);

            Assert.NotNull(result);
            Assert.Equal("Mixed Case Project", result.Project.Name);
            Assert.Equal("Design", result.Milestones[0].Name);
            Assert.Equal("Task", result.Tasks[0].Name);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ValidateJsonSchema_WithValidJson_ReturnsTrue()
    {
        var json = CreateValidProjectDataJson();

        var result = _service.ValidateJsonSchema(json);

        Assert.True(result);
    }

    [Fact]
    public void ValidateJsonSchema_WithMalformedJson_ReturnsFalse()
    {
        var json = "{ invalid json }";

        var result = _service.ValidateJsonSchema(json);

        Assert.False(result);
    }

    [Fact]
    public void ValidateJsonSchema_WithNullInput_ReturnsFalse()
    {
        var result = _service.ValidateJsonSchema(null);

        Assert.False(result);
    }

    [Fact]
    public void ValidateJsonSchema_WithEmptyString_ReturnsFalse()
    {
        var result = _service.ValidateJsonSchema(string.Empty);

        Assert.False(result);
    }

    [Fact]
    public void ValidateJsonSchema_WithMissingRequiredFields_ReturnsFalse()
    {
        var json = @"{
            ""project"": { ""name"": ""Test"" },
            ""milestones"": [],
            ""tasks"": []
        }";

        var result = _service.ValidateJsonSchema(json);

        Assert.False(result);
    }

    [Fact]
    public async Task GetCachedData_BeforeLoad_ReturnsNull()
    {
        var result = _service.GetCachedData();

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCachedData_AfterLoad_ReturnsCachedData()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = CreateValidProjectDataJson();
            await File.WriteAllTextAsync(tempFile, json);

            var loadedData = await _service.LoadProjectDataAsync(tempFile);
            var cachedData = _service.GetCachedData();

            Assert.NotNull(cachedData);
            Assert.Equal(loadedData.Project.Name, cachedData.Project.Name);
            Assert.Equal(loadedData.Metrics.TotalTasks, cachedData.Metrics.TotalTasks);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task RefreshData_ClearsCache()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = CreateValidProjectDataJson();
            await File.WriteAllTextAsync(tempFile, json);

            await _service.LoadProjectDataAsync(tempFile);
            Assert.NotNull(_service.GetCachedData());

            _service.RefreshData();
            var result = _service.GetCachedData();

            Assert.Null(result);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithNullDeserializationResult_ThrowsDataLoadException()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "null");

            var exception = await Assert.ThrowsAsync<DataLoadException>(
                () => _service.LoadProjectDataAsync(tempFile));

            Assert.Contains("JSON deserialization resulted in null", exception.Message);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_CachesLoadedData()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = CreateValidProjectDataJson();
            await File.WriteAllTextAsync(tempFile, json);

            var result1 = await _service.LoadProjectDataAsync(tempFile);
            var result2 = _service.GetCachedData();

            Assert.Same(result1, result2);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}