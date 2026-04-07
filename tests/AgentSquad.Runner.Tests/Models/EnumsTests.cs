using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Models;

public class EnumsTests
{
    [Fact]
    public void MilestoneStatus_HasRequiredValues()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(MilestoneStatus), "Completed"));
        Assert.True(Enum.IsDefined(typeof(MilestoneStatus), "InProgress"));
        Assert.True(Enum.IsDefined(typeof(MilestoneStatus), "AtRisk"));
        Assert.True(Enum.IsDefined(typeof(MilestoneStatus), "Future"));
    }

    [Fact]
    public void WorkItemStatus_HasRequiredValues()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(WorkItemStatus), "Shipped"));
        Assert.True(Enum.IsDefined(typeof(WorkItemStatus), "InProgress"));
        Assert.True(Enum.IsDefined(typeof(WorkItemStatus), "CarriedOver"));
    }

    [Fact]
    public void HealthStatus_HasRequiredValues()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(HealthStatus), "OnTrack"));
        Assert.True(Enum.IsDefined(typeof(HealthStatus), "AtRisk"));
        Assert.True(Enum.IsDefined(typeof(HealthStatus), "Blocked"));
    }

    [Theory]
    [InlineData(0, "Completed")]
    [InlineData(1, "InProgress")]
    [InlineData(2, "AtRisk")]
    [InlineData(3, "Future")]
    public void MilestoneStatus_HasCorrectOrdinalValues(int value, string name)
    {
        // Assert
        Assert.Equal(name, ((MilestoneStatus)value).ToString());
    }

    [Theory]
    [InlineData(0, "Shipped")]
    [InlineData(1, "InProgress")]
    [InlineData(2, "CarriedOver")]
    public void WorkItemStatus_HasCorrectOrdinalValues(int value, string name)
    {
        // Assert
        Assert.Equal(name, ((WorkItemStatus)value).ToString());
    }
}