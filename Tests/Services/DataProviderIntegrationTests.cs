using System.Diagnostics;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgentSquad.Runner.Tests.Services;

public class DataProviderIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly string _testDataPath;
    private readonly string _testDataDirectory;

    public DataProviderIntegrationTests()
    {
        _testDataDirectory = Path.Combine(Path.GetTempPath(), $"dataprovider_tests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataDirectory);
        _testDataPath = Path.Combine(_testDataDirectory, "data.json");

        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testDataDirectory);
        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(_testDataDirectory);

        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddScoped<IDataCache, DataCache>();
        services.AddScoped<IDataProvider>(sp =>
        {
            var cache = sp.GetRequiredService<IDataCache>();
            var logger = LoggerFactory.Create(c => c.AddConsole()).CreateLogger<DataProvider>();
            return new DataProvider(cache, logger, _mockEnvironment.Object);
        });
        services.AddLogging(c => c.AddConsole());

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidDataJson_LoadsAndPopulatesProject()
    {
        var exampleData = CreateExampleProjectJson();
        await File.WriteAllTextAsync(_testDataPath, exampleData);

        var dataProvider = _serviceProvider.GetRequiredService<IDataProvider>();

        var project = await dataProvider.LoadProjectDataAsync();

        Assert.NotNull(project);
        Assert.Equal("Executive Dashboard Project", project.Name);
        Assert.Equal("Q1 2024 Executive Reporting Dashboard Initiative", project.Description);
        Assert.Equal(45, project.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, project.HealthStatus);
        Assert.Equal(12, project.VelocityThisMonth);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidDataJson_PopulatesMilestones()
    {
        var exampleData = CreateExampleProjectJson();
        await File.WriteAllTextAsync(_testDataPath, exampleData);

        var dataProvider = _serviceProvider.GetRequiredService<IDataProvider>();

        var project = await dataProvider.LoadProjectDataAsync();

        Assert.NotNull(project.Milestones);
        Assert.True(project.Milestones.Count >= 1);
        var firstMilestone = project.Milestones[0];
        Assert.NotNull(firstMilestone);
        Assert.Equal("Phase 1 - Foundation", firstMilestone.Name);
        Assert.Equal(MilestoneStatus.Completed, firstMilestone.Status);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidDataJson_PopulatesWorkItems()
    {
        var exampleData = CreateExampleProjectJson();
        await File.WriteAllTextAsync(_testDataPath, exampleData);

        var dataProvider = _serviceProvider.GetRequiredService<IDataProvider>();

        var project = await dataProvider.LoadProjectDataAsync();

        Assert.NotNull(project.WorkItems);
        Assert.True(project.WorkItems.Count > 0);
        var shippedItem = project.WorkItems.FirstOrDefault(w => w.Status == WorkItemStatus.Shipped);
        Assert.NotNull(shippedItem);
        Assert.NotEmpty(shippedItem.Title);
    }

    [Fact]
    public async Task LoadProjectDataAsync_SecondCall_ReturnsCachedData()
    {
        var exampleData = CreateExampleProjectJson();
        await File.WriteAllTextAsync(_testDataPath, exampleData);

        var dataProvider = _serviceProvider.GetRequiredService<IDataProvider>();
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        var project1 = await dataProvider.LoadProjectDataAsync();
        stopwatch.Stop();
        var firstCallDuration = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        var project2 = await dataProvider.LoadProjectDataAsync();
        stopwatch.Stop();
        var secondCallDuration = stopwatch.ElapsedMilliseconds;

        Assert.NotNull(project1);
        Assert.NotNull(project2);
        Assert.Same(project1, project2);
        Assert.True(secondCallDuration < firstCallDuration);
    }

    [Fact]
    public async Task InvalidateCache_ForcesCacheReload()
    {
        var exampleData = CreateExampleProjectJson();
        await File.WriteAllTextAsync(_testDataPath, exampleData);

        var dataProvider = _serviceProvider.GetRequiredService<IDataProvider>();

        var project1 = await dataProvider.LoadProjectDataAsync();
        dataProvider.InvalidateCache();
        var project2 = await dataProvider.LoadProjectDataAsync();

        Assert.NotNull(project1);
        Assert.NotNull(project2);
        Assert.NotSame(project1, project2);
        Assert.Equal(project1.Name, project2.Name);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidDataJson_NestedObjectsPopulated()
    {
        var exampleData = CreateExampleProjectJson();
        await File.WriteAllTextAsync(_testDataPath, exampleData);

        var dataProvider = _serviceProvider.GetRequiredService<IDataProvider>();

        var project = await dataProvider.LoadProjectDataAsync();

        Assert.NotNull(project);
        Assert.NotNull(project.Name);
        Assert.NotNull(project.Milestones);
        Assert.NotNull(project.WorkItems);

        if (project.Milestones.Count > 0)
        {
            var milestone = project.Milestones[0];
            Assert.NotNull(milestone.Name);
            Assert.NotEqual(default, milestone.TargetDate);
            Assert.True(Enum.IsDefined(typeof(MilestoneStatus), milestone.Status));
        }

        if (project.WorkItems.Count > 0)
        {
            var workItem = project.WorkItems[0];
            Assert.NotNull(workItem.Title);
            Assert.True(Enum.IsDefined(typeof(WorkItemStatus), workItem.Status));
        }
    }

    [Fact]
    public async Task LoadProjectDataAsync_CacheKeyConsistency()
    {
        var exampleData = CreateExampleProjectJson();
        await File.WriteAllTextAsync(_testDataPath, exampleData);

        var dataProvider = _serviceProvider.GetRequiredService<IDataProvider>();

        var project1 = await dataProvider.LoadProjectDataAsync();
        var project2 = await dataProvider.LoadProjectDataAsync();
        var project3 = await dataProvider.LoadProjectDataAsync();

        Assert.Same(project1, project2);
        Assert.Same(project2, project3);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingFile_ThrowsFileNotFound()
    {
        var dataProvider = _serviceProvider.GetRequiredService<IDataProvider>();

        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => dataProvider.LoadProjectDataAsync());
        Assert.NotNull(exception);
        Assert.Contains("data.json", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithInvalidJson_ThrowsJsonException()
    {
        var invalidJson = "{ invalid json }";
        await File.WriteAllTextAsync(_testDataPath, invalidJson);

        var dataProvider = _serviceProvider.GetRequiredService<IDataProvider>();

        var exception = await Assert.ThrowsAsync<JsonException>(() => dataProvider.LoadProjectDataAsync());
        Assert.NotNull(exception);
        Assert.Contains("Invalid JSON", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingRequiredField_ThrowsInvalidOperation()
    {
        var invalidData = @"{
  ""description"": ""Missing project name"",
  ""startDate"": ""2024-01-01"",
  ""targetEndDate"": ""2024-12-31"",
  ""completionPercentage"": 45,
  ""healthStatus"": ""OnTrack"",
  ""velocityThisMonth"": 12,
  ""milestones"": [
    {
      ""name"": ""Phase 1"",
      ""targetDate"": ""2024-03-31"",
      ""status"": ""Completed""
    }
  ],
  ""workItems"": []
}";
        await File.WriteAllTextAsync(_testDataPath, invalidData);

        var dataProvider = _serviceProvider.GetRequiredService<IDataProvider>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => dataProvider.LoadProjectDataAsync());
        Assert.NotNull(exception);
        Assert.Contains("Project name", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithEmptyMilestones_ThrowsValidationError()
    {
        var invalidData = @"{
  ""name"": ""Project"",
  ""description"": ""Empty milestones"",
  ""startDate"": ""2024-01-01"",
  ""targetEndDate"": ""2024-12-31"",
  ""completionPercentage"": 45,
  ""healthStatus"": ""OnTrack"",
  ""velocityThisMonth"": 12,
  ""milestones"": [],
  ""workItems"": []
}";
        await File.WriteAllTextAsync(_testDataPath, invalidData);

        var dataProvider = _serviceProvider.GetRequiredService<IDataProvider>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => dataProvider.LoadProjectDataAsync());
        Assert.NotNull(exception);
        Assert.Contains("at least one milestone", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private string CreateExampleProjectJson()
    {
        return @"{
  ""name"": ""Executive Dashboard Project"",
  ""description"": ""Q1 2024 Executive Reporting Dashboard Initiative"",
  ""startDate"": ""2024-01-01"",
  ""targetEndDate"": ""2024-12-31"",
  ""completionPercentage"": 45,
  ""healthStatus"": ""OnTrack"",
  ""velocityThisMonth"": 12,
  ""milestones"": [
    {
      ""name"": ""Phase 1 - Foundation"",
      ""targetDate"": ""2024-03-31"",
      ""status"": ""Completed"",
      ""description"": ""Core infrastructure and data model setup""
    },
    {
      ""name"": ""Phase 2 - UI Components"",
      ""targetDate"": ""2024-06-30"",
      ""status"": ""InProgress"",
      ""description"": ""Build Blazor components for dashboard visualization""
    },
    {
      ""name"": ""Phase 3 - Launch"",
      ""targetDate"": ""2024-09-30"",
      ""status"": ""Future"",
      ""description"": ""Production deployment and optimization""
    }
  ],
  ""workItems"": [
    {
      ""title"": ""DataProvider Service Implementation"",
      ""description"": ""Implement JSON reading and caching service layer"",
      ""status"": ""Shipped"",
      ""assignedTo"": ""Engineering Team""
    },
    {
      ""title"": ""Dashboard Layout Component"",
      ""description"": ""Create main dashboard Blazor component with grid layout"",
      ""status"": ""InProgress"",
      ""assignedTo"": ""UI Team""
    },
    {
      ""title"": ""Milestone Timeline Visualization"",
      ""description"": ""Implement milestone timeline with Chart.js"",
      ""status"": ""InProgress"",
      ""assignedTo"": ""Frontend Team""
    },
    {
      ""title"": ""Project Metrics Display"",
      ""description"": ""Create KPI cards for project health indicators"",
      ""status"": ""CarriedOver"",
      ""assignedTo"": ""Design Team""
    }
  ]
}";
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataDirectory))
        {
            Directory.Delete(_testDataDirectory, recursive: true);
        }

        _serviceProvider?.Dispose();
    }
}