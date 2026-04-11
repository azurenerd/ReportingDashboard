using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class DashboardDataTests
{
    [Fact]
    public void DashboardData_DefaultValues_ShouldBeEmpty()
    {
        var data = new DashboardData();

        data.Title.Should().BeEmpty();
        data.Subtitle.Should().BeEmpty();
        data.BacklogLink.Should().BeEmpty();
        data.CurrentMonth.Should().BeEmpty();
        data.Months.Should().NotBeNull().And.BeEmpty();
        data.Timeline.Should().NotBeNull();
        data.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void DashboardData_SetProperties_ShouldRetainValues()
    {
        var data = new DashboardData
        {
            Title = "Test Project",
            Subtitle = "Team A - Q1",
            BacklogLink = "https://dev.azure.com/backlog",
            CurrentMonth = "Mar",
            Months = new List<string> { "Jan", "Feb", "Mar" }
        };

        data.Title.Should().Be("Test Project");
        data.Subtitle.Should().Be("Team A - Q1");
        data.BacklogLink.Should().Be("https://dev.azure.com/backlog");
        data.CurrentMonth.Should().Be("Mar");
        data.Months.Should().HaveCount(3);
    }

    [Fact]
    public void DashboardData_JsonDeserialization_ShouldMapAllFields()
    {
        var json = """
        {
            "title": "My Dashboard",
            "subtitle": "Engineering - April",
            "backlogLink": "https://ado.example.com",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": []
            },
            "heatmap": {
                "shipped": {},
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Title.Should().Be("My Dashboard");
        data.Subtitle.Should().Be("Engineering - April");
        data.BacklogLink.Should().Be("https://ado.example.com");
        data.CurrentMonth.Should().Be("Apr");
        data.Months.Should().HaveCount(4);
        data.Timeline.StartDate.Should().Be("2026-01-01");
        data.Timeline.EndDate.Should().Be("2026-06-30");
        data.Timeline.NowDate.Should().Be("2026-04-10");
    }

    [Fact]
    public void DashboardData_JsonDeserialization_WithMissingOptionalFields_ShouldUseDefaults()
    {
        var json = """{ "title": "Minimal" }""";

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Title.Should().Be("Minimal");
        data.Subtitle.Should().BeEmpty();
        data.Months.Should().BeEmpty();
        data.Timeline.Should().NotBeNull();
        data.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void DashboardData_JsonDeserialization_EmptyJson_ShouldReturnDefaults()
    {
        var json = "{}";

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Title.Should().BeEmpty();
        data.Timeline.Should().NotBeNull();
        data.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void DashboardData_JsonRoundTrip_ShouldPreserveData()
    {
        var original = new DashboardData
        {
            Title = "Round Trip",
            Subtitle = "Test",
            CurrentMonth = "Feb",
            Months = new List<string> { "Jan", "Feb" }
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Title.Should().Be("Round Trip");
        deserialized.CurrentMonth.Should().Be("Feb");
        deserialized.Months.Should().BeEquivalentTo(new[] { "Jan", "Feb" });
    }
}

[Trait("Category", "Unit")]
public class TimelineDataTests
{
    [Fact]
    public void TimelineData_DefaultValues_ShouldBeEmpty()
    {
        var timeline = new TimelineData();

        timeline.StartDate.Should().BeEmpty();
        timeline.EndDate.Should().BeEmpty();
        timeline.NowDate.Should().BeEmpty();
        timeline.Tracks.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void TimelineData_JsonDeserialization_ShouldMapTracks()
    {
        var json = """
        {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "id": "M1",
                    "name": "Core Platform",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-02-15", "label": "Feb 15", "type": "poc" },
                        { "date": "2026-04-01", "label": "Apr 1", "type": "production" }
                    ]
                }
            ]
        }
        """;

        var timeline = JsonSerializer.Deserialize<TimelineData>(json);

        timeline.Should().NotBeNull();
        timeline!.Tracks.Should().HaveCount(1);
        timeline.Tracks[0].Id.Should().Be("M1");
        timeline.Tracks[0].Milestones.Should().HaveCount(2);
        timeline.Tracks[0].Milestones[0].Type.Should().Be("poc");
        timeline.Tracks[0].Milestones[1].Type.Should().Be("production");
    }
}

[Trait("Category", "Unit")]
public class TimelineTrackTests
{
    [Fact]
    public void TimelineTrack_DefaultColor_ShouldBeMicrosoftBlue()
    {
        var track = new TimelineTrack();

        track.Color.Should().Be("#0078D4");
    }

    [Fact]
    public void TimelineTrack_DefaultValues_ShouldBeEmpty()
    {
        var track = new TimelineTrack();

        track.Id.Should().BeEmpty();
        track.Name.Should().BeEmpty();
        track.Milestones.Should().NotBeNull().And.BeEmpty();
    }
}

[Trait("Category", "Unit")]
public class MilestoneMarkerTests
{
    [Fact]
    public void MilestoneMarker_DefaultType_ShouldBeCheckpoint()
    {
        var marker = new MilestoneMarker();

        marker.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void MilestoneMarker_DefaultValues_ShouldBeEmpty()
    {
        var marker = new MilestoneMarker();

        marker.Date.Should().BeEmpty();
        marker.Label.Should().BeEmpty();
    }

    [Theory]
    [InlineData("poc")]
    [InlineData("production")]
    [InlineData("checkpoint")]
    public void MilestoneMarker_Type_ShouldAcceptValidValues(string type)
    {
        var marker = new MilestoneMarker { Type = type };

        marker.Type.Should().Be(type);
    }
}

[Trait("Category", "Unit")]
public class HeatmapDataTests
{
    [Fact]
    public void HeatmapData_DefaultValues_ShouldBeEmptyDictionaries()
    {
        var heatmap = new HeatmapData();

        heatmap.Shipped.Should().NotBeNull().And.BeEmpty();
        heatmap.InProgress.Should().NotBeNull().And.BeEmpty();
        heatmap.Carryover.Should().NotBeNull().And.BeEmpty();
        heatmap.Blockers.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void HeatmapData_JsonDeserialization_ShouldMapCategories()
    {
        var json = """
        {
            "shipped": {
                "Jan": ["Feature A", "Feature B"],
                "Feb": ["Feature C"]
            },
            "inProgress": {
                "Mar": ["Task X"]
            },
            "carryover": {},
            "blockers": {
                "Apr": ["Blocker 1"]
            }
        }
        """;

        var heatmap = JsonSerializer.Deserialize<HeatmapData>(json);

        heatmap.Should().NotBeNull();
        heatmap!.Shipped.Should().HaveCount(2);
        heatmap.Shipped["Jan"].Should().HaveCount(2);
        heatmap.Shipped["Feb"].Should().ContainSingle().Which.Should().Be("Feature C");
        heatmap.InProgress["Mar"].Should().ContainSingle();
        heatmap.Carryover.Should().BeEmpty();
        heatmap.Blockers["Apr"].Should().ContainSingle().Which.Should().Be("Blocker 1");
    }
}