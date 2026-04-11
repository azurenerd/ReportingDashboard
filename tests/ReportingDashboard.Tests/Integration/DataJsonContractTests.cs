using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Tests the contract between data.json structure and the DashboardDataService deserialization.
/// Verifies various JSON shapes are handled correctly end-to-end.
/// </summary>
[Trait("Category", "Integration")]
public class DataJsonContractTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    public DataJsonContractTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"JsonContract_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private async Task<DashboardDataService> LoadJson(string json)
    {
        var path = Path.Combine(_tempDir, $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, json);
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);
        return svc;
    }

    [Fact]
    public async Task Contract_MinimalValidJson_Loads()
    {
        var json = """
        {
            "title": "Min",
            "subtitle": "Sub",
            "currentMonth": "Jan",
            "months": ["January"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-04-01",
                "tracks": [{"name":"M1","label":"T","color":"#000","milestones":[]}]
            },
            "heatmap": {"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}
        }
        """;

        var svc = await LoadJson(json);

        Assert.False(svc.IsError);
        Assert.Equal("Min", svc.Data!.Title);
    }

    [Fact]
    public async Task Contract_CamelCasePropertyNames_Deserialize()
    {
        var json = """
        {
            "title": "Camel",
            "subtitle": "Case Test",
            "backlogLink": "https://test.com",
            "currentMonth": "Feb",
            "months": ["February"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-02-15",
                "tracks": [{"name":"T1","label":"Track","color":"#111","milestones":[
                    {"date":"2026-03-01","type":"poc","label":"PoC"}
                ]}]
            },
            "heatmap": {
                "shipped": {"feb": ["Item1"]},
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;

        var svc = await LoadJson(json);

        Assert.False(svc.IsError);
        Assert.Equal("https://test.com", svc.Data!.BacklogLink);
        Assert.Equal("poc", svc.Data.Timeline.Tracks[0].Milestones[0].Type);
        Assert.Single(svc.Data.Heatmap.Shipped["feb"]);
    }

    [Fact]
    public async Task Contract_CaseInsensitivePropertyNames_Deserialize()
    {
        // Test that PropertyNameCaseInsensitive works
        var json = """
        {
            "Title": "Upper",
            "Subtitle": "Case",
            "CurrentMonth": "Mar",
            "Months": ["March"],
            "Timeline": {
                "StartDate": "2026-01-01",
                "EndDate": "2026-07-01",
                "NowDate": "2026-03-15",
                "Tracks": [{"Name":"M1","Label":"T","Color":"#000","Milestones":[]}]
            },
            "Heatmap": {"Shipped":{},"InProgress":{},"Carryover":{},"Blockers":{}}
        }
        """;

        var svc = await LoadJson(json);

        Assert.False(svc.IsError);
        Assert.Equal("Upper", svc.Data!.Title);
    }

    [Fact]
    public async Task Contract_EmptyHeatmapCategories_ValidStructure()
    {
        var json = """
        {
            "title": "Empty Heatmap",
            "subtitle": "No items",
            "currentMonth": "Jan",
            "months": ["January"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-01-15",
                "tracks": [{"name":"M1","label":"T","color":"#000","milestones":[]}]
            },
            "heatmap": {
                "shipped": {},
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;

        var svc = await LoadJson(json);

        Assert.False(svc.IsError);
        Assert.Empty(svc.Data!.Heatmap.Shipped);
        Assert.Empty(svc.Data.Heatmap.InProgress);
        Assert.Empty(svc.Data.Heatmap.Carryover);
        Assert.Empty(svc.Data.Heatmap.Blockers);
    }

    [Fact]
    public async Task Contract_MultipleMonthsWithData_AllPreserved()
    {
        var json = """
        {
            "title": "Multi Month",
            "subtitle": "Full year",
            "currentMonth": "June",
            "months": ["January","February","March","April","May","June"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-06-15",
                "tracks": [{"name":"M1","label":"T","color":"#000","milestones":[]}]
            },
            "heatmap": {
                "shipped": {
                    "jan": ["A"],
                    "feb": ["B","C"],
                    "mar": ["D"],
                    "apr": ["E","F","G"],
                    "may": ["H"],
                    "jun": ["I"]
                },
                "inProgress": {"jun":["J","K"]},
                "carryover": {"mar":["L"],"apr":["M"]},
                "blockers": {"feb":["N"]}
            }
        }
        """;

        var svc = await LoadJson(json);

        Assert.False(svc.IsError);
        Assert.Equal(6, svc.Data!.Months.Count);
        Assert.Equal(6, svc.Data.Heatmap.Shipped.Count);
        Assert.Equal(3, svc.Data.Heatmap.Shipped["apr"].Count);
        Assert.Equal(2, svc.Data.Heatmap.InProgress["jun"].Count);
        Assert.Equal(2, svc.Data.Heatmap.Carryover.Count);
    }

    [Fact]
    public async Task Contract_AllMilestoneTypes_Recognized()
    {
        var json = """
        {
            "title": "Milestone Types",
            "subtitle": "All types",
            "currentMonth": "Apr",
            "months": ["April"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-04-01",
                "tracks": [{
                    "name":"M1","label":"T","color":"#000",
                    "milestones":[
                        {"date":"2026-02-01","type":"poc","label":"PoC"},
                        {"date":"2026-04-01","type":"production","label":"Prod"},
                        {"date":"2026-05-01","type":"checkpoint","label":"Check"},
                        {"date":"2026-06-01","type":"custom","label":"Custom"}
                    ]
                }]
            },
            "heatmap": {"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}
        }
        """;

        var svc = await LoadJson(json);

        Assert.False(svc.IsError);
        var milestones = svc.Data!.Timeline.Tracks[0].Milestones;
        Assert.Equal(4, milestones.Count);
        Assert.Equal("poc", milestones[0].Type);
        Assert.Equal("production", milestones[1].Type);
        Assert.Equal("checkpoint", milestones[2].Type);
        Assert.Equal("custom", milestones[3].Type);
    }

    [Fact]
    public async Task Contract_SpecialCharactersInStrings_Preserved()
    {
        var json = """
        {
            "title": "Project: Alpha & Beta <v2>",
            "subtitle": "Team \"Core\" — Q2'26",
            "currentMonth": "Apr",
            "months": ["April"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-04-01",
                "tracks": [{"name":"M1","label":"Track & Test","color":"#000","milestones":[]}]
            },
            "heatmap": {
                "shipped": {"apr": ["Feature <X>", "Bug fix & deploy"]},
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;

        var svc = await LoadJson(json);

        Assert.False(svc.IsError);
        Assert.Contains("Alpha & Beta", svc.Data!.Title);
        Assert.Contains("<v2>", svc.Data.Title);
        Assert.Contains("Feature <X>", svc.Data.Heatmap.Shipped["apr"]);
    }

    [Fact]
    public async Task Contract_UnicodeInStrings_Preserved()
    {
        var json = """
        {
            "title": "Проект Дашборд 🚀",
            "subtitle": "チーム - 四月",
            "currentMonth": "Apr",
            "months": ["April"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-04-01",
                "tracks": [{"name":"M1","label":"Трек","color":"#000","milestones":[]}]
            },
            "heatmap": {"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}
        }
        """;

        var svc = await LoadJson(json);

        Assert.False(svc.IsError);
        Assert.Contains("🚀", svc.Data!.Title);
        Assert.Contains("チーム", svc.Data.Subtitle);
    }

    [Fact]
    public async Task Contract_MissingOptionalBacklogLink_DefaultsToEmpty()
    {
        var json = """
        {
            "title": "No Link",
            "subtitle": "Sub",
            "currentMonth": "Jan",
            "months": ["January"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-01-15",
                "tracks": [{"name":"M1","label":"T","color":"#000","milestones":[]}]
            },
            "heatmap": {"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}
        }
        """;

        var svc = await LoadJson(json);

        Assert.False(svc.IsError);
        Assert.Equal(string.Empty, svc.Data!.BacklogLink);
    }

    [Fact]
    public async Task Contract_TrackWithNoMilestones_ValidStructure()
    {
        var json = """
        {
            "title": "Empty Track",
            "subtitle": "Sub",
            "currentMonth": "Jan",
            "months": ["January"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-01-15",
                "tracks": [
                    {"name":"M1","label":"Empty","color":"#111","milestones":[]},
                    {"name":"M2","label":"Also Empty","color":"#222","milestones":[]}
                ]
            },
            "heatmap": {"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}
        }
        """;

        var svc = await LoadJson(json);

        Assert.False(svc.IsError);
        Assert.Equal(2, svc.Data!.Timeline.Tracks.Count);
        Assert.Empty(svc.Data.Timeline.Tracks[0].Milestones);
        Assert.Empty(svc.Data.Timeline.Tracks[1].Milestones);
    }
}