using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

public class DashboardDataModelTests
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void DashboardData_DefaultValues_AreCorrect()
    {
        var data = new DashboardData();

        data.Title.Should().Be(string.Empty);
        data.Subtitle.Should().Be(string.Empty);
        data.BacklogLink.Should().Be(string.Empty);
        data.CurrentMonth.Should().Be(string.Empty);
        data.Months.Should().NotBeNull().And.BeEmpty();
        data.Timeline.Should().NotBeNull();
        data.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void TimelineData_DefaultValues_AreCorrect()
    {
        var tl = new TimelineData();

        tl.StartDate.Should().Be(string.Empty);
        tl.EndDate.Should().Be(string.Empty);
        tl.NowDate.Should().Be(string.Empty);
        tl.Tracks.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void TimelineTrack_DefaultValues_AreCorrect()
    {
        var track = new TimelineTrack();

        track.Name.Should().Be(string.Empty);
        track.Label.Should().Be(string.Empty);
        track.Color.Should().Be("#999");
        track.Milestones.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Milestone_DefaultValues_AreCorrect()
    {
        var milestone = new Milestone();

        milestone.Date.Should().Be(string.Empty);
        milestone.Type.Should().Be("checkpoint");
        milestone.Label.Should().Be(string.Empty);
    }

    [Fact]
    public void HeatmapData_DefaultValues_AreCorrect()
    {
        var hm = new HeatmapData();

        hm.Shipped.Should().NotBeNull().And.BeEmpty();
        hm.InProgress.Should().NotBeNull().And.BeEmpty();
        hm.Carryover.Should().NotBeNull().And.BeEmpty();
        hm.Blockers.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void DashboardData_JsonRoundTrip_PreservesAllFields()
    {
        var original = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            BacklogLink = "https://link",
            CurrentMonth = "April",
            Months = new List<string> { "January", "February", "March", "April" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-07-01",
                NowDate = "2026-04-10",
                Tracks = new List<TimelineTrack>
                {
                    new()
                    {
                        Name = "M1",
                        Label = "Core",
                        Color = "#4285F4",
                        Milestones = new List<Milestone>
                        {
                            new() { Date = "2026-02-15", Type = "poc", Label = "Feb 15" }
                        }
                    }
                }
            },
            Heatmap = new HeatmapData
            {
                Shipped = new Dictionary<string, List<string>> { ["jan"] = new() { "A" } }
            }
        };

        var json = JsonSerializer.Serialize(original, CamelCaseOptions);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json, CamelCaseOptions);

        deserialized.Should().NotBeNull();
        deserialized!.Title.Should().Be("Test");
        deserialized.Subtitle.Should().Be("Sub");
        deserialized.Months.Should().HaveCount(4);
        deserialized.Timeline.Tracks.Should().HaveCount(1);
        deserialized.Timeline.Tracks[0].Milestones.Should().HaveCount(1);
        deserialized.Heatmap.Shipped.Should().ContainKey("jan");
    }

    [Fact]
    public void Serialization_ProducesCamelCaseKeys()
    {
        var data = new DashboardData
        {
            Title = "Test",
            CurrentMonth = "April"
        };

        var json = JsonSerializer.Serialize(data, CamelCaseOptions);

        json.Should().Contain("\"title\"");
        json.Should().Contain("\"currentMonth\"");
        json.Should().Contain("\"timeline\"");
        json.Should().Contain("\"heatmap\"");
    }

    [Fact]
    public void Milestone_JsonRoundTrip_PreservesFields()
    {
        var original = new Milestone { Date = "2026-06-15", Type = "production", Label = "GA" };

        var json = JsonSerializer.Serialize(original, CamelCaseOptions);
        var deserialized = JsonSerializer.Deserialize<Milestone>(json, CamelCaseOptions);

        deserialized.Should().NotBeNull();
        deserialized!.Date.Should().Be("2026-06-15");
        deserialized.Type.Should().Be("production");
        deserialized.Label.Should().Be("GA");
    }
}