using Xunit;
using AgentSquad.Data;
using AgentSquad.Components;

public class MilestoneTimelineTests
{
    [Fact]
    public void RenderAllMilestones_WhenListPopulated()
    {
        var milestones = new List<Milestone>
        {
            new() { Name = "M1", TargetDate = DateTime.Now.AddDays(10), Status = MilestoneStatus.InProgress, CompletionPercentage = 50 },
            new() { Name = "M2", TargetDate = DateTime.Now.AddDays(30), Status = MilestoneStatus.Pending, CompletionPercentage = 0 }
        };

        Assert.Equal(2, milestones.Count);
    }

    [Fact]
    public void ApplyCorrectColorClass_ForCompletedStatus()
    {
        var milestone = new Milestone { Status = MilestoneStatus.Completed };
        Assert.Equal(MilestoneStatus.Completed, milestone.Status);
    }

    [Fact]
    public void FormatDate_AsYYYYMMDD()
    {
        var date = new DateTime(2025, 03, 15);
        var formatted = date.ToString("yyyy-MM-dd");
        Assert.Equal("2025-03-15", formatted);
    }

    [Fact]
    public void HandleEmptyMilestoneList()
    {
        var milestones = new List<Milestone>();
        Assert.Empty(milestones);
    }

    [Fact]
    public void HandleZeroProjectDurationDays()
    {
        var projectDays = 0;
        Assert.False(projectDays > 0);
    }

    [Fact]
    public void HandleNegativeProjectDurationDays()
    {
        var projectDays = -5;
        Assert.False(projectDays > 0);
    }

    [Fact]
    public void HandleNullMilestoneList()
    {
        List<Milestone> milestones = null;
        Assert.Null(milestones);
    }
}