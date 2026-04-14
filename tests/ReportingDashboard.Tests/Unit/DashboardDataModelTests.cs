using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void DashboardData_Deserializes_ValidJson()
    {
        var json = """
        {
            "schemaVersion": 1,
            "title": "Project Atlas",
            "subtitle": "Platform Engineering",
            "backlogUrl": "https://dev.azure.com/test",
            "nowDateOverride": null,
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "workstreams": []
            },
            "heatmap": {
                "monthColumns": ["Jan", "Feb"],
                "categories": []
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data.Should().NotBeNull();
        data!.Title.Should().Be("Project Atlas");
        data.SchemaVersion.Should().Be(1);
        data.NowDateOverride.Should().BeNull();
        data.Timeline.Workstreams.Should().BeEmpty();
        data.Heatmap.MonthColumns.Should().HaveCount(2);
    }

    [Fact]
    public void Milestone_Deserializes_WithOptionalLabelPosition()
    {
        var json = """
        {
            "label": "Mar PoC",
            "date": "2026-03-15",
            "type": "poc",
            "labelPosition": "above"
        }
        """;

        var ms = JsonSerializer.Deserialize<Milestone>(json, Options);

        ms.Should().NotBeNull();
        ms!.Label.Should().Be("Mar PoC");
        ms.LabelPosition.Should().Be("above");
    }

    [Fact]
    public void Milestone_Deserializes_WithoutLabelPosition()
    {
        var json = """
        {
            "label": "Start",
            "date": "2026-01-15",
            "type": "start"
        }
        """;

        var ms = JsonSerializer.Deserialize<Milestone>(json, Options);

        ms.Should().NotBeNull();
        ms!.LabelPosition.Should().BeNull();
    }

    [Fact]
    public void StatusCategory_Deserializes_WithEmptyItems()
    {
        var json = """
        {
            "name": "Shipped",
            "emoji": "✅",
            "cssClass": "ship",
            "months": [
                { "month": "Jan", "items": [] },
                { "month": "Feb", "items": ["Item A", "Item B"] }
            ]
        }
        """;

        var cat = JsonSerializer.Deserialize<StatusCategory>(json, Options);

        cat.Should().NotBeNull();
        cat!.Months[0].Items.Should().BeEmpty();
        cat.Months[1].Items.Should().HaveCount(2);
    }

    [Fact]
    public void DashboardData_WithNowDateOverride_DeserializesCorrectly()
    {
        var json = """
        {
            "schemaVersion": 1,
            "title": "Test",
            "subtitle": "Sub",
            "backlogUrl": "https://example.com",
            "nowDateOverride": "2026-03-15",
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "workstreams": []
            },
            "heatmap": {
                "monthColumns": [],
                "categories": []
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data.Should().NotBeNull();
        data!.NowDateOverride.Should().Be("2026-03-15");
    }

    [Fact]
    public void DashboardData_WithoutOptionalFields_DeserializesCorrectly()
    {
        var json = """
        {
            "schemaVersion": 1,
            "title": "Test",
            "subtitle": "Sub",
            "backlogUrl": "https://example.com",
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "workstreams": []
            },
            "heatmap": {
                "monthColumns": [],
                "categories": []
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data.Should().NotBeNull();
        data!.NowDateOverride.Should().BeNull();
    }

    [Fact]
    public void Workstream_Deserializes_WithMilestones()
    {
        var json = """
        {
            "id": "M1",
            "name": "Chatbot & MS Role",
            "color": "#0078D4",
            "milestones": [
                { "label": "Jan Start", "date": "2026-01-12", "type": "start" },
                { "label": "Mar PoC", "date": "2026-03-26", "type": "poc" },
                { "label": "May Prod", "date": "2026-05-01", "type": "production" }
            ]
        }
        """;

        var ws = JsonSerializer.Deserialize<Workstream>(json, Options);

        ws.Should().NotBeNull();
        ws!.Id.Should().Be("M1");
        ws.Milestones.Should().HaveCount(3);
        ws.Milestones[0].Type.Should().Be("start");
        ws.Milestones[1].Type.Should().Be("poc");
        ws.Milestones[2].Type.Should().Be("production");
    }
}