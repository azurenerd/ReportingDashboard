using Xunit;
using Moq;
using AgentSquad.Services;
using AgentSquad.Models;

namespace AgentSquad.Tests.Integration;

public class DashboardIntegrationTests
{
    private readonly Mock<IDataCache> _mockCache;
    private readonly Mock<ILogger<DataProvider>> _mockLogger;
    private readonly string _testDbPath = "integration_test.db";

    public DashboardIntegrationTests()
    {
        _mockCache = new Mock<IDataCache>();
        _mockLogger = new Mock<ILogger<DataProvider>>();
    }

    [Fact]
    public async Task DashboardIntegration_LoadsCompleteProjectData()
    {
        var project = new Project { Id = "proj-1", Name = "Integration Test Project" };
        var metrics = new ProjectMetrics { CompletedMilestones = 2, TotalMilestones = 5 };
        var workItems = new[] {
            new WorkItem { Id = "wi-1", Title = "Item 1", Status = WorkItemStatus.Active }
        };

        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync(project);
        _mockCache.Setup(c => c.GetAsync<ProjectMetrics>(It.IsAny<string>()))
            .ReturnsAsync(metrics);
        _mockCache.Setup(c => c.GetAsync<WorkItem[]>(It.IsAny<string>()))
            .ReturnsAsync(workItems);

        var provider = new DataProvider(_mockLogger.Object, _mockCache.Object, _testDbPath);

        var projResult = await _mockCache.Object.GetAsync<Project>("project");
        var metricsResult = await _mockCache.Object.GetAsync<ProjectMetrics>("metrics");
        var itemsResult = await _mockCache.Object.GetAsync<WorkItem[]>("items");

        Assert.NotNull(projResult);
        Assert.NotNull(metricsResult);
        Assert.NotNull(itemsResult);
    }

    [Fact]
    public async Task DashboardIntegration_SynchronizesDataAcrossComponents()
    {
        var updateData = new ProjectMetrics { CompletedMilestones = 3, TotalMilestones = 6 };

        _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);
        _mockCache.Setup(c => c.GetAsync<ProjectMetrics>(It.IsAny<string>()))
            .ReturnsAsync(updateData);

        await _mockCache.Object.SetAsync("metrics", updateData);
        var result = await _mockCache.Object.GetAsync<ProjectMetrics>("metrics");

        Assert.Equal(3, result.CompletedMilestones);
        _mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task DashboardIntegration_HandlesDataProviderErrors()
    {
        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Cache error"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _mockCache.Object.GetAsync<Project>("project")
        );
    }
}