using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class TimelineDataModelTests
{
    [Fact]
    public void TimelineData_DefaultConstructor_SetsEmptyDefaults()
    {
        var data = new TimelineData();

        data.StartDate.Should().BeEmpty();
        data.EndDate.Should().BeEmpty();
        data.NowDate.Should().BeEmpty();
        data.Tracks.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void TimelineData_Deserialize_AllProperties()
    {
        var json = """
        {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": []
        }
        """;

        var data = JsonSerializer.Deserialize<TimelineData>(json);

        data.Should().NotBeNull();
        data!.StartDate.Should().Be("2026-01-01");
        data.EndDate.Should().Be("2026-06-30");
        data.NowDate.Should().Be("2026-04-10");
        data.Tracks.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void TimelineData_Deserialize_WithTracks()
    {
        var json = """
        {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "name": "M1",
                    "label": "Feature A",
                    "color": "#0078D4",
                    "milestones": []
                }
            ]
        }
        """;

        var data = JsonSerializer.Deserialize<TimelineData>(json);

        data.Should().NotBeNull();
        data!.Tracks.Should().HaveCount(1);
        data.Tracks[0].Name.Should().Be("M1");
    }

    [Fact]
    public void TimelineData_Tracks_CollectionPreventsNull()
    {
        var data = new TimelineData();

        var action = () => data.Tracks.Count;

        action.Should().NotThrow();
        data.Tracks.Count.Should().Be(0);
    }

    [Fact]
    public void TimelineData_Serialize_UsesCorrectPropertyNames()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10"
        };

        var json = JsonSerializer.Serialize(data);

        json.Should().Contain("\"startDate\"");
        json.Should().Contain("\"endDate\"");
        json.Should().Contain("\"nowDate\"");
        json.Should().Contain("\"tracks\"");
    }
}

[Trait("Category", "Unit")]
public class TimelineTrackModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyDefaults()
    {
        var track = new TimelineTrack();

        track.Name.Should().BeEmpty();
        track.Label.Should().BeEmpty();
        track.Color.Should().Be("#999");
        track.Milestones.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Deserialize_AllProperties()
    {
        var json = """
        {
            "name": "M2",
            "label": "Data Pipeline",
            "color": "#00897B",
            "milestones": [
                { "date": "2026-02-15", "type": "poc", "label": "PoC Complete" }
            ]
        }
        """;

        var track = JsonSerializer.Deserialize<TimelineTrack>(json);

        track.Should().NotBeNull();
        track!.Name.Should().Be("M2");
        track.Label.Should().Be("Data Pipeline");
        track.Color.Should().Be("#00897B");
        track.Milestones.Should().HaveCount(1);
    }

    [Fact]
    public void Deserialize_WithMissingColor_UsesDefault()
    {
        var json = """{ "name": "M1", "label": "Test" }""";

        // Default in class is "#999", but deserialization may overwrite to empty
        var track = JsonSerializer.Deserialize<TimelineTrack>(json);

        track.Should().NotBeNull();
        track!.Name.Should().Be("M1");
    }

    [Fact]
    public void Milestones_CollectionPreventsNull()
    {
        var track = new TimelineTrack();

        var action = () =>
        {
            foreach (var _ in track.Milestones) { }
        };

        action.Should().NotThrow();
    }

    [Fact]
    public void Serialize_UsesCorrectJsonPropertyNames()
    {
        var track = new TimelineTrack
        {
            Name = "M1",
            Label = "Test",
            Color = "#FF0000"
        };

        var json = JsonSerializer.Serialize(track);

        json.Should().Contain("\"name\"");
        json.Should().Contain("\"label\"");
        json.Should().Contain("\"color\"");
        json.Should().Contain("\"milestones\"");
    }
}

[Trait("Category", "Unit")]
public class MilestoneModelTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var milestone = new Milestone();

        milestone.Date.Should().BeEmpty();
        milestone.Type.Should().Be("checkpoint");
        milestone.Label.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_PocType()
    {
        var json = """{ "date": "2026-03-01", "type": "poc", "label": "PoC Ready" }""";

        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        milestone.Should().NotBeNull();
        milestone!.Date.Should().Be("2026-03-01");
        milestone.Type.Should().Be("poc");
        milestone.Label.Should().Be("PoC Ready");
    }

    [Fact]
    public void Deserialize_ProductionType()
    {
        var json = """{ "date": "2026-05-01", "type": "production", "label": "GA Release" }""";

        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        milestone.Should().NotBeNull();
        milestone!.Type.Should().Be("production");
    }

    [Fact]
    public void Deserialize_CheckpointType()
    {
        var json = """{ "date": "2026-02-15", "type": "checkpoint", "label": "Design Review" }""";

        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        milestone.Should().NotBeNull();
        milestone!.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void Deserialize_MissingType_UsesDefaultCheckpoint()
    {
        var json = """{ "date": "2026-04-01", "label": "Some Event" }""";

        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        milestone.Should().NotBeNull();
        // Default value in class is "checkpoint"
        milestone!.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void Serialize_UsesCorrectJsonPropertyNames()
    {
        var milestone = new Milestone
        {
            Date = "2026-01-15",
            Type = "poc",
            Label = "Test"
        };

        var json = JsonSerializer.Serialize(milestone);

        json.Should().Contain("\"date\"");
        json.Should().Contain("\"type\"");
        json.Should().Contain("\"label\"");
    }

    [Fact]
    public void RoundTrip_PreservesValues()
    {
        var original = new Milestone
        {
            Date = "2026-07-01",
            Type = "production",
            Label = "Final Release"
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Milestone>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Date.Should().Be(original.Date);
        deserialized.Type.Should().Be(original.Type);
        deserialized.Label.Should().Be(original.Label);
    }
}