using Xunit;
using Moq;
using AgentSquad.Models;
using AgentSquad.Services;

namespace AgentSquad.Tests.Components;

public class DashboardLayoutTests
{
    private readonly Mock<IDataCache> _mockCache;
    private readonly Mock<ILogger<DataProvider>> _mockLogger;

    public DashboardLayoutTests()
    {
        _mockCache = new Mock<IDataCache>();
        _mockLogger = new Mock<ILogger<DataProvider>>();
    }

    [Fact]
    public async Task DashboardLayout_LoadsProjectMetrics_OnInitialization()
    {
        var testData = new ProjectMetrics
        {
            Id = "proj-1",
            CompletedMilestones = 5,
            TotalMilestones = 10
        };

        _mockCache.Setup(c => c.GetAsync<ProjectMetrics>("metrics"))
            .ReturnsAsync(testData);

        var provider = new DataProvider(_mockLogger.Object, _mockCache.Object, "test.db");
        var result = await provider.GetProjectMetricsAsync();

        Assert.NotNull(result);
        Assert.Equal(5, result.CompletedMilestones);
    }

    [Fact]
    public async Task DashboardLayout_HandlesEmptyCache_Gracefully()
    {
        _mockCache.Setup(c => c.GetAsync<ProjectMetrics>(It.IsAny<string>()))
            .ReturnsAsync((ProjectMetrics)null);

        var provider = new DataProvider(_mockLogger.Object, _mockCache.Object, "test.db");
        var result = await provider.GetProjectMetricsAsync();

        Assert.Null(result);
    }
}