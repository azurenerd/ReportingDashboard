using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

/// <summary>
/// Tests JSON deserialization behavior for all model types,
/// covering camelCase mapping, missing fields, null-safe defaults, and edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class JsonDeserializationFoundationTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = false,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    // --- DashboardData deserialization ---

    [Fact]
    public void DashboardData_DeserializeCamelCase_MapsAllProperties()
    {
        var json = """
        {
            "title": "My Project",
            "subtitle": "Team Alpha",
            "backlogLink": "https://ado.com/backlog",
            "currentMonth": "Mar",
            "months": ["Jan", "Feb", "Mar"],
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-06-30", "nowDate": "2026-03-15", "tracks": [] },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options)!;

        data.Title.Should().Be("My Project");
        data.Subtitle.Should().Be("Team Alpha");
        data.BacklogLink.Should().Be("https://ado.com/backlog");
        data.CurrentMonth.Should().Be("Mar");
        data.Months.Should().BeEquivalentTo(new[] { "Jan", "Feb", "Mar" });
    }

    [Fact]
    public void DashboardData_MissingOptionalFields_DefaultsApply()
    {
        var json = "{}";

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options)!;

        data.Title.Should().BeEmpty();
        data.Subtitle.Should().BeEmpty();
        data.BacklogLink.Should().BeEmpty();
        data.CurrentMonth.Should().BeEmpty();
        data.Months.Should().NotBeNull().And.BeEmpty();
        data.Timeline.Should().NotBeNull();
        data.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void DashboardData_PascalCaseKeys_AreNotMapped()
    {
        // PropertyNameCaseInsensitive = false means PascalCase won't map
        var json = """{ "Title": "Wrong Case", "BacklogLink": "https://test.com" }""";

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options)!;

        data.Title.Should().BeEmpty("PascalCase 'Title' should not map when case-insensitive is false");
        data.BacklogLink.Should().BeEmpty();
    }

    [Fact]
    public void DashboardData_ExtraFields_AreIgnored()
    {
        var json = """
        {
            "title": "Test",
            "unknownField": 42,
            "anotherExtra": { "nested": true }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options)!;

        data.Title.Should().Be("Test");
    }

    // --- HeatmapData deserialization ---

    [Fact]
    public void HeatmapData_MissingShippedKey_DefaultsToEmptyDictionary()
    {
        var json = """{ "inProgress": { "jan": ["Item"] } }""";

        var data = JsonSerializer.Deserialize<HeatmapData>(json, Options)!;

        data.Shipped.Should().NotBeNull().And.BeEmpty();
        data.InProgress.Should().ContainKey("jan");
    }

    [Fact]
    public void HeatmapData_EmptyObject_AllDictionariesAreEmpty()
    {
        var json = "{}";

        var data = JsonSerializer.Deserialize<HeatmapData>(json, Options)!;

        data.Shipped.Should().NotBeNull().And.BeEmpty();
        data.InProgress.Should().NotBeNull().And.BeEmpty();
        data.Carryover.Should().NotBeNull().And.BeEmpty();
        data.Blockers.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void HeatmapData_InProgressCamelCase_DeserializesCorrectly()
    {
        var json = """{ "inProgress": { "apr": ["Monitoring", "Dashboards"] } }""";

        var data = JsonSerializer.Deserialize<HeatmapData>(json, Options)!;

        data.InProgress["apr"].Should().HaveCount(2);
        data.InProgress["apr"].Should().Contain("Monitoring");
    }

    [Fact]
    public void HeatmapData_EmptyListsForMonth_DeserializesAsEmptyList()
    {
        var json = """{ "shipped": { "jan": [] } }""";

        var data = JsonSerializer.Deserialize<HeatmapData>(json, Options)!;

        data.Shipped["jan"].Should().NotBeNull().And.BeEmpty();
    }

    // --- TimelineData deserialization ---

    [Fact]
    public void TimelineData_FullDeserialization_AllFieldsMapped()
    {
        var json = """
        {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "name": "Chatbot",
                    "label": "M1",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-02-15", "label": "Feb 15", "type": "poc" }
                    ]
                }
            ]
        }
        """;

        var data = JsonSerializer.Deserialize<TimelineData>(json, Options)!;

        data.StartDate.Should().Be("2026-01-01");
        data.EndDate.Should().Be("2026-06-30");
        data.NowDate.Should().Be("2026-04-10");
        data.Tracks.Should().HaveCount(1);
        data.Tracks[0].Name.Should().Be("Chatbot");
        data.Tracks[0].Milestones[0].Type.Should().Be("poc");
    }

    [Fact]
    public void TimelineData_MissingTracks_DefaultsToEmptyList()
    {
        var json = """{ "startDate": "2026-01-01" }""";

        var data = JsonSerializer.Deserialize<TimelineData>(json, Options)!;

        data.Tracks.Should().NotBeNull().And.BeEmpty();
    }

    // --- TimelineTrack deserialization ---

    [Fact]
    public void TimelineTrack_MissingColor_DefaultsToGray()
    {
        var json = """{ "name": "Track", "label": "M1", "milestones": [] }""";

        var track = JsonSerializer.Deserialize<TimelineTrack>(json, Options)!;

        track.Color.Should().Be("#999");
    }

    [Fact]
    public void TimelineTrack_WithColor_OverridesDefault()
    {
        var json = """{ "name": "Track", "label": "M1", "color": "#FF0000", "milestones": [] }""";

        var track = JsonSerializer.Deserialize<TimelineTrack>(json, Options)!;

        track.Color.Should().Be("#FF0000");
    }

    [Fact]
    public void TimelineTrack_MissingMilestones_DefaultsToEmptyList()
    {
        var json = """{ "name": "Track", "label": "M1" }""";

        var track = JsonSerializer.Deserialize<TimelineTrack>(json, Options)!;

        track.Milestones.Should().NotBeNull().And.BeEmpty();
    }

    // --- Milestone deserialization ---

    [Fact]
    public void Milestone_DefaultType_IsCheckpoint()
    {
        var json = """{ "date": "2026-03-01", "label": "Mar 1" }""";

        var milestone = JsonSerializer.Deserialize<Milestone>(json, Options)!;

        milestone.Type.Should().Be("checkpoint");
    }

    [Theory]
    [InlineData("checkpoint")]
    [InlineData("poc")]
    [InlineData("production")]
    public void Milestone_AllValidTypes_DeserializeCorrectly(string type)
    {
        var json = $$"""{ "date": "2026-03-01", "type": "{{type}}", "label": "Mar 1" }""";

        var milestone = JsonSerializer.Deserialize<Milestone>(json, Options)!;

        milestone.Type.Should().Be(type);
    }

    [Fact]
    public void Milestone_EmptyObject_HasDefaults()
    {
        var json = "{}";

        var milestone = JsonSerializer.Deserialize<Milestone>(json, Options)!;

        milestone.Date.Should().BeEmpty();
        milestone.Label.Should().BeEmpty();
        milestone.Type.Should().Be("checkpoint");
    }

    // --- Comments and trailing commas ---

    [Fact]
    public void DashboardData_JsonWithComments_DeserializesCorrectly()
    {
        var json = """
        {
            // This is a comment
            "title": "Commented JSON",
            "subtitle": "Sub",
            /* Block comment */
            "months": ["Jan"]
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options)!;

        data.Title.Should().Be("Commented JSON");
    }

    [Fact]
    public void DashboardData_JsonWithTrailingCommas_DeserializesCorrectly()
    {
        var json = """
        {
            "title": "Trailing",
            "months": ["Jan", "Feb",],
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options)!;

        data.Title.Should().Be("Trailing");
        data.Months.Should().HaveCount(2);
    }
}