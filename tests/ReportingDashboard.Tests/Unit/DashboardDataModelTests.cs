using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    private static readonly JsonSerializerOptions CamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void Milestone_LabelPosition_IsNullableAndDefaultsToNull()
    {
        var json = """{"date":"2026-03-01","label":"v1","type":"checkpoint"}""";
        var milestone = JsonSerializer.Deserialize<Milestone>(json, CamelCase);

        milestone.Should().NotBeNull();
        milestone!.LabelPosition.Should().BeNull();
        milestone.Date.Should().Be("2026-03-01");
    }

    [Fact]
    public void HeatmapRow_Items_DeserializesAsDictionaryOfStringListString()
    {
        var json = """{"category":"Shipped","items":{"Jan":["Feature A","Feature B"],"Feb":[]}}""";
        var row = JsonSerializer.Deserialize<HeatmapRow>(json, CamelCase);

        row.Should().NotBeNull();
        row!.Category.Should().Be("Shipped");
        row.Items.Should().ContainKey("Jan");
        row.Items["Jan"].Should().HaveCount(2);
        row.Items["Feb"].Should().BeEmpty();
    }

    [Fact]
    public void DashboardData_RoundTrips_ThroughJsonSerialization()
    {
        var data = new DashboardData(
            Title: "Test",
            Subtitle: "Sub",
            BacklogUrl: "https://example.com",
            CurrentDate: "2026-04-01",
            TimelineStart: "2026-01-01",
            TimelineEnd: "2026-06-30",
            MilestoneStreams: new List<MilestoneStream>
            {
                new("M1", "Stream 1", "#000", new List<Milestone>
                {
                    new("2026-03-01", "v1", "checkpoint", null)
                })
            },
            Heatmap: new HeatmapData(
                new List<string> { "Jan", "Feb" },
                0,
                new List<HeatmapRow>
                {
                    new("Shipped", new Dictionary<string, List<string>> { ["Jan"] = new() { "Item" } })
                })
        );

        var json = JsonSerializer.Serialize(data, CamelCase);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json, CamelCase);

        deserialized.Should().NotBeNull();
        deserialized!.Title.Should().Be("Test");
        deserialized.MilestoneStreams.Should().HaveCount(1);
        deserialized.Heatmap.Columns.Should().HaveCount(2);
    }
}