using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void DashboardData_DeserializesFullJson_AllFieldsPopulated()
    {
        var json = """
        {
            "title": "Test Roadmap",
            "subtitle": "Team A - April 2026",
            "backlogUrl": "https://dev.azure.com/test",
            "timelineMonths": ["Jan", "Feb", "Mar"],
            "currentMonth": "Feb",
            "milestones": [
                {
                    "id": "M1",
                    "label": "Milestone One",
                    "color": "#0078D4",
                    "events": [
                        { "date": "2026-01-15", "type": "checkpoint", "label": "Design Review" }
                    ]
                }
            ],
            "heatmap": {
                "columns": ["Jan", "Feb"],
                "rows": [
                    {
                        "category": "shipped",
                        "items": { "Jan": ["Item A"], "Feb": ["Item B", "Item C"] }
                    }
                ]
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        data.Should().NotBeNull();
        data!.Title.Should().Be("Test Roadmap");
        data.Subtitle.Should().Be("Team A - April 2026");
        data.BacklogUrl.Should().Be("https://dev.azure.com/test");
        data.TimelineMonths.Should().HaveCount(3);
        data.CurrentMonth.Should().Be("Feb");
        data.Milestones.Should().HaveCount(1);
        data.Milestones![0].Id.Should().Be("M1");
        data.Milestones[0].Events.Should().HaveCount(1);
        data.Milestones[0].Events[0].Date.Should().Be(new DateOnly(2026, 1, 15));
        data.Heatmap.Should().NotBeNull();
        data.Heatmap!.Columns.Should().HaveCount(2);
        data.Heatmap.Rows.Should().HaveCount(1);
        data.Heatmap.Rows[0].Items["Feb"].Should().HaveCount(2);
    }

    [Fact]
    public void DashboardData_DeserializesEmptyJson_AllFieldsNull()
    {
        var json = "{}";
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        data.Should().NotBeNull();
        data!.Title.Should().BeNull();
        data.Subtitle.Should().BeNull();
        data.BacklogUrl.Should().BeNull();
        data.TimelineMonths.Should().BeNull();
        data.CurrentMonth.Should().BeNull();
        data.Milestones.Should().BeNull();
        data.Heatmap.Should().BeNull();
    }

    [Fact]
    public void MilestoneTrack_DefaultValues_AreCorrect()
    {
        var track = new MilestoneTrack();

        track.Id.Should().Be("");
        track.Label.Should().Be("");
        track.Color.Should().Be("#000");
        track.Events.Should().BeEmpty();
    }

    [Fact]
    public void HeatmapData_DefaultConstructor_CreatesEmptyArrays()
    {
        var heatmap = new HeatmapData();

        heatmap.Columns.Should().BeEmpty();
        heatmap.Rows.Should().BeEmpty();
    }

    [Fact]
    public void MilestoneEvent_DefaultValues_AreCorrect()
    {
        var evt = new MilestoneEvent();

        evt.Type.Should().Be("checkpoint");
        evt.Label.Should().Be("");
        evt.Date.Should().Be(default(DateOnly));
    }
}