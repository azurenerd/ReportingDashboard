using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Models.Tests;

public class ModelEnumTests
{
    [Fact]
    public void TestMilestoneStatusEnum_AllValuesPresent()
    {
        // Assert
        Assert.Equal(MilestoneStatus.Completed, MilestoneStatus.Completed);
        Assert.Equal(MilestoneStatus.InProgress, MilestoneStatus.InProgress);
        Assert.Equal(MilestoneStatus.AtRisk, MilestoneStatus.AtRisk);
        Assert.Equal(MilestoneStatus.Future, MilestoneStatus.Future);
    }

    [Fact]
    public void TestWorkItemStatusEnum_AllValuesPresent()
    {
        // Assert
        Assert.Equal(WorkItemStatus.Shipped, WorkItemStatus.Shipped);
        Assert.Equal(WorkItemStatus.InProgress, WorkItemStatus.InProgress);
        Assert.Equal(WorkItemStatus.CarriedOver, WorkItemStatus.CarriedOver);
    }

    [Fact]
    public void TestHealthStatusEnum_AllValuesPresent()
    {
        // Assert
        Assert.Equal(HealthStatus.OnTrack, HealthStatus.OnTrack);
        Assert.Equal(HealthStatus.AtRisk, HealthStatus.AtRisk);
        Assert.Equal(HealthStatus.Blocked, HealthStatus.Blocked);
    }
}