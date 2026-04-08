using Xunit;
using AgentSquad.Services.Models;

namespace AgentSquad.Tests.Services.Models;

public class MilestoneTests
{
    [Fact]
    public void Milestone_HasIdProperty()
    {
        var milestone = new Milestone { Id = "m1" };
        Assert.Equal("m1", milestone.Id);
    }

    [Fact]
    public void Milestone_HasNameProperty()
    {
        var milestone = new Milestone { Name = "Phase 1" };
        Assert.Equal("Phase 1", milestone.Name);
    }

    [Fact]
    public void Milestone_HasTargetDateProperty()
    {
        var date = new DateTime(2026, 04, 15);
        var milestone = new Milestone { TargetDate = date };
        Assert.Equal(date, milestone.TargetDate);
    }

    [Fact]
    public void Milestone_HasActualDateProperty()
    {
        var date = new DateTime(2026, 04, 14);
        var milestone = new Milestone { ActualDate = date };
        Assert.Equal(date, milestone.ActualDate);
    }

    [Fact]
    public void Milestone_ActualDateCanBeNull()
    {
        var milestone = new Milestone { ActualDate = null };
        Assert.Null(milestone.ActualDate);
    }

    [Fact]
    public void Milestone_HasStatusProperty()
    {
        var milestone = new Milestone { Status = MilestoneStatus.Completed };
        Assert.Equal(MilestoneStatus.Completed, milestone.Status);
    }

    [Fact]
    public void Milestone_HasCompletionPercentageProperty()
    {
        var milestone = new Milestone { CompletionPercentage = 75 };
        Assert.Equal(75, milestone.CompletionPercentage);
    }

    [Fact]
    public void Milestone_DefaultStatusIsPending()
    {
        var milestone = new Milestone();
        Assert.Equal(MilestoneStatus.Pending, milestone.Status);
    }

    [Fact]
    public void Milestone_DefaultCompletionPercentageIsZero()
    {
        var milestone = new Milestone();
        Assert.Equal(0, milestone.CompletionPercentage);
    }

    [Fact]
    public void Milestone_DefaultNameIsEmpty()
    {
        var milestone = new Milestone();
        Assert.Equal(string.Empty, milestone.Name);
    }

    [Fact]
    public void Milestone_DefaultIdIsEmpty()
    {
        var milestone = new Milestone();
        Assert.Equal(string.Empty, milestone.Id);
    }

    [Fact]
    public void Milestone_CanBeInitializedWithAllProperties()
    {
        var milestone = new Milestone
        {
            Id = "m1",
            Name = "Phase 1",
            TargetDate = new DateTime(2026, 04, 15),
            ActualDate = new DateTime(2026, 04, 14),
            Status = MilestoneStatus.Completed,
            CompletionPercentage = 100
        };

        Assert.Equal("m1", milestone.Id);
        Assert.Equal("Phase 1", milestone.Name);
        Assert.Equal(new DateTime(2026, 04, 15), milestone.TargetDate);
        Assert.Equal(new DateTime(2026, 04, 14), milestone.ActualDate);
        Assert.Equal(MilestoneStatus.Completed, milestone.Status);
        Assert.Equal(100, milestone.CompletionPercentage);
    }

    [Fact]
    public void Milestone_CompletionPercentageCanBeZero()
    {
        var milestone = new Milestone { CompletionPercentage = 0 };
        Assert.Equal(0, milestone.CompletionPercentage);
    }

    [Fact]
    public void Milestone_CompletionPercentageCanBe100()
    {
        var milestone = new Milestone { CompletionPercentage = 100 };
        Assert.Equal(100, milestone.CompletionPercentage);
    }

    [Fact]
    public void Milestone_CompletionPercentageCanBeAnyValidValue()
    {
        var milestone = new Milestone { CompletionPercentage = 50 };
        Assert.Equal(50, milestone.CompletionPercentage);
    }

    [Fact]
    public void Milestone_TargetDateCanBeInPast()
    {
        var pastDate = DateTime.Now.AddDays(-30);
        var milestone = new Milestone { TargetDate = pastDate };
        Assert.True(milestone.TargetDate < DateTime.Now);
    }

    [Fact]
    public void Milestone_TargetDateCanBeInFuture()
    {
        var futureDate = DateTime.Now.AddDays(30);
        var milestone = new Milestone { TargetDate = futureDate };
        Assert.True(milestone.TargetDate > DateTime.Now);
    }

    [Fact]
    public void Milestone_NameCanBeEmpty()
    {
        var milestone = new Milestone { Name = string.Empty };
        Assert.Equal(string.Empty, milestone.Name);
    }

    [Fact]
    public void Milestone_NameCanBeVeryLong()
    {
        var longName = new string('a', 1000);
        var milestone = new Milestone { Name = longName };
        Assert.Equal(longName, milestone.Name);
    }

    [Fact]
    public void Milestone_IdCanBeGuid()
    {
        var guid = Guid.NewGuid().ToString();
        var milestone = new Milestone { Id = guid };
        Assert.Equal(guid, milestone.Id);
    }

    [Fact]
    public void Milestone_StatusCanBeAllEnumValues()
    {
        var completed = new Milestone { Status = MilestoneStatus.Completed };
        var inProgress = new Milestone { Status = MilestoneStatus.InProgress };
        var pending = new Milestone { Status = MilestoneStatus.Pending };

        Assert.Equal(MilestoneStatus.Completed, completed.Status);
        Assert.Equal(MilestoneStatus.InProgress, inProgress.Status);
        Assert.Equal(MilestoneStatus.Pending, pending.Status);
    }

    [Fact]
    public void Milestone_ActualDateCanBeSameAsTargetDate()
    {
        var date = new DateTime(2026, 04, 15);
        var milestone = new Milestone
        {
            TargetDate = date,
            ActualDate = date
        };

        Assert.Equal(milestone.TargetDate, milestone.ActualDate);
    }

    [Fact]
    public void Milestone_ActualDateCanBeBeforeTargetDate()
    {
        var targetDate = new DateTime(2026, 04, 15);
        var actualDate = new DateTime(2026, 04, 14);
        var milestone = new Milestone
        {
            TargetDate = targetDate,
            ActualDate = actualDate
        };

        Assert.True(milestone.ActualDate < milestone.TargetDate);
    }

    [Fact]
    public void Milestone_ActualDateCanBeAfterTargetDate()
    {
        var targetDate = new DateTime(2026, 04, 15);
        var actualDate = new DateTime(2026, 04, 16);
        var milestone = new Milestone
        {
            TargetDate = targetDate,
            ActualDate = actualDate
        };

        Assert.True(milestone.ActualDate > milestone.TargetDate);
    }
}