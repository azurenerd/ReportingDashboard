using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class TimelineDataTests
{
    [Fact]
    public void TimelineData_DefaultConstructor_TracksNotNull()
    {
        var data = new TimelineData();
        Assert.NotNull(data.Tracks);
    }

    [Fact]
    public void TimelineData_CanSetDateFields()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10"
        };

        Assert.Equal("2026-01-01", data.StartDate);
        Assert.Equal("2026-07-01", data.EndDate);
        Assert.Equal("2026-04-10", data.NowDate);
    }

    [Fact]
    public void TimelineTrack_CanSetAllProperties()
    {
        var track = new TimelineTrack
        {
            Name = "M1",
            Label = "Core Platform",
            Color = "#4285F4",
            Milestones = new List<Milestone>()
        };

        Assert.Equal("M1", track.Name);
        Assert.Equal("Core Platform", track.Label);
        Assert.Equal("#4285F4", track.Color);
        Assert.Empty(track.Milestones);
    }

    [Fact]
    public void Milestone_CanSetAllProperties()
    {
        var milestone = new Milestone
        {
            Date = "2026-02-15",
            Type = "poc",
            Label = "Feb 15"
        };

        Assert.Equal("2026-02-15", milestone.Date);
        Assert.Equal("poc", milestone.Type);
        Assert.Equal("Feb 15", milestone.Label);
    }

    [Fact]
    public void TimelineTrack_MultipleMilestones_AllAccessible()
    {
        var track = new TimelineTrack
        {
            Name = "M1",
            Label = "Test",
            Color = "#000",
            Milestones = new List<Milestone>
            {
                new() { Date = "2026-01-15", Type = "poc", Label = "PoC" },
                new() { Date = "2026-03-01", Type = "checkpoint", Label = "Check" },
                new() { Date = "2026-05-15", Type = "production", Label = "Prod" }
            }
        };

        Assert.Equal(3, track.Milestones.Count);
        Assert.Equal("poc", track.Milestones[0].Type);
        Assert.Equal("checkpoint", track.Milestones[1].Type);
        Assert.Equal("production", track.Milestones[2].Type);
    }
}