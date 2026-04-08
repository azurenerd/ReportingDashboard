using Xunit;
using AgentSquad.Services.Models;

namespace AgentSquad.Tests.Services.Models;

public class MilestoneStatusTests
{
    [Fact]
    public void MilestoneStatus_HasCompletedValue()
    {
        Assert.Equal(0, (int)MilestoneStatus.Completed);
    }

    [Fact]
    public void MilestoneStatus_HasInProgressValue()
    {
        Assert.Equal(1, (int)MilestoneStatus.InProgress);
    }

    [Fact]
    public void MilestoneStatus_HasPendingValue()
    {
        Assert.Equal(2, (int)MilestoneStatus.Pending);
    }

    [Fact]
    public void MilestoneStatus_CanBeCastToInt()
    {
        var status = MilestoneStatus.Completed;
        var intValue = (int)status;
        Assert.Equal(0, intValue);
    }

    [Fact]
    public void MilestoneStatus_CanBeCastFromInt()
    {
        var status = (MilestoneStatus)0;
        Assert.Equal(MilestoneStatus.Completed, status);
    }

    [Fact]
    public void MilestoneStatus_CanBeCompared()
    {
        var status1 = MilestoneStatus.Completed;
        var status2 = MilestoneStatus.Completed;
        Assert.Equal(status1, status2);
    }

    [Fact]
    public void MilestoneStatus_CanBeConvertedToString()
    {
        var status = MilestoneStatus.Completed;
        var stringValue = status.ToString();
        Assert.Equal("Completed", stringValue);
    }

    [Fact]
    public void MilestoneStatus_AllValuesAreDistinct()
    {
        var completed = (int)MilestoneStatus.Completed;
        var inProgress = (int)MilestoneStatus.InProgress;
        var pending = (int)MilestoneStatus.Pending;

        var values = new[] { completed, inProgress, pending };
        var uniqueValues = values.Distinct().Count();

        Assert.Equal(3, uniqueValues);
    }

    [Fact]
    public void MilestoneStatus_StringRepresentations()
    {
        Assert.Equal("Completed", MilestoneStatus.Completed.ToString());
        Assert.Equal("InProgress", MilestoneStatus.InProgress.ToString());
        Assert.Equal("Pending", MilestoneStatus.Pending.ToString());
    }
}