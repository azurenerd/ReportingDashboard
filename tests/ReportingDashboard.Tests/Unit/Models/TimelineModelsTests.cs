using System.Text.Json;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class TimelineModelsTests
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    #region TimelineData

    [Fact]
    public void TimelineData_DefaultStartDate_IsEmpty()
    {
        var tl = new TimelineData();
        Assert.Equal(string.Empty, tl.StartDate);
    }

    [Fact]
    public void TimelineData_DefaultEndDate_IsEmpty()
    {
        var tl = new TimelineData();
        Assert.Equal(string.Empty, tl.EndDate);
    }

    [Fact]
    public void TimelineData_DefaultNowDate_IsEmpty()
    {
        var tl = new TimelineData();
        Assert.Equal(string.Empty, tl.NowDate);
    }

    [Fact]
    public void TimelineData_DefaultTracks_IsEmptyList()
    {
        var tl = new TimelineData();
        Assert.NotNull(tl.Tracks);
        Assert.Empty(tl.Tracks);
    }

    [Fact]
    public void TimelineData_JsonDeserialization_MapsAllFields()
    {
        var json = """
        {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": []
        }
        """;
        var tl = JsonSerializer.Deserialize<TimelineData>(json, JsonOpts);

        Assert.NotNull(tl);
        Assert.Equal("2026-01-01", tl!.StartDate);
        Assert.Equal("2026-06-30", tl.EndDate);
        Assert.Equal("2026-04-10", tl.NowDate);
        Assert.Empty(tl.Tracks);
    }

    [Fact]
    public void TimelineData_JsonDeserialization_WithTracks()
    {
        var json = """
        {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                { "name": "M1", "label": "Core", "color": "#0078D4", "milestones": [] },
                { "name": "M2", "label": "API", "color": "#00897B", "milestones": [] }
            ]
        }
        """;
        var tl = JsonSerializer.Deserialize<TimelineData>(json, JsonOpts);

        Assert.Equal(2, tl!.Tracks.Count);
        Assert.Equal("M1", tl.Tracks[0].Name);
        Assert.Equal("M2", tl.Tracks[1].Name);
    }

    #endregion

    #region TimelineTrack

    [Fact]
    public void TimelineTrack_DefaultColor_Is999()
    {
        var track = new TimelineTrack();
        Assert.Equal("#999", track.Color);
    }

    [Fact]
    public void TimelineTrack_DefaultMilestones_IsEmptyList()
    {
        var track = new TimelineTrack();
        Assert.NotNull(track.Milestones);
        Assert.Empty(track.Milestones);
    }

    [Fact]
    public void TimelineTrack_JsonDeserialization_MapsAllFields()
    {
        var json = """
        {
            "name": "M1",
            "label": "Core Platform",
            "color": "#4285F4",
            "milestones": [
                { "date": "2026-02-15", "type": "poc", "label": "PoC Complete" }
            ]
        }
        """;
        var track = JsonSerializer.Deserialize<TimelineTrack>(json, JsonOpts);

        Assert.NotNull(track);
        Assert.Equal("M1", track!.Name);
        Assert.Equal("Core Platform", track.Label);
        Assert.Equal("#4285F4", track.Color);
        Assert.Single(track.Milestones);
    }

    [Fact]
    public void TimelineTrack_JsonDeserialization_MissingColor_UsesDefault()
    {
        var json = """{ "name": "M1", "label": "Test", "milestones": [] }""";
        var track = JsonSerializer.Deserialize<TimelineTrack>(json, JsonOpts);

        Assert.Equal("#999", track!.Color);
    }

    [Fact]
    public void TimelineTrack_WithMultipleMilestones_AllDeserialized()
    {
        var json = """
        {
            "name": "M1",
            "label": "Core",
            "color": "#000",
            "milestones": [
                { "date": "2026-01-15", "type": "poc", "label": "PoC" },
                { "date": "2026-03-01", "type": "checkpoint", "label": "Check" },
                { "date": "2026-05-01", "type": "production", "label": "GA" },
                { "date": "2026-06-15", "type": "checkpoint", "label": "Review" }
            ]
        }
        """;
        var track = JsonSerializer.Deserialize<TimelineTrack>(json, JsonOpts);

        Assert.Equal(4, track!.Milestones.Count);
        Assert.Equal("poc", track.Milestones[0].Type);
        Assert.Equal("checkpoint", track.Milestones[1].Type);
        Assert.Equal("production", track.Milestones[2].Type);
    }

    #endregion

    #region Milestone

    [Fact]
    public void Milestone_DefaultType_IsCheckpoint()
    {
        var ms = new Milestone();
        Assert.Equal("checkpoint", ms.Type);
    }

    [Fact]
    public void Milestone_DefaultDate_IsEmpty()
    {
        var ms = new Milestone();
        Assert.Equal(string.Empty, ms.Date);
    }

    [Fact]
    public void Milestone_DefaultLabel_IsEmpty()
    {
        var ms = new Milestone();
        Assert.Equal(string.Empty, ms.Label);
    }

    [Fact]
    public void Milestone_JsonDeserialization_PocType()
    {
        var json = """{ "date": "2026-02-15", "type": "poc", "label": "PoC Complete" }""";
        var ms = JsonSerializer.Deserialize<Milestone>(json, JsonOpts);

        Assert.Equal("poc", ms!.Type);
        Assert.Equal("2026-02-15", ms.Date);
        Assert.Equal("PoC Complete", ms.Label);
    }

    [Fact]
    public void Milestone_JsonDeserialization_ProductionType()
    {
        var json = """{ "date": "2026-05-01", "type": "production", "label": "GA Release" }""";
        var ms = JsonSerializer.Deserialize<Milestone>(json, JsonOpts);

        Assert.Equal("production", ms!.Type);
    }

    [Fact]
    public void Milestone_JsonDeserialization_MissingType_UsesDefault()
    {
        var json = """{ "date": "2026-03-01", "label": "Check" }""";
        var ms = JsonSerializer.Deserialize<Milestone>(json, JsonOpts);

        Assert.Equal("checkpoint", ms!.Type);
    }

    [Fact]
    public void Milestone_JsonRoundTrip_PreservesAllFields()
    {
        var original = new Milestone { Date = "2026-06-15", Type = "production", Label = "GA" };
        var json = JsonSerializer.Serialize(original, JsonOpts);
        var deserialized = JsonSerializer.Deserialize<Milestone>(json, JsonOpts);

        Assert.Equal(original.Date, deserialized!.Date);
        Assert.Equal(original.Type, deserialized.Type);
        Assert.Equal(original.Label, deserialized.Label);
    }

    #endregion

    #region JSON property names

    [Fact]
    public void TimelineData_Serialization_UsesCamelCasePropertyNames()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10"
        };
        var json = JsonSerializer.Serialize(tl, JsonOpts);

        Assert.Contains("\"startDate\"", json);
        Assert.Contains("\"endDate\"", json);
        Assert.Contains("\"nowDate\"", json);
        Assert.Contains("\"tracks\"", json);
    }

    [Fact]
    public void TimelineTrack_Serialization_UsesCamelCasePropertyNames()
    {
        var track = new TimelineTrack { Name = "M1", Label = "Core", Color = "#000" };
        var json = JsonSerializer.Serialize(track, JsonOpts);

        Assert.Contains("\"name\"", json);
        Assert.Contains("\"label\"", json);
        Assert.Contains("\"color\"", json);
        Assert.Contains("\"milestones\"", json);
    }

    [Fact]
    public void Milestone_Serialization_UsesCamelCasePropertyNames()
    {
        var ms = new Milestone { Date = "2026-01-01", Type = "poc", Label = "PoC" };
        var json = JsonSerializer.Serialize(ms, JsonOpts);

        Assert.Contains("\"date\"", json);
        Assert.Contains("\"type\"", json);
        Assert.Contains("\"label\"", json);
    }

    #endregion
}