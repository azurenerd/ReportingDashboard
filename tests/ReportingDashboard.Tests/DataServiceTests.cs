using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Tests;

public class DataServiceTests
{
    private static readonly string ValidJson = """
    {
      "schemaVersion": 1,
      "title": "Test Project",
      "subtitle": "Test Subtitle",
      "backlogUrl": "https://example.com",
      "nowDateOverride": null,
      "timeline": {
        "startDate": "2026-01-01",
        "endDate": "2026-07-01",
        "workstreams": [
          {
            "id": "M1",
            "name": "Test Workstream",
            "color": "#0078D4",
            "milestones": [
              { "label": "Jan 12", "date": "2026-01-12", "type": "start" }
            ]
          }
        ]
      },
      "heatmap": {
        "monthColumns": ["Jan", "Feb"],
        "categories": [
          {
            "name": "Shipped",
            "emoji": "✅",
            "cssClass": "ship",
            "months": [
              { "month": "Jan", "items": ["Item A"] },
              { "month": "Feb", "items": [] }
            ]
          }
        ]
      }
    }
    """;

    [Fact]
    public void Deserialize_ValidJson_ReturnsData()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(ValidJson);
        Assert.NotNull(data);
        Assert.Equal(1, data!.SchemaVersion);
        Assert.Equal("Test Project", data.Title);
        Assert.Equal("Test Subtitle", data.Subtitle);
        Assert.Equal("https://example.com", data.BacklogUrl);
        Assert.Null(data.NowDateOverride);
    }

    [Fact]
    public void Deserialize_Timeline_ParsesCorrectly()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(ValidJson)!;
        Assert.Equal("2026-01-01", data.Timeline.StartDate);
        Assert.Equal("2026-07-01", data.Timeline.EndDate);
        Assert.Single(data.Timeline.Workstreams);
        Assert.Equal("M1", data.Timeline.Workstreams[0].Id);
        Assert.Equal("#0078D4", data.Timeline.Workstreams[0].Color);
        Assert.Single(data.Timeline.Workstreams[0].Milestones);
        Assert.Equal("start", data.Timeline.Workstreams[0].Milestones[0].Type);
    }

    [Fact]
    public void Deserialize_Heatmap_ParsesCorrectly()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(ValidJson)!;
        Assert.Equal(2, data.Heatmap.MonthColumns.Length);
        Assert.Single(data.Heatmap.Categories);

        var cat = data.Heatmap.Categories[0];
        Assert.Equal("Shipped", cat.Name);
        Assert.Equal("ship", cat.CssClass);
        Assert.Equal(2, cat.Months.Length);
        Assert.Single(cat.Months[0].Items);
        Assert.Equal("Item A", cat.Months[0].Items[0]);
        Assert.Empty(cat.Months[1].Items);
    }

    [Fact]
    public void Deserialize_MalformedJson_ThrowsJsonException()
    {
        var badJson = "{ this is not valid json }";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DashboardData>(badJson));
    }

    [Fact]
    public void Deserialize_NowDateOverride_Parses()
    {
        var json = ValidJson.Replace("\"nowDateOverride\": null", "\"nowDateOverride\": \"2026-03-15\"");
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;
        Assert.Equal("2026-03-15", data.NowDateOverride);
        var parsed = DateOnly.ParseExact(data.NowDateOverride, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        Assert.Equal(new DateOnly(2026, 3, 15), parsed);
    }

    [Fact]
    public void Deserialize_MilestoneWithLabelPosition()
    {
        var json = ValidJson.Replace(
            "{ \"label\": \"Jan 12\", \"date\": \"2026-01-12\", \"type\": \"start\" }",
            "{ \"label\": \"Jan 12\", \"date\": \"2026-01-12\", \"type\": \"start\", \"labelPosition\": \"below\" }");
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;
        Assert.Equal("below", data.Timeline.Workstreams[0].Milestones[0].LabelPosition);
    }

    [Fact]
    public void Deserialize_SchemaVersion_MismatchDetectable()
    {
        var json = ValidJson.Replace("\"schemaVersion\": 1", "\"schemaVersion\": 99");
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;
        Assert.Equal(99, data.SchemaVersion);
    }
}