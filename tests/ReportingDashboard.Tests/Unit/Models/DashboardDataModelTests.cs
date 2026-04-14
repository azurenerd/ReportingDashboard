using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Tests.Unit.Models;

public class DashboardDataModelTests
{
    [Fact]
    public void DashboardData_DefaultValues_CollectionsAreNotNull()
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
    public void DashboardData_DefaultValues_StringsAreEmpty()
    {
        var data = new DashboardData();

        Assert.Equal(string.Empty, data.Title);
        Assert.Equal(string.Empty, data.Subtitle);
        Assert.Equal(string.Empty, data.BacklogLink);
        Assert.Equal(string.Empty, data.CurrentMonth);
    }

    [Fact]
    public void DashboardData_Deserialize_ValidJson_ReturnsPopulatedModel()
    {
        var json = """
        {
            "title": "Test Project",
            "subtitle": "Team A \u00b7 Workstream B \u00b7 March 2026",
            "backlogLink": "https://dev.azure.com/org/project",
            "currentMonth": "Mar",
            "months": ["Jan", "Feb", "Mar"]
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal("Test Project", data.Title);
        Assert.Equal("https://dev.azure.com/org/project", data.BacklogLink);
        Assert.Equal("Mar", data.CurrentMonth);
        Assert.Equal(3, data.Months.Count);
        Assert.Equal("Feb", data.Months[1]);
    }

    [Fact]
    public void DashboardData_Deserialize_MissingOptionalFields_UsesDefaults()
    {
        var json = """{ "title": "Minimal" }""";

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal("Minimal", data.Title);
        Assert.Equal(string.Empty, data.Subtitle);
        Assert.Empty(data.Months);
        Assert.NotNull(data.Timeline);
        Assert.NotNull(data.Heatmap);
    }

    [Fact]
    public void TimelineData_Deserialize_WithTracks()
    {
        var json = """
        {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "name": "M1",
                    "label": "Feature Alpha",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-03-15", "type": "poc", "label": "Mar PoC" }
                    ]
                }
            ]
        }
        """;

        var timeline = JsonSerializer.Deserialize<TimelineData>(json);

        Assert.NotNull(timeline);
        Assert.Equal("2026-01-01", timeline.StartDate);
        Assert.Equal("2026-06-30", timeline.EndDate);
        Assert.Equal("2026-04-10", timeline.NowDate);
        Assert.Single(timeline.Tracks);
        Assert.Equal("M1", timeline.Tracks[0].Name);
        Assert.Equal("Feature Alpha", timeline.Tracks[0].Label);
        Assert.Equal("#0078D4", timeline.Tracks[0].Color);
        Assert.Single(timeline.Tracks[0].Milestones);
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
    public void Milestone_Deserialize_AllTypes()
    {
        var types = new[] { "checkpoint", "poc", "production" };

        foreach (var type in types)
        {
            var json = $$"""{ "date": "2026-01-01", "type": "{{type}}", "label": "Test" }""";
            var milestone = JsonSerializer.Deserialize<Milestone>(json);

            Assert.NotNull(milestone);
            Assert.Equal(type, milestone.Type);
        }
    }

    [Fact]
    public void HeatmapData_Deserialize_WithCategories()
    {
        var json = """
        {
            "shipped": {
                "jan": ["Item A", "Item B"],
                "feb": ["Item C"]
            },
            "inProgress": {
                "jan": [],
                "feb": ["Item D"]
            },
            "carryover": {
                "jan": [],
                "feb": []
            },
            "blockers": {
                "jan": [],
                "feb": ["Blocked Item"]
            }
        }
        """;

        var heatmap = JsonSerializer.Deserialize<HeatmapData>(json);

        Assert.NotNull(heatmap);
        Assert.Equal(2, heatmap.Shipped.Count);
        Assert.Equal(2, heatmap.Shipped["jan"].Count);
        Assert.Equal("Item A", heatmap.Shipped["jan"][0]);
        Assert.Single(heatmap.InProgress["feb"]);
        Assert.Empty(heatmap.Carryover["jan"]);
        Assert.Single(heatmap.Blockers["feb"]);
    }

    [Fact]
    public void HeatmapData_DefaultValues_DictionariesAreEmpty()
    {
        var heatmap = new HeatmapData();

        Assert.Empty(heatmap.Shipped);
        Assert.Empty(heatmap.InProgress);
        Assert.Empty(heatmap.Carryover);
        Assert.Empty(heatmap.Blockers);
    }

    [Fact]
    public void DashboardData_Deserialize_FullSampleJson()
    {
        var json = """
        {
            "title": "Privacy Automation Release Roadmap",
            "subtitle": "Trusted Platform \u00b7 Privacy Automation \u00b7 April 2026",
            "backlogLink": "https://dev.azure.com/org/project/_backlogs",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "name": "M1",
                        "label": "Chatbot & MS Role",
                        "color": "#0078D4",
                        "milestones": [
                            { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
                            { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
                            { "date": "2026-05-01", "type": "production", "label": "May Prod" }
                        ]
                    },
                    {
                        "name": "M2",
                        "label": "Compliance Engine",
                        "color": "#00897B",
                        "milestones": [
                            { "date": "2026-02-15", "type": "checkpoint", "label": "Feb 15" },
                            { "date": "2026-04-15", "type": "poc", "label": "Apr 15 PoC" }
                        ]
                    }
                ]
            },
            "heatmap": {
                "shipped": {
                    "jan": ["DSR v2 API endpoint"],
                    "feb": ["Chatbot classifier"],
                    "mar": ["MS Graph sync"],
                    "apr": ["Auto-classification v1"]
                },
                "inProgress": {
                    "jan": [],
                    "feb": [],
                    "mar": ["Dashboard wireframes"],
                    "apr": ["Chatbot hardening", "Dashboard build"]
                },
                "carryover": {
                    "jan": [],
                    "feb": [],
                    "mar": [],
                    "apr": ["Audit log perf fix"]
                },
                "blockers": {
                    "jan": [],
                    "feb": [],
                    "mar": ["Graph API throttling"],
                    "apr": ["Legal review pending"]
                }
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal("Privacy Automation Release Roadmap", data.Title);
        Assert.Equal("Apr", data.CurrentMonth);
        Assert.Equal(4, data.Months.Count);

        // Timeline assertions
        Assert.Equal("2026-01-01", data.Timeline.StartDate);
        Assert.Equal("2026-06-30", data.Timeline.EndDate);
        Assert.Equal("2026-04-10", data.Timeline.NowDate);
        Assert.Equal(2, data.Timeline.Tracks.Count);
        Assert.Equal("M1", data.Timeline.Tracks[0].Name);
        Assert.Equal("#0078D4", data.Timeline.Tracks[0].Color);
        Assert.Equal(3, data.Timeline.Tracks[0].Milestones.Count);
        Assert.Equal("poc", data.Timeline.Tracks[0].Milestones[1].Type);

        // Heatmap assertions
        Assert.Equal(4, data.Heatmap.Shipped.Count);
        Assert.Single(data.Heatmap.Shipped["jan"]);
        Assert.Equal(2, data.Heatmap.InProgress["apr"].Count);
        Assert.Empty(data.Heatmap.Carryover["jan"]);
        Assert.Single(data.Heatmap.Blockers["apr"]);
    }

    [Fact]
    public void DashboardData_Deserialize_EmptyJson_ReturnsDefaults()
    {
        var json = "{}";

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal(string.Empty, data.Title);
        Assert.Empty(data.Months);
        Assert.NotNull(data.Timeline);
        Assert.Empty(data.Timeline.Tracks);
        Assert.NotNull(data.Heatmap);
        Assert.Empty(data.Heatmap.Shipped);
    }

    [Fact]
    public void TimelineData_Deserialize_EmptyTracks()
    {
        var json = """
        {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": []
        }
        """;

        var timeline = JsonSerializer.Deserialize<TimelineData>(json);

        Assert.NotNull(timeline);
        Assert.Empty(timeline.Tracks);
    }

    [Fact]
    public void TimelineTrack_Deserialize_EmptyMilestones()
    {
        var json = """
        {
            "name": "M1",
            "label": "Empty Track",
            "color": "#0078D4",
            "milestones": []
        }
        """;

        var track = JsonSerializer.Deserialize<TimelineTrack>(json);

        Assert.NotNull(track);
        Assert.Equal("M1", track.Name);
        Assert.Empty(track.Milestones);
    }

    [Fact]
    public void HeatmapData_Deserialize_EmptyMonthArrays()
    {
        var json = """
        {
            "shipped": { "jan": [] },
            "inProgress": { "jan": [] },
            "carryover": { "jan": [] },
            "blockers": { "jan": [] }
        }
        """;

        var heatmap = JsonSerializer.Deserialize<HeatmapData>(json);

        Assert.NotNull(heatmap);
        Assert.Single(heatmap.Shipped);
        Assert.Empty(heatmap.Shipped["jan"]);
    }
}