using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    [Fact]
    public void Deserializes_CamelCaseJson_ToPascalCaseProperties()
    {
        var json = """
        {
            "project": { "title": "My Title", "subtitle": "My Sub", "backlogUrl": "https://ado.com", "currentDate": "2026-04-10" },
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-06-01", "tracks": [] },
            "heatmap": { "months": ["Jan"], "highlightMonth": "Jan", "rows": [] }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Project.Title.Should().Be("My Title");
        data.Project.Subtitle.Should().Be("My Sub");
        data.Project.BacklogUrl.Should().Be("https://ado.com");
        data.Project.CurrentDate.Should().Be("2026-04-10");
        data.Timeline.StartDate.Should().Be("2026-01-01");
        data.Heatmap.Months.Should().ContainSingle("Jan");
    }

    [Fact]
    public void BacklogUrl_IsNull_WhenOmittedFromJson()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "currentDate": "2026-01-01" },
            "timeline": { "startDate": "", "endDate": "", "tracks": [] },
            "heatmap": { "months": [], "highlightMonth": "", "rows": [] }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data!.Project.BacklogUrl.Should().BeNull();
    }

    [Fact]
    public void DefaultValues_AppliedForMissingFields()
    {
        var json = "{}";
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Project.Title.Should().BeEmpty();
        data.Timeline.Tracks.Should().BeEmpty();
        data.Heatmap.Months.Should().BeEmpty();
    }

    [Fact]
    public void MilestoneItem_DefaultType_IsCheckpoint()
    {
        var milestone = new MilestoneItem();
        milestone.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void StatusRow_Items_DeserializesCorrectly()
    {
        var json = """
        {
            "category": "Shipped",
            "items": {
                "Jan": ["Feature A", "Feature B"],
                "Feb": []
            }
        }
        """;

        var row = JsonSerializer.Deserialize<StatusRow>(json);

        row!.Category.Should().Be("Shipped");
        row.Items["Jan"].Should().HaveCount(2);
        row.Items["Feb"].Should().BeEmpty();
    }
}