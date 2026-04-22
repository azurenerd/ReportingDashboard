using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = false,
        AllowTrailingCommas = true
    };

    [Fact]
    public void FullDeserialization_AllFieldsPopulated()
    {
        var json = """
        {
            "project": {
                "title": "Agent Squad",
                "subtitle": "Team · April 2026",
                "backlogUrl": "https://dev.azure.com/backlog",
                "currentDate": "2026-04-15"
            },
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-31",
                "tracks": [
                    {
                        "id": "M1",
                        "name": "Chatbot",
                        "color": "#0078D4",
                        "milestones": [
                            { "date": "2026-03-01", "label": "v1", "type": "poc" }
                        ]
                    }
                ]
            },
            "heatmap": {
                "months": ["January", "February"],
                "highlightMonth": "February",
                "rows": [
                    {
                        "category": "Shipped",
                        "items": { "January": ["Item A"] }
                    }
                ]
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data.Should().NotBeNull();
        data!.Project.Title.Should().Be("Agent Squad");
        data.Project.BacklogUrl.Should().Be("https://dev.azure.com/backlog");
        data.Project.CurrentDate.Should().Be("2026-04-15");
        data.Timeline.Tracks.Should().HaveCount(1);
        data.Timeline.Tracks[0].Milestones.Should().HaveCount(1);
        data.Timeline.Tracks[0].Milestones[0].Type.Should().Be("poc");
        data.Heatmap.Months.Should().HaveCount(2);
        data.Heatmap.HighlightMonth.Should().Be("February");
        data.Heatmap.Rows[0].Items["January"].Should().Contain("Item A");
    }

    [Fact]
    public void OptionalBacklogUrl_IsNullWhenMissing()
    {
        var json = """
        {
            "project": { "title": "Test", "subtitle": "Sub", "currentDate": "2026-01-01" },
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-06-01", "tracks": [] },
            "heatmap": { "months": [], "highlightMonth": "", "rows": [] }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data.Should().NotBeNull();
        data!.Project.BacklogUrl.Should().BeNull();
    }

    [Fact]
    public void EmptyMilestoneList_DeserializesCorrectly()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "currentDate": "2026-01-01" },
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-01",
                "tracks": [{ "id": "M1", "name": "Track", "color": "#000", "milestones": [] }]
            },
            "heatmap": { "months": [], "highlightMonth": "", "rows": [] }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data!.Timeline.Tracks.Should().HaveCount(1);
        data.Timeline.Tracks[0].Milestones.Should().BeEmpty();
    }
}