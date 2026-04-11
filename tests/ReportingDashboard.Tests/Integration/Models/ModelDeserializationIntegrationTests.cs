using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Models;

[Trait("Category", "Integration")]
public class ModelDeserializationIntegrationTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static string GetFullSampleJson() => """
    {
        "title": "Privacy Automation Release Roadmap",
        "subtitle": "Trusted Platform – Privacy Automation – April 2026",
        "backlogLink": "https://dev.azure.com/org/project/_backlogs",
        "currentMonth": "Apr",
        "months": ["Jan", "Feb", "Mar", "Apr"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "id": "M1",
                    "name": "Chatbot Integration",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-01-20", "label": "Jan 20", "type": "checkpoint" },
                        { "date": "2026-02-15", "label": "Feb 15", "type": "poc" },
                        { "date": "2026-04-01", "label": "Apr 1", "type": "production" }
                    ]
                },
                {
                    "id": "M2",
                    "name": "Data Pipeline",
                    "color": "#00897B",
                    "milestones": [
                        { "date": "2026-03-01", "label": "Mar 1", "type": "poc" },
                        { "date": "2026-05-15", "label": "May 15", "type": "production" }
                    ]
                },
                {
                    "id": "M3",
                    "name": "Compliance Engine",
                    "color": "#546E7A",
                    "milestones": [
                        { "date": "2026-02-01", "label": "Feb 1", "type": "checkpoint" },
                        { "date": "2026-06-01", "label": "Jun 1", "type": "production" }
                    ]
                }
            ]
        },
        "heatmap": {
            "shipped": {
                "jan": ["Privacy SDK v2.1", "Telemetry Pipeline"],
                "feb": ["Consent API v3", "Data Classification ML"],
                "mar": ["Auto-redaction Engine"]
            },
            "inProgress": {
                "mar": ["Cross-tenant Sync"],
                "apr": ["Real-time Monitoring", "Compliance Dashboard v2"]
            },
            "carryover": {
                "apr": ["Legacy Migration Script"]
            },
            "blockers": {
                "apr": ["Pending Legal Review on EU Data"]
            }
        }
    }
    """;

    [Fact]
    public void FullJson_DeserializesToDashboardData_WithAllFields()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(GetFullSampleJson(), Options);

        data.Should().NotBeNull();
        data!.Title.Should().Be("Privacy Automation Release Roadmap");
        data.Subtitle.Should().Contain("Trusted Platform");
        data.BacklogLink.Should().StartWith("https://");
        data.CurrentMonth.Should().Be("Apr");
    }

    [Fact]
    public void FullJson_Months_DeserializeAs4ElementList()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(GetFullSampleJson(), Options)!;

        data.Months.Should().HaveCount(4);
        data.Months[0].Should().Be("Jan");
        data.Months[3].Should().Be("Apr");
    }

    [Fact]
    public void FullJson_Timeline_Has3TracksWithCorrectProperties()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(GetFullSampleJson(), Options)!;

        data.Timeline.Tracks.Should().HaveCount(3);

        data.Timeline.Tracks[0].Id.Should().Be("M1");
        data.Timeline.Tracks[0].Name.Should().Be("Chatbot Integration");
        data.Timeline.Tracks[0].Color.Should().Be("#0078D4");
        data.Timeline.Tracks[0].Milestones.Should().HaveCount(3);

        data.Timeline.Tracks[1].Id.Should().Be("M2");
        data.Timeline.Tracks[1].Milestones.Should().HaveCount(2);

        data.Timeline.Tracks[2].Id.Should().Be("M3");
        data.Timeline.Tracks[2].Milestones.Should().HaveCount(2);
    }

    [Fact]
    public void FullJson_MilestoneTypes_ContainAllThreeVariants()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(GetFullSampleJson(), Options)!;

        var allTypes = data.Timeline.Tracks
            .SelectMany(t => t.Milestones)
            .Select(m => m.Type)
            .Distinct()
            .ToList();

        allTypes.Should().Contain("checkpoint");
        allTypes.Should().Contain("poc");
        allTypes.Should().Contain("production");
    }

    [Fact]
    public void FullJson_HeatmapShipped_ContainsCorrectItemCounts()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(GetFullSampleJson(), Options)!;

        data.Heatmap.Shipped["jan"].Should().HaveCount(2);
        data.Heatmap.Shipped["feb"].Should().HaveCount(2);
        data.Heatmap.Shipped["mar"].Should().HaveCount(1);
    }

    [Fact]
    public void FullJson_HeatmapInProgress_ContainsCorrectItemCounts()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(GetFullSampleJson(), Options)!;

        data.Heatmap.InProgress["mar"].Should().HaveCount(1);
        data.Heatmap.InProgress["apr"].Should().HaveCount(2);
    }

    [Fact]
    public void FullJson_HeatmapItemContent_IsAccurate()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(GetFullSampleJson(), Options)!;

        data.Heatmap.Shipped["jan"].Should().Contain("Privacy SDK v2.1");
        data.Heatmap.Shipped["jan"].Should().Contain("Telemetry Pipeline");
        data.Heatmap.InProgress["apr"].Should().Contain("Real-time Monitoring");
        data.Heatmap.Carryover["apr"].Should().Contain("Legacy Migration Script");
        data.Heatmap.Blockers["apr"].Should().Contain("Pending Legal Review on EU Data");
    }

    [Fact]
    public void FullJson_MissingHeatmapKey_IsNotPresent()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(GetFullSampleJson(), Options)!;

        data.Heatmap.Shipped.ContainsKey("may").Should().BeFalse();
        data.Heatmap.Shipped.ContainsKey("jun").Should().BeFalse();
    }

    [Fact]
    public void PartialJson_MissingTimeline_GetsDefaultTimelineData()
    {
        var json = """{ "title": "Partial" }""";
        var data = JsonSerializer.Deserialize<DashboardData>(json, Options)!;

        data.Timeline.Should().NotBeNull();
        data.Timeline.Tracks.Should().BeEmpty();
        data.Timeline.StartDate.Should().BeEmpty();
    }

    [Fact]
    public void PartialJson_MissingHeatmap_GetsDefaultHeatmapData()
    {
        var json = """{ "title": "Partial" }""";
        var data = JsonSerializer.Deserialize<DashboardData>(json, Options)!;

        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Shipped.Should().BeEmpty();
        data.Heatmap.InProgress.Should().BeEmpty();
        data.Heatmap.Carryover.Should().BeEmpty();
        data.Heatmap.Blockers.Should().BeEmpty();
    }

    [Fact]
    public void PartialJson_MissingMonths_GetsEmptyList()
    {
        var json = """{ "title": "No Months" }""";
        var data = JsonSerializer.Deserialize<DashboardData>(json, Options)!;

        data.Months.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void EmptyObject_AllCollectionsInitialized()
    {
        var data = JsonSerializer.Deserialize<DashboardData>("{}", Options)!;

        data.Months.Should().NotBeNull();
        data.Timeline.Should().NotBeNull();
        data.Timeline.Tracks.Should().NotBeNull();
        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Shipped.Should().NotBeNull();
        data.Heatmap.InProgress.Should().NotBeNull();
        data.Heatmap.Carryover.Should().NotBeNull();
        data.Heatmap.Blockers.Should().NotBeNull();
    }

    [Fact]
    public void SerializeAndDeserialize_ProducesEquivalentResult()
    {
        var original = JsonSerializer.Deserialize<DashboardData>(GetFullSampleJson(), Options)!;
        var serialized = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(serialized, Options)!;

        deserialized.Title.Should().Be(original.Title);
        deserialized.Months.Should().BeEquivalentTo(original.Months);
        deserialized.Timeline.Tracks.Should().HaveCount(original.Timeline.Tracks.Count);
        deserialized.Heatmap.Shipped.Should().HaveCount(original.Heatmap.Shipped.Count);
    }
}