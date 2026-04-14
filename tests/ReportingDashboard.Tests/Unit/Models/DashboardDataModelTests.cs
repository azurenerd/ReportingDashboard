using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

/// <summary>
/// Tests the actual DashboardData, TimelineData, HeatmapData models
/// from the PR #1171 source code (JsonPropertyName attributes, defaults, round-trips).
/// </summary>
public class DashboardDataModelTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void DashboardData_DefaultCollections_NotNull()
    {
        var data = new DashboardData();

        data.Title.Should().Be("");
        data.Subtitle.Should().Be("");
        data.BacklogLink.Should().Be("");
        data.CurrentMonth.Should().Be("");
        data.Months.Should().NotBeNull().And.BeEmpty();
        data.Timeline.Should().NotBeNull();
        data.Timeline.Tracks.Should().NotBeNull().And.BeEmpty();
        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Shipped.Should().NotBeNull().And.BeEmpty();
        data.Heatmap.InProgress.Should().NotBeNull().And.BeEmpty();
        data.Heatmap.Carryover.Should().NotBeNull().And.BeEmpty();
        data.Heatmap.Blockers.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DashboardData_JsonRoundTrip_PopulatesAllFields()
    {
        var json = """
        {
            "title": "Q2 Dashboard",
            "subtitle": "Engineering · Core Platform · April 2026",
            "backlogLink": "https://dev.azure.com/project",
            "currentMonth": "Apr",
            "months": ["Jan","Feb","Mar","Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "name": "M1",
                        "label": "Auth Rewrite",
                        "color": "#4285F4",
                        "milestones": [
                            { "date": "2026-02-15", "type": "poc", "label": "PoC" },
                            { "date": "2026-05-01", "type": "production", "label": "GA" }
                        ]
                    }
                ]
            },
            "heatmap": {
                "shipped": { "Jan": ["Feature A"], "Feb": ["Feature B"] },
                "inProgress": { "Mar": ["Feature C"] },
                "carryover": {},
                "blockers": { "Apr": ["Blocker 1"] }
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Title.Should().Be("Q2 Dashboard");
        data.Subtitle.Should().Contain("Core Platform");
        data.BacklogLink.Should().Be("https://dev.azure.com/project");
        data.CurrentMonth.Should().Be("Apr");
        data.Months.Should().HaveCount(4);
        data.Timeline.StartDate.Should().Be("2026-01-01");
        data.Timeline.NowDate.Should().Be("2026-04-10");
        data.Timeline.Tracks.Should().HaveCount(1);
        data.Timeline.Tracks[0].Name.Should().Be("M1");
        data.Timeline.Tracks[0].Color.Should().Be("#4285F4");
        data.Timeline.Tracks[0].Milestones.Should().HaveCount(2);
        data.Timeline.Tracks[0].Milestones[0].Type.Should().Be("poc");
        data.Heatmap.Shipped.Should().HaveCount(2);
        data.Heatmap.Blockers["Apr"].Should().Contain("Blocker 1");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TimelineTrack_DefaultColor_IsGray()
    {
        var track = new TimelineTrack();

        track.Color.Should().Be("#999");
        track.Name.Should().Be("");
        track.Label.Should().Be("");
        track.Milestones.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Milestone_DefaultType_IsCheckpoint()
    {
        var milestone = new Milestone();

        milestone.Type.Should().Be("checkpoint");
        milestone.Date.Should().Be("");
        milestone.Label.Should().Be("");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DashboardData_Deserialize_PartialJson_DefaultsApplied()
    {
        var json = """{ "title": "Only Title" }""";

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Title.Should().Be("Only Title");
        data.Subtitle.Should().Be("");
        data.Months.Should().NotBeNull().And.BeEmpty();
        data.Timeline.Should().NotBeNull();
        data.Timeline.Tracks.Should().BeEmpty();
        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Shipped.Should().BeEmpty();
    }
}