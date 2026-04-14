using System.Text.Json;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

public class DashboardDataModelTests
{
    [Fact]
    public void DashboardData_DefaultCollections_NotNull()
    {
        var data = new DashboardData();

        Assert.NotNull(data.Months);
        Assert.Empty(data.Months);
        Assert.NotNull(data.Timeline);
        Assert.NotNull(data.Timeline.Tracks);
        Assert.Empty(data.Timeline.Tracks);
        Assert.NotNull(data.Heatmap);
        Assert.NotNull(data.Heatmap.Shipped);
        Assert.NotNull(data.Heatmap.InProgress);
        Assert.NotNull(data.Heatmap.Carryover);
        Assert.NotNull(data.Heatmap.Blockers);
    }

    [Fact]
    public void DashboardData_Deserialize_ValidJson_PopulatesAllFields()
    {
        var json = """
        {
            "title": "Test Project",
            "subtitle": "Team A · Stream B · April 2026",
            "backlogLink": "https://dev.azure.com/test",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "name": "M1",
                        "label": "Core",
                        "color": "#4285F4",
                        "milestones": [
                            { "date": "2026-02-14", "type": "poc", "label": "PoC" }
                        ]
                    }
                ]
            },
            "heatmap": {
                "shipped": { "Jan": ["Item A"], "Feb": [] },
                "inProgress": { "Apr": ["Item B"] },
                "carryover": {},
                "blockers": { "Apr": ["Blocker 1"] }
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal("Test Project", data!.Title);
        Assert.Equal("Team A · Stream B · April 2026", data.Subtitle);
        Assert.Equal("https://dev.azure.com/test", data.BacklogLink);
        Assert.Equal("Apr", data.CurrentMonth);
        Assert.Equal(4, data.Months.Count);
        Assert.Single(data.Timeline.Tracks);
        Assert.Equal("M1", data.Timeline.Tracks[0].Name);
        Assert.Single(data.Timeline.Tracks[0].Milestones);
        Assert.Equal("poc", data.Timeline.Tracks[0].Milestones[0].Type);
        Assert.Single(data.Heatmap.Shipped["Jan"]);
        Assert.Single(data.Heatmap.Blockers["Apr"]);
    }

    [Fact]
    public void TimelineTrack_DefaultColor_IsGray()
    {
        var track = new TimelineTrack();

        Assert.Equal("#999", track.Color);
    }

    [Fact]
    public void Milestone_DefaultType_IsCheckpoint()
    {
        var milestone = new Milestone();

        Assert.Equal("checkpoint", milestone.Type);
    }

    [Fact]
    public void HeatmapData_Deserialize_EmptyCategories_ReturnsEmptyDictionaries()
    {
        var json = """{ "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }""";

        var data = JsonSerializer.Deserialize<HeatmapData>(json);

        Assert.NotNull(data);
        Assert.Empty(data!.Shipped);
        Assert.Empty(data.InProgress);
        Assert.Empty(data.Carryover);
        Assert.Empty(data.Blockers);
    }

    [Fact]
    public void DashboardData_Deserialize_ExtraFields_Ignored()
    {
        var json = """{ "title": "Test", "unknownField": 42, "anotherExtra": "hello" }""";

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal("Test", data!.Title);
    }

    [Fact]
    public void DashboardData_Deserialize_PartialJson_DefaultsApplied()
    {
        var json = """{ "title": "Partial" }""";

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal("Partial", data!.Title);
        Assert.Equal("", data.Subtitle);
        Assert.Empty(data.Months);
        Assert.NotNull(data.Timeline);
        Assert.NotNull(data.Heatmap);
    }
}