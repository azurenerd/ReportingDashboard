using System.Text.Json;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests;

public class DataServiceTests
{
    [Fact]
    public void DashboardData_ValidJson_DeserializesCorrectly()
    {
        var json = GetSampleJson();
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal(1, data.SchemaVersion);
        Assert.Equal("Test Project", data.Title);
        Assert.Equal("Test Subtitle", data.Subtitle);
        Assert.Equal("https://example.com", data.BacklogUrl);
        Assert.NotNull(data.Timeline);
        Assert.Equal("2026-01-01", data.Timeline.StartDate);
        Assert.Equal("2026-07-01", data.Timeline.EndDate);
        Assert.Single(data.Timeline.Workstreams);
        Assert.Equal("M1", data.Timeline.Workstreams[0].Id);
        Assert.Equal(2, data.Timeline.Workstreams[0].Milestones.Length);
        Assert.NotNull(data.Heatmap);
        Assert.Equal(2, data.Heatmap.MonthColumns.Length);
        Assert.Single(data.Heatmap.Categories);
    }

    [Fact]
    public void DashboardData_MalformedJson_ThrowsJsonException()
    {
        var json = "{ invalid json }";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DashboardData>(json));
    }

    [Fact]
    public void DashboardData_NowDateOverride_ParsesCorrectly()
    {
        var json = GetSampleJson(nowOverride: "\"2026-03-15\"");
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;
        Assert.Equal("2026-03-15", data.NowDateOverride);
    }

    [Fact]
    public void DashboardData_NullNowDateOverride_IsNull()
    {
        var json = GetSampleJson();
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;
        Assert.Null(data.NowDateOverride);
    }

    [Fact]
    public void DashboardData_MilestoneWithLabelPosition_ParsesCorrectly()
    {
        var json = GetSampleJson();
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;
        Assert.Equal("above", data.Timeline.Workstreams[0].Milestones[0].LabelPosition);
        Assert.Null(data.Timeline.Workstreams[0].Milestones[1].LabelPosition);
    }

    [Fact]
    public void DashboardData_EmptyItems_DeserializesAsEmptyArray()
    {
        var json = GetSampleJson();
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;
        var feb = data.Heatmap.Categories[0].Months[1];
        Assert.Empty(feb.Items);
    }

    [Fact]
    public void DashboardData_SchemaVersionMismatch_Detected()
    {
        var json = GetSampleJson().Replace("\"schemaVersion\": 1", "\"schemaVersion\": 99");
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;
        Assert.Equal(99, data.SchemaVersion);
    }

    private static string GetSampleJson(string nowOverride = "null")
    {
        return $$"""
        {
          "schemaVersion": 1,
          "title": "Test Project",
          "subtitle": "Test Subtitle",
          "backlogUrl": "https://example.com",
          "nowDateOverride": {{nowOverride}},
          "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-07-01",
            "workstreams": [
              {
                "id": "M1",
                "name": "Test Workstream",
                "color": "#0078D4",
                "milestones": [
                  { "label": "Start", "date": "2026-01-15", "type": "start", "labelPosition": "above" },
                  { "label": "PoC", "date": "2026-03-26", "type": "poc" }
                ]
              }
            ]
          },
          "heatmap": {
            "monthColumns": ["Jan", "Feb"],
            "categories": [
              {
                "name": "Shipped",
                "emoji": "OK",
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
    }
}