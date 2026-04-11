using System.Text.Json;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

/// <summary>
/// Tests that verify [JsonPropertyName] attributes produce correct
/// camelCase JSON keys when using default serializer (no naming policy override).
/// This is critical because the data.json file uses camelCase.
/// </summary>
[Trait("Category", "Unit")]
public class JsonPropertyNameMappingTests
{
    [Fact]
    public void DashboardData_Serialization_UsesExactJsonPropertyNames()
    {
        var data = new DashboardData
        {
            Title = "Test",
            BacklogLink = "http://link",
            CurrentMonth = "March"
        };

        // Use default options (no naming policy) to test [JsonPropertyName] attributes
        var json = JsonSerializer.Serialize(data);

        Assert.Contains("\"title\"", json);
        Assert.Contains("\"subtitle\"", json);
        Assert.Contains("\"backlogLink\"", json);
        Assert.Contains("\"currentMonth\"", json);
        Assert.Contains("\"months\"", json);
        Assert.Contains("\"timeline\"", json);
        Assert.Contains("\"heatmap\"", json);
    }

    [Fact]
    public void TimelineData_Serialization_UsesExactJsonPropertyNames()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10"
        };

        var json = JsonSerializer.Serialize(tl);

        Assert.Contains("\"startDate\"", json);
        Assert.Contains("\"endDate\"", json);
        Assert.Contains("\"nowDate\"", json);
        Assert.Contains("\"tracks\"", json);
    }

    [Fact]
    public void TimelineTrack_Serialization_UsesExactJsonPropertyNames()
    {
        var track = new TimelineTrack { Name = "M1", Label = "Core", Color = "#000" };

        var json = JsonSerializer.Serialize(track);

        Assert.Contains("\"name\"", json);
        Assert.Contains("\"label\"", json);
        Assert.Contains("\"color\"", json);
        Assert.Contains("\"milestones\"", json);
    }

    [Fact]
    public void Milestone_Serialization_UsesExactJsonPropertyNames()
    {
        var ms = new Milestone { Date = "2026-02-15", Type = "poc", Label = "PoC" };

        var json = JsonSerializer.Serialize(ms);

        Assert.Contains("\"date\"", json);
        Assert.Contains("\"type\"", json);
        Assert.Contains("\"label\"", json);
    }

    [Fact]
    public void HeatmapData_Serialization_UsesExactJsonPropertyNames()
    {
        var hm = new HeatmapData();

        var json = JsonSerializer.Serialize(hm);

        Assert.Contains("\"shipped\"", json);
        Assert.Contains("\"inProgress\"", json);
        Assert.Contains("\"carryover\"", json);
        Assert.Contains("\"blockers\"", json);
    }

    [Fact]
    public void DashboardData_Deserialization_WithJsonPropertyNames_WorksWithoutNamingPolicy()
    {
        var json = """
        {
            "title":"Test Project",
            "subtitle":"Sub",
            "backlogLink":"https://link",
            "currentMonth":"April",
            "months":["Jan","Feb"],
            "timeline":{"startDate":"2026-01-01","endDate":"2026-07-01","nowDate":"2026-04-10","tracks":[]},
            "heatmap":{"shipped":{},"inProgress":{"apr":["X"]},"carryover":{},"blockers":{}}
        }
        """;

        // Deserialize WITHOUT any naming policy - relies purely on [JsonPropertyName]
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal("Test Project", data!.Title);
        Assert.Equal("Sub", data.Subtitle);
        Assert.Equal("https://link", data.BacklogLink);
        Assert.Equal("April", data.CurrentMonth);
        Assert.Equal(2, data.Months.Count);
        Assert.Equal("2026-01-01", data.Timeline.StartDate);
        Assert.Single(data.Heatmap.InProgress["apr"]);
    }

    [Fact]
    public void HeatmapData_InProgress_CamelCase_DeserializesCorrectly()
    {
        // Critical: "inProgress" in JSON must map to InProgress C# property
        var json = """{"shipped":{},"inProgress":{"mar":["Task1"]},"carryover":{},"blockers":{}}""";

        var hm = JsonSerializer.Deserialize<HeatmapData>(json);

        Assert.NotNull(hm);
        Assert.Single(hm!.InProgress);
        Assert.Single(hm.InProgress["mar"]);
        Assert.Equal("Task1", hm.InProgress["mar"][0]);
    }

    [Fact]
    public void TimelineTrack_Color_DefaultValue_Is999()
    {
        var json = """{"name":"M1","label":"Test","milestones":[]}""";

        var track = JsonSerializer.Deserialize<TimelineTrack>(json);

        Assert.NotNull(track);
        Assert.Equal("#999", track!.Color);
    }

    [Fact]
    public void Milestone_Type_DefaultValue_IsCheckpoint()
    {
        var json = """{"date":"2026-01-01","label":"Test"}""";

        var ms = JsonSerializer.Deserialize<Milestone>(json);

        Assert.NotNull(ms);
        Assert.Equal("checkpoint", ms!.Type);
    }
}