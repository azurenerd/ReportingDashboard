using Xunit;
using AgentSquad.Services;
using AgentSquad.Services.Models;

namespace AgentSquad.Tests.Services;

public class ProjectDataServiceTests : IAsyncLifetime
{
    private readonly ProjectDataService _service;
    private readonly string _testJsonPath;
    private readonly string _testDirectory;

    public ProjectDataServiceTests()
    {
        _service = new ProjectDataService();
        _testDirectory = Path.Combine(Path.GetTempPath(), "MilestoneTests");
        _testJsonPath = Path.Combine(_testDirectory, "test-data.json");
    }

    public Task InitializeAsync()
    {
        if (!Directory.Exists(_testDirectory))
        {
            Directory.CreateDirectory(_testDirectory);
        }
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch { }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task LoadProjectDataAsync_ReturnsDefaultData_WhenFileDoesNotExist()
    {
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.json");
        var result = await _service.LoadProjectDataAsync(nonExistentPath);

        Assert.NotNull(result);
        Assert.NotEmpty(result.ProjectName);
    }

    [Fact]
    public async Task LoadProjectDataAsync_ReturnsProjectDataFromJson()
    {
        var json = @"{
            ""projectName"": ""Test Project"",
            ""description"": ""Test Description"",
            ""startDate"": ""2026-02-15T00:00:00"",
            ""endDate"": ""2026-08-15T00:00:00"",
            ""totalTasks"": 24,
            ""completedTasks"": 8,
            ""milestones"": []
        }";

        await File.WriteAllTextAsync(_testJsonPath, json);
        var result = await _service.LoadProjectDataAsync(_testJsonPath);

        Assert.NotNull(result);
        Assert.Equal("Test Project", result.ProjectName);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(24, result.TotalTasks);
        Assert.Equal(8, result.CompletedTasks);
    }

    [Fact]
    public async Task LoadProjectDataAsync_ReturnsMilestonesFromJson()
    {
        var json = @"{
            ""projectName"": ""Test Project"",
            ""description"": ""Test Description"",
            ""startDate"": ""2026-02-15T00:00:00"",
            ""endDate"": ""2026-08-15T00:00:00"",
            ""totalTasks"": 24,
            ""completedTasks"": 8,
            ""milestones"": [
                {
                    ""id"": ""m1"",
                    ""name"": ""Phase 1"",
                    ""targetDate"": ""2026-04-15T00:00:00"",
                    ""actualDate"": null,
                    ""status"": 0,
                    ""completionPercentage"": 100
                }
            ]
        }";

        await File.WriteAllTextAsync(_testJsonPath, json);
        var result = await _service.LoadProjectDataAsync(_testJsonPath);

        Assert.NotEmpty(result.Milestones);
        Assert.Equal("Phase 1", result.Milestones[0].Name);
        Assert.Equal(MilestoneStatus.Completed, result.Milestones[0].Status);
    }

    [Fact]
    public async Task LoadProjectDataAsync_HandlesMalformedJson()
    {
        var malformedJson = @"{ invalid json }";
        await File.WriteAllTextAsync(_testJsonPath, malformedJson);

        var result = await _service.LoadProjectDataAsync(_testJsonPath);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task LoadProjectDataAsync_HandlesEmptyJson()
    {
        await File.WriteAllTextAsync(_testJsonPath, string.Empty);

        var result = await _service.LoadProjectDataAsync(_testJsonPath);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task LoadProjectDataAsync_HandlesNullJson()
    {
        var nullJson = "null";
        await File.WriteAllTextAsync(_testJsonPath, nullJson);

        var result = await _service.LoadProjectDataAsync(_testJsonPath);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetDefaultProjectData_ReturnsValidProjectData()
    {
        var result = _service.GetDefaultProjectData();

        Assert.NotNull(result);
        Assert.NotEmpty(result.ProjectName);
        Assert.True(result.StartDate < result.EndDate);
    }

    [Fact]
    public async Task GetDefaultProjectData_ReturnsMilestones()
    {
        var result = _service.GetDefaultProjectData();

        Assert.NotEmpty(result.Milestones);
        Assert.True(result.Milestones.Count >= 3);
    }

    [Fact]
    public async Task GetDefaultProjectData_HasMilestoneWithAllStatuses()
    {
        var result = _service.GetDefaultProjectData();

        var statuses = result.Milestones.Select(m => m.Status).Distinct().ToList();
        Assert.Contains(MilestoneStatus.Completed, statuses);
        Assert.Contains(MilestoneStatus.InProgress, statuses);
        Assert.Contains(MilestoneStatus.Pending, statuses);
    }

    [Fact]
    public async Task GetDefaultProjectData_MilestonesHaveValidDates()
    {
        var result = _service.GetDefaultProjectData();

        foreach (var milestone in result.Milestones)
        {
            Assert.NotEqual(default(DateTime), milestone.TargetDate);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_IsCaseInsensitive()
    {
        var json = @"{
            ""ProjectName"": ""Test"",
            ""Description"": ""Test"",
            ""StartDate"": ""2026-02-15T00:00:00"",
            ""EndDate"": ""2026-08-15T00:00:00"",
            ""TotalTasks"": 24,
            ""CompletedTasks"": 8,
            ""Milestones"": []
        }";

        await File.WriteAllTextAsync(_testJsonPath, json);
        var result = await _service.LoadProjectDataAsync(_testJsonPath);

        Assert.NotNull(result);
        Assert.Equal("Test", result.ProjectName);
    }

    [Fact]
    public async Task GetDefaultProjectData_StartDateIsInPast()
    {
        var result = _service.GetDefaultProjectData();
        Assert.True(result.StartDate < DateTime.Now);
    }

    [Fact]
    public async Task GetDefaultProjectData_EndDateIsInFuture()
    {
        var result = _service.GetDefaultProjectData();
        Assert.True(result.EndDate > DateTime.Now);
    }

    [Fact]
    public async Task GetDefaultProjectData_CompletedMilestonesHave100Percent()
    {
        var result = _service.GetDefaultProjectData();
        var completedMilestones = result.Milestones.Where(m => m.Status == MilestoneStatus.Completed);

        foreach (var milestone in completedMilestones)
        {
            Assert.Equal(100, milestone.CompletionPercentage);
        }
    }

    [Fact]
    public async Task GetDefaultProjectData_PendingMilestonesHave0Percent()
    {
        var result = _service.GetDefaultProjectData();
        var pendingMilestones = result.Milestones.Where(m => m.Status == MilestoneStatus.Pending);

        foreach (var milestone in pendingMilestones)
        {
            Assert.Equal(0, milestone.CompletionPercentage);
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_PreservesDateFormat()
    {
        var json = @"{
            ""projectName"": ""Test"",
            ""description"": ""Test"",
            ""startDate"": ""2026-02-15T00:00:00"",
            ""endDate"": ""2026-08-15T00:00:00"",
            ""totalTasks"": 24,
            ""completedTasks"": 8,
            ""milestones"": []
        }";

        await File.WriteAllTextAsync(_testJsonPath, json);
        var result = await _service.LoadProjectDataAsync(_testJsonPath);

        Assert.Equal(new DateTime(2026, 02, 15), result.StartDate);
        Assert.Equal(new DateTime(2026, 08, 15), result.EndDate);
    }

    [Fact]
    public async Task LoadProjectDataAsync_AllowsMissingOptionalFields()
    {
        var json = @"{
            ""projectName"": ""Test"",
            ""description"": ""Test"",
            ""startDate"": ""2026-02-15T00:00:00"",
            ""endDate"": ""2026-08-15T00:00:00""
        }";

        await File.WriteAllTextAsync(_testJsonPath, json);
        var result = await _service.LoadProjectDataAsync(_testJsonPath);

        Assert.NotNull(result);
    }
}