using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    [Fact]
    public void Deserializes_CompleteJson_ToCorrectStructure()
    {
        var json = """
        {
            "project": {
                "title": "My Project",
                "subtitle": "Team A - April 2026",
                "backlogUrl": "https://dev.azure.com/backlog",
                "currentDate": "2026-04-15"
            },
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "tracks": [
                    {
                        "id": "M1",
                        "name": "Chatbot",
                        "color": "#0078D4",
                        "milestones": [
                            { "date": "2026-02-15", "label": "PoC Done", "type": "poc" },
                            { "date": "2026-05-01", "label": "Ship", "type": "production" }
                        ]
                    }
                ]
            },
            "heatmap": {
                "months": ["January", "February", "March"],
                "highlightMonth": "March",
                "rows": [
                    {
                        "category": "Shipped",
                        "items": { "January": ["Feature A"], "February": ["Feature B", "Feature C"] }
                    }
                ]
            }
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<DashboardData>(json, options);

        data.Should().NotBeNull();
        data!.Project.Title.Should().Be("My Project");
        data.Project.BacklogUrl.Should().Be("https://dev.azure.com/backlog");
        data.Timeline.Tracks.Should().HaveCount(1);
        data.Timeline.Tracks[0].Milestones.Should().HaveCount(2);
        data.Timeline.Tracks[0].Milestones[0].Type.Should().Be("poc");
        data.Heatmap.Months.Should().HaveCount(3);
        data.Heatmap.HighlightMonth.Should().Be("March");
        data.Heatmap.Rows[0].Items["February"].Should().HaveCount(2);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var project = new ProjectInfo();
        project.Title.Should().BeEmpty();
        project.BacklogUrl.Should().BeNull();

        var track = new TimelineTrack();
        track.Color.Should().Be("#999");

        var milestone = new MilestoneItem();
        milestone.Type.Should().Be("checkpoint");
    }
}