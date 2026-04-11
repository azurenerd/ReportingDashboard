using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class FullDataDeserializationTests
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private const string FullSampleJson = """
    {
        "title": "Privacy Automation Release Roadmap",
        "subtitle": "Trusted Platform - Privacy Automation - April 2026",
        "backlogLink": "https://dev.azure.com/org/project/_backlogs",
        "currentMonth": "Apr",
        "months": ["Jan", "Feb", "Mar", "Apr"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "name": "M1",
                    "label": "Chatbot",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-02-01", "type": "checkpoint", "label": "Design" },
                        { "date": "2026-03-15", "type": "poc", "label": "PoC" },
                        { "date": "2026-05-01", "type": "production", "label": "GA" }
                    ]
                },
                {
                    "name": "M2",
                    "label": "Data Pipeline",
                    "color": "#00897B",
                    "milestones": [
                        { "date": "2026-01-15", "type": "checkpoint", "label": "Kickoff" }
                    ]
                },
                {
                    "name": "M3",
                    "label": "Compliance",
                    "color": "#546E7A",
                    "milestones": []
                }
            ]
        },
        "heatmap": {
            "shipped": {
                "jan": ["Auth Module", "Setup CI"],
                "feb": ["Search API"]
            },
            "inProgress": {
                "mar": ["Dashboard UI"],
                "apr": ["Reports"]
            },
            "carryover": {
                "apr": ["Migration"]
            },
            "blockers": {
                "apr": ["Vendor Delay"]
            }
        }
    }
    """;

    [Fact]
    public void FullSample_DeserializesCompletely()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(FullSampleJson, CamelCaseOptions);

        data.Should().NotBeNull();
        data!.Title.Should().Be("Privacy Automation Release Roadmap");
        data.Subtitle.Should().Contain("April 2026");
        data.BacklogLink.Should().StartWith("https://");
        data.CurrentMonth.Should().Be("Apr");
        data.Months.Should().HaveCount(4);
    }

    [Fact]
    public void FullSample_Timeline_HasThreeTracks()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(FullSampleJson, CamelCaseOptions);

        data!.Timeline.Should().NotBeNull();
        data.Timeline!.Tracks.Should().HaveCount(3);
        data.Timeline.Tracks[0].Name.Should().Be("M1");
        data.Timeline.Tracks[1].Name.Should().Be("M2");
        data.Timeline.Tracks[2].Name.Should().Be("M3");
    }

    [Fact]
    public void FullSample_TimelineDates_ArePopulated()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(FullSampleJson, CamelCaseOptions);

        data!.Timeline!.StartDate.Should().Be("2026-01-01");
        data.Timeline.EndDate.Should().Be("2026-06-30");
        data.Timeline.NowDate.Should().Be("2026-04-10");
    }

    [Fact]
    public void FullSample_Milestones_AllThreeTypes()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(FullSampleJson, CamelCaseOptions);

        var m1Milestones = data!.Timeline!.Tracks[0].Milestones;
        m1Milestones.Should().HaveCount(3);
        m1Milestones.Should().Contain(m => m.Type == "checkpoint");
        m1Milestones.Should().Contain(m => m.Type == "poc");
        m1Milestones.Should().Contain(m => m.Type == "production");
    }

    [Fact]
    public void FullSample_EmptyMilestones_DoesNotThrow()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(FullSampleJson, CamelCaseOptions);

        var m3 = data!.Timeline!.Tracks[2];
        m3.Milestones.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void FullSample_Heatmap_ShippedItems()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(FullSampleJson, CamelCaseOptions);

        data!.Heatmap.Should().NotBeNull();
        data.Heatmap!.Shipped.Should().ContainKey("jan");
        data.Heatmap.Shipped["jan"].Should().HaveCount(2);
        data.Heatmap.Shipped["feb"].Should().HaveCount(1);
    }

    [Fact]
    public void FullSample_Heatmap_AllCategories()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(FullSampleJson, CamelCaseOptions);

        data!.Heatmap!.InProgress.Should().ContainKey("mar");
        data.Heatmap.InProgress.Should().ContainKey("apr");
        data.Heatmap.Carryover.Should().ContainKey("apr");
        data.Heatmap.Blockers.Should().ContainKey("apr");
        data.Heatmap.Blockers["apr"].Should().Contain("Vendor Delay");
    }

    [Fact]
    public void Deserialize_MalformedJson_ThrowsJsonException()
    {
        var malformedJson = """{ "title": "test", }""";

        var action = () => JsonSerializer.Deserialize<DashboardData>(malformedJson);

        action.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_CompletelyInvalidJson_ThrowsJsonException()
    {
        var invalidJson = "not json at all";

        var action = () => JsonSerializer.Deserialize<DashboardData>(invalidJson);

        action.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_NullJsonString_ThrowsArgumentNullException()
    {
        string? nullJson = null;

        var action = () => JsonSerializer.Deserialize<DashboardData>(nullJson!);

        action.Should().Throw<ArgumentNullException>();
    }
}