using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// Tests for DashboardData model serialization/deserialization.
/// Validates that all record types round-trip correctly through System.Text.Json,
/// camelCase JsonPropertyName attributes work, and nullable fields behave correctly.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    private static readonly JsonSerializerOptions CaseInsensitive = new() { PropertyNameCaseInsensitive = true };

    private static string CreateFullJson() => """
    {
        "schemaVersion": 1,
        "title": "Atlas Roadmap",
        "subtitle": "Platform Engineering",
        "backlogUrl": "https://dev.azure.com/example",
        "nowDateOverride": "2026-04-10",
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-07-01",
            "workstreams": [
                {
                    "id": "m1",
                    "name": "M1 - Chatbot",
                    "color": "#0078D4",
                    "milestones": [
                        { "label": "Kick-off", "date": "2026-01-15", "type": "start", "labelPosition": "above" },
                        { "label": "PoC", "date": "2026-03-01", "type": "poc" }
                    ]
                },
                {
                    "id": "m2",
                    "name": "M2 - API",
                    "color": "#00897B",
                    "milestones": [
                        { "label": "Release", "date": "2026-06-01", "type": "production" }
                    ]
                }
            ]
        },
        "heatmap": {
            "monthColumns": ["Jan", "Feb", "Mar", "Apr"],
            "categories": [
                {
                    "name": "Shipped",
                    "emoji": "✅",
                    "cssClass": "ship",
                    "months": [
                        { "month": "Jan", "items": ["Feature A", "Feature B"] },
                        { "month": "Feb", "items": [] }
                    ]
                },
                {
                    "name": "Blockers",
                    "emoji": "🚫",
                    "cssClass": "block",
                    "months": [
                        { "month": "Jan", "items": ["Bug #123"] }
                    ]
                }
            ]
        }
    }
    """;

    [Fact]
    public void Deserialization_AllNestedRecords_PopulateCorrectly()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(CreateFullJson(), CaseInsensitive);

        data.Should().NotBeNull();
        data!.SchemaVersion.Should().Be(1);
        data.Title.Should().Be("Atlas Roadmap");
        data.NowDateOverride.Should().Be("2026-04-10");

        data.Timeline.Workstreams.Should().HaveCount(2);
        data.Timeline.Workstreams[0].Milestones.Should().HaveCount(2);
        data.Timeline.Workstreams[0].Milestones[0].LabelPosition.Should().Be("above");
        data.Timeline.Workstreams[0].Milestones[1].LabelPosition.Should().BeNull();

        data.Heatmap.MonthColumns.Should().Equal("Jan", "Feb", "Mar", "Apr");
        data.Heatmap.Categories.Should().HaveCount(2);
        data.Heatmap.Categories[0].Months[0].Items.Should().Equal("Feature A", "Feature B");
        data.Heatmap.Categories[0].Months[1].Items.Should().BeEmpty();
    }

    [Fact]
    public void Deserialization_CamelCaseKeys_MapViaJsonPropertyName()
    {
        var json = """{"schemaVersion":1,"title":"T","subtitle":"S","backlogUrl":"http://x","timeline":{"startDate":"2026-01-01","endDate":"2026-07-01","workstreams":[]},"heatmap":{"monthColumns":[],"categories":[]}}""";

        var data = JsonSerializer.Deserialize<DashboardData>(json, CaseInsensitive);

        data.Should().NotBeNull();
        data!.Title.Should().Be("T");
        data.BacklogUrl.Should().Be("http://x");
        data.Timeline.StartDate.Should().Be("2026-01-01");
    }

    [Fact]
    public void Deserialization_NullableFields_DefaultToNull()
    {
        var json = """{"schemaVersion":1,"title":"T","subtitle":"S","backlogUrl":"http://x","timeline":{"startDate":"2026-01-01","endDate":"2026-07-01","workstreams":[]},"heatmap":{"monthColumns":[],"categories":[]}}""";

        var data = JsonSerializer.Deserialize<DashboardData>(json, CaseInsensitive);

        data!.NowDateOverride.Should().BeNull();
    }

    [Fact]
    public void RoundTrip_SerializeDeserialize_PreservesValues()
    {
        var original = JsonSerializer.Deserialize<DashboardData>(CreateFullJson(), CaseInsensitive)!;
        var serialized = JsonSerializer.Serialize(original);
        var roundTripped = JsonSerializer.Deserialize<DashboardData>(serialized, CaseInsensitive);

        roundTripped!.Title.Should().Be(original.Title);
        roundTripped.SchemaVersion.Should().Be(original.SchemaVersion);
        roundTripped.Timeline.Workstreams.Should().HaveCount(original.Timeline.Workstreams.Length);
        roundTripped.Heatmap.Categories[0].Months[0].Items
            .Should().Equal(original.Heatmap.Categories[0].Months[0].Items);
    }

    [Fact]
    public void Deserialization_EmptyWorkstreamsAndCategories_Succeeds()
    {
        var json = """{"schemaVersion":1,"title":"Empty","subtitle":"S","backlogUrl":"http://x","timeline":{"startDate":"2026-01-01","endDate":"2026-07-01","workstreams":[]},"heatmap":{"monthColumns":[],"categories":[]}}""";

        var data = JsonSerializer.Deserialize<DashboardData>(json, CaseInsensitive);

        data!.Timeline.Workstreams.Should().BeEmpty();
        data.Heatmap.MonthColumns.Should().BeEmpty();
        data.Heatmap.Categories.Should().BeEmpty();
    }
}