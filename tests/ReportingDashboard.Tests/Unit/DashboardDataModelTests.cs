using System.Text.Json;
using Xunit;
using ReportingDashboard.Models;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    [Fact]
    public void Deserialize_FullJson_MapsAllProperties()
    {
        var json = """
        {
          "title": "Phoenix Roadmap",
          "subtitle": "Privacy Workstream",
          "backlogUrl": "https://dev.azure.com/org/project",
          "currentDate": "2026-04-14",
          "months": ["Jan","Feb","Mar"],
          "currentMonthIndex": 2,
          "timelineStart": "2026-01-01",
          "timelineEnd": "2026-03-31",
          "milestones": [
            {
              "id": "M1",
              "label": "M1 - Chatbot",
              "description": "Chatbot & MS Role",
              "color": "#0078D4",
              "markers": [
                { "date": "2026-01-15", "type": "checkpoint", "label": "Jan Check" },
                { "date": "2026-03-01", "type": "poc", "label": "Mar PoC" }
              ]
            }
          ],
          "categories": [
            { "name": "Shipped", "key": "shipped", "items": { "Jan": ["Feature A", "Feature B"] } },
            { "name": "In Progress", "key": "inProgress", "items": {} },
            { "name": "Carryover", "key": "carryover", "items": {} },
            { "name": "Blockers", "key": "blockers", "items": { "Feb": ["Blocker X"] } }
          ]
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions)!;

        Assert.Equal("Phoenix Roadmap", data.Title);
        Assert.Equal("Privacy Workstream", data.Subtitle);
        Assert.Equal("https://dev.azure.com/org/project", data.BacklogUrl);
        Assert.Equal(new DateTime(2026, 4, 14), data.CurrentDate);
        Assert.Equal(3, data.Months.Count);
        Assert.Equal(2, data.CurrentMonthIndex);
        Assert.Single(data.Milestones);
        Assert.Equal(2, data.Milestones[0].Markers.Count);
        Assert.Equal("checkpoint", data.Milestones[0].Markers[0].Type);
        Assert.Equal("poc", data.Milestones[0].Markers[1].Type);
        Assert.Equal(4, data.Categories.Count);
        Assert.Equal(2, data.Categories[0].Items["Jan"].Count);
        Assert.Single(data.Categories[3].Items["Feb"]);
    }

    [Fact]
    public void Deserialize_MilestoneMarker_ParsesDatesCorrectly()
    {
        var json = """{ "date": "2026-03-26", "type": "production", "label": "Go Live" }""";

        var marker = JsonSerializer.Deserialize<MilestoneMarker>(json, JsonOptions)!;

        Assert.Equal(new DateTime(2026, 3, 26), marker.Date);
        Assert.Equal("production", marker.Type);
        Assert.Equal("Go Live", marker.Label);
    }

    [Fact]
    public void Deserialize_HeatmapCategory_ItemsDictionaryStructure()
    {
        var json = """
        {
          "name": "Shipped",
          "key": "shipped",
          "items": {
            "Jan": ["A", "B"],
            "Feb": [],
            "Mar": ["C"]
          }
        }
        """;

        var cat = JsonSerializer.Deserialize<HeatmapCategory>(json, JsonOptions)!;

        Assert.Equal("Shipped", cat.Name);
        Assert.Equal("shipped", cat.Key);
        Assert.Equal(3, cat.Items.Count);
        Assert.Equal(2, cat.Items["Jan"].Count);
        Assert.Empty(cat.Items["Feb"]);
        Assert.Single(cat.Items["Mar"]);
    }

    [Fact]
    public void Deserialize_EmptyString_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<DashboardData>(string.Empty, JsonOptions));
    }

    [Fact]
    public void Deserialize_MalformedJson_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<DashboardData>("{broken", JsonOptions));
    }
}