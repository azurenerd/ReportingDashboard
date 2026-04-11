using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Models;

[Trait("Category", "Integration")]
public class FullDataJsonSerializationTests
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static string CreateSampleJson() => """
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
    public void Deserialize_ThenSerialize_RoundTrip_PreservesStructure()
    {
        var original = JsonSerializer.Deserialize<DashboardData>(CreateSampleJson(), CamelCaseOptions);

        var serialized = JsonSerializer.Serialize(original, CamelCaseOptions);
        var roundTripped = JsonSerializer.Deserialize<DashboardData>(serialized, CamelCaseOptions);

        roundTripped.Should().NotBeNull();
        roundTripped!.Title.Should().Be(original!.Title);
        roundTripped.Subtitle.Should().Be(original.Subtitle);
        roundTripped.Months.Should().BeEquivalentTo(original.Months);
        roundTripped.Timeline!.Tracks.Should().HaveCount(original.Timeline!.Tracks.Count);
        roundTripped.Heatmap!.Shipped.Should().HaveCount(original.Heatmap!.Shipped.Count);
    }

    [Fact]
    public void Deserialize_MilestoneTypes_IncludeAllThree()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(CreateSampleJson(), CamelCaseOptions);

        var allMilestones = data!.Timeline!.Tracks
            .SelectMany(t => t.Milestones)
            .ToList();

        allMilestones.Should().Contain(m => m.Type == "checkpoint");
        allMilestones.Should().Contain(m => m.Type == "poc");
        allMilestones.Should().Contain(m => m.Type == "production");
    }

    [Fact]
    public void Deserialize_HeatmapCategories_AllPresent()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(CreateSampleJson(), CamelCaseOptions);

        data!.Heatmap!.Shipped.Should().NotBeEmpty();
        data.Heatmap.InProgress.Should().NotBeEmpty();
        data.Heatmap.Carryover.Should().NotBeEmpty();
        data.Heatmap.Blockers.Should().NotBeEmpty();
    }

    [Fact]
    public void Deserialize_EmptyCollections_DoNotProduceNull()
    {
        var json = """
        {
            "title": "Minimal",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": [],
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

        var data = JsonSerializer.Deserialize<DashboardData>(json, CamelCaseOptions);

        data!.Months.Should().NotBeNull().And.BeEmpty();
        data.Timeline!.Tracks.Should().NotBeNull().And.BeEmpty();
        data.Heatmap!.Shipped.Should().NotBeNull().And.BeEmpty();
        data.Heatmap.InProgress.Should().NotBeNull().And.BeEmpty();
        data.Heatmap.Carryover.Should().NotBeNull().And.BeEmpty();
        data.Heatmap.Blockers.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Deserialize_Track_WithEmptyMilestones_DoesNotThrow()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(CreateSampleJson(), CamelCaseOptions);

        var m3 = data!.Timeline!.Tracks[2];
        m3.Milestones.Should().NotBeNull().And.BeEmpty();
        m3.Name.Should().Be("M3");
    }

    [Fact]
    public void Deserialize_CaseInsensitive_MatchesCamelCase()
    {
        var json = """
        {
            "Title": "Upper Case",
            "Subtitle": "Upper Sub",
            "BacklogLink": "https://test.com",
            "CurrentMonth": "Apr",
            "Months": ["Jan"],
            "Timeline": {
                "StartDate": "2026-01-01",
                "EndDate": "2026-06-30",
                "NowDate": "2026-04-10",
                "Tracks": [{ "Name": "M1", "Label": "T", "Color": "#000", "Milestones": [] }]
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, CamelCaseOptions);

        data.Should().NotBeNull();
        data!.Title.Should().Be("Upper Case");
        data.Timeline!.Tracks.Should().HaveCount(1);
    }

    [Fact]
    public void Deserialize_MissingOptionalHeatmap_RemainsNull()
    {
        var json = """
        {
            "title": "No Heatmap",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"]
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, CamelCaseOptions);

        data!.Heatmap.Should().BeNull();
        data.Timeline.Should().BeNull();
    }

    [Fact]
    public void Deserialize_MissingOptionalTimeline_RemainsNull()
    {
        var json = """
        {
            "title": "No Timeline",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"]
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, CamelCaseOptions);

        data!.Timeline.Should().BeNull();
    }

    [Fact]
    public void Deserialize_ErrorMessage_NotInJson()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(CreateSampleJson(), CamelCaseOptions);

        data!.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Serialize_ErrorMessage_IsExcluded()
    {
        var data = new DashboardData
        {
            Title = "Test",
            ErrorMessage = "Some error"
        };

        var json = JsonSerializer.Serialize(data, CamelCaseOptions);

        json.Should().NotContain("errorMessage");
        json.Should().NotContain("ErrorMessage");
    }
}