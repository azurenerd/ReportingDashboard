using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class TimelineDataDefaultsTests
{
    [Fact]
    public void TimelineData_DefaultConstruction_StartDateIsEmptyString()
    {
        var data = new TimelineData();
        data.StartDate.Should().BeEmpty();
    }

    [Fact]
    public void TimelineData_DefaultConstruction_EndDateIsEmptyString()
    {
        var data = new TimelineData();
        data.EndDate.Should().BeEmpty();
    }

    [Fact]
    public void TimelineData_DefaultConstruction_NowDateIsEmptyString()
    {
        var data = new TimelineData();
        data.NowDate.Should().BeEmpty();
    }

    [Fact]
    public void TimelineData_DefaultConstruction_TracksIsEmptyList()
    {
        var data = new TimelineData();
        data.Tracks.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void TimelineTrack_DefaultConstruction_NameIsEmptyString()
    {
        var track = new TimelineTrack();
        track.Name.Should().BeEmpty();
    }

    [Fact]
    public void TimelineTrack_DefaultConstruction_LabelIsEmptyString()
    {
        var track = new TimelineTrack();
        track.Label.Should().BeEmpty();
    }

    [Fact]
    public void TimelineTrack_DefaultConstruction_ColorHasDefaultValue()
    {
        var track = new TimelineTrack();
        track.Color.Should().Be("#999");
    }

    [Fact]
    public void TimelineTrack_DefaultConstruction_MilestonesIsEmptyList()
    {
        var track = new TimelineTrack();
        track.Milestones.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Milestone_DefaultConstruction_DateIsEmptyString()
    {
        var milestone = new Milestone();
        milestone.Date.Should().BeEmpty();
    }

    [Fact]
    public void Milestone_DefaultConstruction_LabelIsEmptyString()
    {
        var milestone = new Milestone();
        milestone.Label.Should().BeEmpty();
    }

    [Fact]
    public void Milestone_DefaultConstruction_TypeIsCheckpoint()
    {
        var milestone = new Milestone();
        milestone.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void TimelineTrack_SetProperties_ValuesAreRetained()
    {
        var track = new TimelineTrack
        {
            Name = "Chatbot",
            Label = "M1",
            Color = "#00897B",
            Milestones = new List<Milestone>
            {
                new() { Date = "2026-02-15", Label = "Feb 15", Type = "poc" },
                new() { Date = "2026-04-01", Label = "Apr 1", Type = "production" }
            }
        };

        track.Name.Should().Be("Chatbot");
        track.Label.Should().Be("M1");
        track.Color.Should().Be("#00897B");
        track.Milestones.Should().HaveCount(2);
        track.Milestones[0].Type.Should().Be("poc");
        track.Milestones[1].Type.Should().Be("production");
    }
}