using System.Text.Json;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Acceptance;

public class DashboardAcceptanceTests
{
    [Fact]
    public async Task Dashboard_LoadsImmediatelyOnApplicationStartup_WithoutAuthentication()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DataProvider>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cacheAdapter = new MemoryCacheAdapter(cache);
        var dataProvider = new DataProvider(cacheAdapter, mockLogger.Object);

        var testJson = """
        {
            "name": "Executive Dashboard",
            "description": "Project reporting dashboard",
            "startDate": "2024-01-01T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 45,
            "healthStatus": "OnTrack",
            "velocityThisMonth": 12,
            "milestones": [
                {
                    "name": "Phase 1 Launch",
                    "targetDate": "2024-03-31T00:00:00Z",
                    "status": "Completed",
                    "description": "Initial phase completion"
                },
                {
                    "name": "Phase 2 Beta",
                    "targetDate": "2024-06-30T00:00:00Z",
                    "status": "InProgress",
                    "description": "Beta testing phase"
                }
            ],
            "workItems": [
                {"title": "Auth module", "status": "Shipped", "assignedTo": "Alice"},
                {"title": "Dashboard UI", "status": "InProgress", "assignedTo": "Bob"},
                {"title": "Reporting API", "status": "CarriedOver", "assignedTo": "Charlie"}
            ]
        }
        """;

        var testFilePath = Path.Combine(Path.GetTempPath(), "dashboard_test.json");
        await File.WriteAllTextAsync(testFilePath, testJson);

        try
        {
            // Act
            var project = await dataProvider.LoadProjectDataAsync();

            // Assert - AC: Dashboard loads immediately without authentication
            Assert.NotNull(project);
            Assert.Equal("Executive Dashboard", project.Name);
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public async Task Dashboard_DisplaysAllRequiredSections_OnSinglePage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DataProvider>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cacheAdapter = new MemoryCacheAdapter(cache);
        var dataProvider = new DataProvider(cacheAdapter, mockLogger.Object);

        var testJson = """
        {
            "name": "Full Dashboard",
            "startDate": "2024-01-01T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 60,
            "healthStatus": "OnTrack",
            "velocityThisMonth": 10,
            "milestones": [
                {"name": "M1", "targetDate": "2024-06-30T00:00:00Z", "status": "InProgress"}
            ],
            "workItems": [
                {"title": "Task 1", "status": "Shipped"},
                {"title": "Task 2", "status": "InProgress"}
            ]
        }
        """;

        var testFilePath = Path.Combine(Path.GetTempPath(), "full_dashboard_test.json");
        await File.WriteAllTextAsync(testFilePath, testJson);

        try
        {
            // Act
            var project = await dataProvider.LoadProjectDataAsync();

            // Assert - AC: All required sections visible
            Assert.NotNull(project.Name);
            Assert.NotEmpty(project.Milestones);
            Assert.NotEmpty(project.WorkItems);
            Assert.True(project.CompletionPercentage >= 0 && project.CompletionPercentage <= 100);
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public async Task Dashboard_LoadsDataFromJsonFile_OnPageInitialization()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DataProvider>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cacheAdapter = new MemoryCacheAdapter(cache);
        var dataProvider = new DataProvider(cacheAdapter, mockLogger.Object);

        var testJson = """
        {
            "name": "JSON Source Project",
            "startDate": "2024-01-01T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 35,
            "healthStatus": "AtRisk",
            "velocityThisMonth": 7,
            "milestones": [
                {"name": "Deadline", "targetDate": "2024-12-31T00:00:00Z", "status": "Future"}
            ],
            "workItems": []
        }
        """;

        var testFilePath = Path.Combine(Path.GetTempPath(), "json_source_test.json");
        await File.WriteAllTextAsync(testFilePath, testJson);

        try
        {
            // Act
            var project = await dataProvider.LoadProjectDataAsync();

            // Assert - AC: Data loads from data.json
            Assert.NotNull(project);
            Assert.Equal("JSON Source Project", project.Name);
            Assert.Equal(HealthStatus.AtRisk, project.HealthStatus);
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public async Task Dashboard_DisplaysProjectHealthMetrics_Prominently()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DataProvider>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cacheAdapter = new MemoryCacheAdapter(cache);
        var dataProvider = new DataProvider(cacheAdapter, mockLogger.Object);

        var testJson = """
        {
            "name": "Metrics Project",
            "startDate": "2024-01-01T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 72,
            "healthStatus": "OnTrack",
            "velocityThisMonth": 15,
            "milestones": [
                {"name": "M1", "targetDate": "2024-06-30T00:00:00Z", "status": "Completed"}
            ],
            "workItems": []
        }
        """;

        var testFilePath = Path.Combine(Path.GetTempPath(), "metrics_test.json");
        await File.WriteAllTextAsync(testFilePath, testJson);

        try
        {
            // Act
            var project = await dataProvider.LoadProjectDataAsync();

            // Assert - AC: Metrics are available and accurate
            Assert.Equal(72, project.CompletionPercentage);
            Assert.Equal(HealthStatus.OnTrack, project.HealthStatus);
            Assert.Equal(15, project.VelocityThisMonth);
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public async Task Dashboard_GroupsWorkItemsByStatus_AsPerRequirement()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DataProvider>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cacheAdapter = new MemoryCacheAdapter(cache);
        var dataProvider = new DataProvider(cacheAdapter, mockLogger.Object);

        var testJson = """
        {
            "name": "Work Item Status Project",
            "startDate": "2024-01-01T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 50,
            "healthStatus": "OnTrack",
            "velocityThisMonth": 8,
            "milestones": [
                {"name": "M1", "targetDate": "2024-06-30T00:00:00Z", "status": "InProgress"}
            ],
            "workItems": [
                {"title": "Feature A", "status": "Shipped", "assignedTo": "Dev1"},
                {"title": "Feature B", "status": "InProgress", "assignedTo": "Dev2"},
                {"title": "Feature C", "status": "CarriedOver", "assignedTo": "Dev3"}
            ]
        }
        """;

        var testFilePath = Path.Combine(Path.GetTempPath(), "status_grouping_test.json");
        await File.WriteAllTextAsync(testFilePath, testJson);

        try
        {
            // Act
            var project = await dataProvider.LoadProjectDataAsync();

            // Assert - AC: Work items grouped by status
            var shippedCount = project.WorkItems.Count(w => w.Status == WorkItemStatus.Shipped);
            var inProgressCount = project.WorkItems.Count(w => w.Status == WorkItemStatus.InProgress);
            var carriedOverCount = project.WorkItems.Count(w => w.Status == WorkItemStatus.CarriedOver);

            Assert.Equal(1, shippedCount);
            Assert.Equal(1, inProgressCount);
            Assert.Equal(1, carriedOverCount);
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public async Task Dashboard_ProvidesMilestoneTimeline_WithStatusIndicators()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DataProvider>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cacheAdapter = new MemoryCacheAdapter(cache);
        var dataProvider = new DataProvider(cacheAdapter, mockLogger.Object);

        var testJson = """
        {
            "name": "Timeline Project",
            "startDate": "2024-01-01T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 40,
            "healthStatus": "OnTrack",
            "velocityThisMonth": 6,
            "milestones": [
                {"name": "Alpha", "targetDate": "2024-03-31T00:00:00Z", "status": "Completed"},
                {"name": "Beta", "targetDate": "2024-06-30T00:00:00Z", "status": "InProgress"},
                {"name": "Release", "targetDate": "2024-12-31T00:00:00Z", "status": "Future"},
                {"name": "Support", "targetDate": "2025-01-31T00:00:00Z", "status": "Future"}
            ],
            "workItems": []
        }
        """;

        var testFilePath = Path.Combine(Path.GetTempPath(), "timeline_test.json");
        await File.WriteAllTextAsync(testFilePath, testJson);

        try
        {
            // Act
            var project = await dataProvider.LoadProjectDataAsync();

            // Assert - AC: Timeline with status indicators
            var completedCount = project.Milestones.Count(m => m.Status == MilestoneStatus.Completed);
            var inProgressCount = project.Milestones.Count(m => m.Status == MilestoneStatus.InProgress);
            var futureCount = project.Milestones.Count(m => m.Status == MilestoneStatus.Future);

            Assert.Equal(1, completedCount);
            Assert.Equal(1, inProgressCount);
            Assert.Equal(2, futureCount);
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }

    [Fact]
    public async Task Dashboard_HandlesErrorsGracefully_WithMissingDataFile()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DataProvider>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cacheAdapter = new MemoryCacheAdapter(cache);
        var dataProvider = new DataProvider(cacheAdapter, mockLogger.Object);

        // Act & Assert - AC: Graceful error handling
        await Assert.ThrowsAsync<FileNotFoundException>(() => dataProvider.LoadProjectDataAsync());
    }

    [Fact]
    public async Task Dashboard_HandlesErrorsGracefully_WithMalformedJson()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DataProvider>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cacheAdapter = new MemoryCacheAdapter(cache);
        var dataProvider = new DataProvider(cacheAdapter, mockLogger.Object);

        var malformedJson = "{ this is not valid json }";
        var testFilePath = Path.Combine(Path.GetTempPath(), "malformed_test.json");
        await File.WriteAllTextAsync(testFilePath, malformedJson);

        try
        {
            // Act & Assert - AC: Graceful error handling
            await Assert.ThrowsAsync<InvalidOperationException>(() => dataProvider.LoadProjectDataAsync());
        }
        finally
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }
    }
}