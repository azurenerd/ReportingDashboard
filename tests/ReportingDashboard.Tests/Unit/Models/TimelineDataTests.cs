using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class TimelineDataTests
{
    [Fact]
    public void TimelineData_DefaultValues_ShouldBeEmpty()
    {
        var data = new TimelineData();

        data.StartDate.Should().BeEmpty();
        data.EndDate.Should().BeEmpty();
        data.NowDate.Should().BeEmpty();
        data.Tracks.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void TimelineData_SetProperties_ShouldRetainValues()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track 1", Color = "#0078D4" }
            }
        };

        data.StartDate.Should().Be("2026-01-01");
        data.EndDate.Should().Be("2026-06-30");
        data.NowDate.Should().Be("2026-04-10");
        data.Tracks.Should().HaveCount(1);
        data.Tracks[0].Id.Should().Be("M1");
    }

    [Fact]
    public void TimelineData_MultipleTracks_ShouldPreserveOrder()
    {
        var data = new TimelineData
        {
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "First" },
                new() { Id = "M2", Name = "Second" },
                new() { Id = "M3", Name = "Third" }
            }
        };

        data.Tracks.Should().HaveCount(3);
        data.Tracks[0].Id.Should().Be("M1");
        data.Tracks[1].Id.Should().Be("M2");
        data.Tracks[2].Id.Should().Be("M3");
    }

    [Fact]
    public void TimelineData_TracksListIsMutable_CanAddTracks()
    {
        var data = new TimelineData();
        data.Tracks.Add(new TimelineTrack { Id = "M1" });

        data.Tracks.Should().HaveCount(1);
    }
}

[Trait("Category", "Unit")]
public class TimelineTrackTests
{
    [Fact]
    public void TimelineTrack_DefaultValues_ShouldHaveDefaults()
    {
        var track = new TimelineTrack();

        track.Id.Should().BeEmpty();
        track.Name.Should().BeEmpty();
        track.Color.Should().Be("#0078D4");
        track.Milestones.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void TimelineTrack_SetAllProperties_ShouldRetainValues()
    {
        var track = new TimelineTrack
        {
            Id = "M2",
            Name = "Backend Services",
            Color = "#00897B",
            Milestones = new List<MilestoneMarker>
            {
                new() { Date = "2026-03-15", Label = "PoC", Type = "poc" },
                new() { Date = "2026-05-01", Label = "GA", Type = "production" }
            }
        };

        track.Id.Should().Be("M2");
        track.Name.Should().Be("Backend Services");
        track.Color.Should().Be("#00897B");
        track.Milestones.Should().HaveCount(2);
    }

    [Fact]
    public void TimelineTrack_DefaultColor_ShouldBeMicrosoftBlue()
    {
        var track = new TimelineTrack();
        track.Color.Should().Be("#0078D4");
    }

    [Fact]
    public void TimelineTrack_MilestonesAreMutable_CanAdd()
    {
        var track = new TimelineTrack();
        track.Milestones.Add(new MilestoneMarker { Label = "Test" });

        track.Milestones.Should().ContainSingle();
    }

    [Fact]
    public void TimelineTrack_EmptyMilestones_ShouldNotThrow()
    {
        var track = new TimelineTrack { Id = "M1", Name = "Empty Track" };

        track.Milestones.Should().BeEmpty();
        var action = () => track.Milestones.Count;
        action.Should().NotThrow();
    }
}

[Trait("Category", "Unit")]
public class MilestoneMarkerTests
{
    [Fact]
    public void MilestoneMarker_DefaultValues_ShouldBeCheckpoint()
    {
        var marker = new MilestoneMarker();

        marker.Date.Should().BeEmpty();
        marker.Label.Should().BeEmpty();
        marker.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void MilestoneMarker_PocType_ShouldRetainValue()
    {
        var marker = new MilestoneMarker
        {
            Date = "2026-03-26",
            Label = "PoC Complete",
            Type = "poc"
        };

        marker.Type.Should().Be("poc");
        marker.Label.Should().Be("PoC Complete");
        marker.Date.Should().Be("2026-03-26");
    }

    [Fact]
    public void MilestoneMarker_ProductionType_ShouldRetainValue()
    {
        var marker = new MilestoneMarker
        {
            Date = "2026-05-15",
            Label = "GA Release",
            Type = "production"
        };

        marker.Type.Should().Be("production");
    }

    [Fact]
    public void MilestoneMarker_CheckpointType_IsDefault()
    {
        var marker = new MilestoneMarker
        {
            Date = "2026-04-01",
            Label = "Review"
        };

        marker.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void MilestoneMarker_ArbitraryType_ShouldRetainValue()
    {
        var marker = new MilestoneMarker { Type = "custom" };
        marker.Type.Should().Be("custom");
    }
}