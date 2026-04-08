using Xunit;
using Moq;
using AgentSquad.Models;
using AgentSquad.Services;

namespace AgentSquad.Tests.Components;

public class MilestoneTimelineTests
{
    private readonly Mock<IDataCache> _mockCache;
    private readonly Mock<ILogger<DataProvider>> _mockLogger;

    public MilestoneTimelineTests()
    {
        _mockCache = new Mock<IDataCache>();
        _mockLogger = new Mock<ILogger<DataProvider>>();
    }

    [Fact]
    public async Task MilestoneTimeline_DisplaysMilestones_InChronologicalOrder()
    {
        var milestones = new[] {
            new Milestone { Id = "m1", Name = "Phase 1", DueDate = new DateTime(2026, 5, 1) },
            new Milestone { Id = "m2", Name = "Phase 2", DueDate = new DateTime(2026, 6, 1) }
        };

        _mockCache.Setup(c => c.GetAsync<Milestone[]>("milestones"))
            .ReturnsAsync(milestones);

        var provider = new DataProvider(_mockLogger.Object, _mockCache.Object, "test.db");
        var result = await provider.GetMilestonesAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
    }

    [Fact]
    public async Task MilestoneTimeline_HandlesMissingMilestones()
    {
        _mockCache.Setup(c => c.GetAsync<Milestone[]>(It.IsAny<string>()))
            .ReturnsAsync((Milestone[])null);

        var provider = new DataProvider(_mockLogger.Object, _mockCache.Object, "test.db");
        var result = await provider.GetMilestonesAsync();

        Assert.Null(result);
    }
}