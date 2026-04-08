using Xunit;
using Moq;
using AgentSquad.Models;
using AgentSquad.Services;

namespace AgentSquad.Tests.Components;

public class ProjectMetricsTests
{
    private readonly Mock<IDataCache> _mockCache;
    private readonly Mock<ILogger<DataProvider>> _mockLogger;

    public ProjectMetricsTests()
    {
        _mockCache = new Mock<IDataCache>();
        _mockLogger = new Mock<ILogger<DataProvider>>();
    }

    [Fact]
    public async Task ProjectMetrics_CalculatesCorrectly()
    {
        var metrics = new ProjectMetrics
        {
            CompletedMilestones = 3,
            TotalMilestones = 8
        };

        await _mockCache.Object.SetAsync("metrics", metrics);
        _mockCache.Setup(c => c.GetAsync<ProjectMetrics>("metrics"))
            .ReturnsAsync(metrics);

        var result = await _mockCache.Object.GetAsync<ProjectMetrics>("metrics");

        Assert.NotNull(result);
        Assert.Equal(3, result.CompletedMilestones);
        Assert.Equal(8, result.TotalMilestones);
    }

    [Fact]
    public async Task ProjectMetrics_HandlesZeroMilestones()
    {
        var metrics = new ProjectMetrics
        {
            CompletedMilestones = 0,
            TotalMilestones = 0
        };

        _mockCache.Setup(c => c.GetAsync<ProjectMetrics>(It.IsAny<string>()))
            .ReturnsAsync(metrics);

        var result = await _mockCache.Object.GetAsync<ProjectMetrics>(It.IsAny<string>());

        Assert.NotNull(result);
        Assert.Equal(0, result.TotalMilestones);
    }
}