using System.Text.Json;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

/// <summary>
/// Unit tests for TimelineData, TimelineTrack, and Milestone model classes.
/// </summary>
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
    public void TimelineData_Defaults_StringsEmpty()
    {
        var tl = new TimelineData();

        Assert.Equal(string.Empty, tl.StartDate);
        Assert.Equal(string.Empty, tl.EndDate);
        Assert.Equal(string.Empty, tl.NowDate);
    }

    [Fact]
    public void TimelineData_Defaults_TracksInitialized()
    {
        var tl = new TimelineData();
        Assert.NotNull(tl.Tracks);
        Assert.Empty(tl.Tracks);
    }

    [Fact]
    public void TimelineData_CanSetDates()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-12-31",
            NowDate = "2026-06-15"
        };

        Assert.Equal("2026-01-01", tl.StartDate);
        Assert.Equal("2026-12-31", tl.EndDate);
        Assert.Equal("2026-06-15", tl.NowDate);
    }

    [Fact]
    public void TimelineData_JsonRoundTrip()
    {
        var original = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Core", Color = "#4285F4", Milestones = new() }
            }
        };

        var json = JsonSerializer.Serialize(original, JsonOpts);
        var restored = JsonSerializer.Deserialize<TimelineData>(json, JsonOpts);

        Assert.NotNull(restored);
        Assert.Equal("2026-01-01", restored!.StartDate);
        Assert.Equal("2026-07-01", restored.EndDate);
        Assert.Equal("2026-04-10", restored.NowDate);
        Assert.Single(restored.Tracks);
    }

    #endregion

    #region TimelineTrack

    [Fact]
    public void TimelineTrack_Defaults()
    {
        var track = new TimelineTrack();

        Assert.Equal(string.Empty, track.Name);
        Assert.Equal(string.Empty, track.Label);
        Assert.Equal("#999", track.Color);
        Assert.NotNull(track.Milestones);
        Assert.Empty(track.Milestones);
    }

    [Fact]
    public void TimelineTrack_CanSetProperties()
    {
        var track = new TimelineTrack
        {
            Name = "M1",
            Label = "Core Platform",
            Color = "#4285F4"
        };

        Assert.Equal("M1", track.Name);
        Assert.Equal("Core Platform", track.Label);
        Assert.Equal("#4285F4", track.Color);
    }

    [Fact]
    public void TimelineTrack_WithMilestones_RoundTrip()
    {
        var track = new TimelineTrack
        {
            Name = "M1",
            Label = "Core",
            Color = "#000",
            Milestones = new List<Milestone>
            {
                new() { Date = "2026-02-15", Type = "poc", Label = "PoC" },
                new() { Date = "2026-05-01", Type = "production", Label = "GA" }
            }
        };

        var json = JsonSerializer.Serialize(track, JsonOpts);
        var restored = JsonSerializer.Deserialize<TimelineTrack>(json, JsonOpts);

        Assert.Equal(2, restored!.Milestones.Count);
        Assert.Equal("poc", restored.Milestones[0].Type);
        Assert.Equal("production", restored.Milestones[1].Type);
    }

    #endregion

    #region Milestone

    [Fact]
    public void Milestone_Defaults()
    {
        var ms = new Milestone();

        Assert.Equal(string.Empty, ms.Date);
        Assert.Equal("checkpoint", ms.Type);
        Assert.Equal(string.Empty, ms.Label);
    }

    [Fact]
    public void Milestone_CanSetPocType()
    {
        var ms = new Milestone { Type = "poc" };
        Assert.Equal("poc", ms.Type);
    }

    [Fact]
    public void Milestone_CanSetProductionType()
    {
        var ms = new Milestone { Type = "production" };
        Assert.Equal("production", ms.Type);
    }

    [Fact]
    public void Milestone_CanSetCheckpointType()
    {
        var ms = new Milestone { Type = "checkpoint" };
        Assert.Equal("checkpoint", ms.Type);
    }

    [Fact]
    public void Milestone_JsonRoundTrip_AllTypes()
    {
        var types = new[] { "poc", "production", "checkpoint" };
        foreach (var type in types)
        {
            var ms = new Milestone { Date = "2026-03-01", Type = type, Label = $"Label for {type}" };
            var json = JsonSerializer.Serialize(ms, JsonOpts);
            var restored = JsonSerializer.Deserialize<Milestone>(json, JsonOpts);

            Assert.Equal(type, restored!.Type);
            Assert.Equal($"Label for {type}", restored.Label);
        }
    }

    [Fact]
    public void Milestone_SpecialCharsInLabel_Preserved()
    {
        var ms = new Milestone { Label = "Release <v2.0> & \"Final\"" };
        var json = JsonSerializer.Serialize(ms, JsonOpts);
        var restored = JsonSerializer.Deserialize<Milestone>(json, JsonOpts);

        Assert.Equal("Release <v2.0> & \"Final\"", restored!.Label);
    }

    #endregion
}