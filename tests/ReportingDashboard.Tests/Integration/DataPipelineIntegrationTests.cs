using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for the full data pipeline: JSON file on disk → DashboardDataService
/// → deeply nested model verification. Covers minified JSON, extra fields, UTF-8 BOM,
/// large datasets, and the actual wwwroot/data.json sample file.
/// </summary>
[Trait("Category", "Integration")]
public class DataPipelineIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public DataPipelineIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DataPipe_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteJson(string json)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    private DashboardDataService CreateService() => new(_logger);

    private string CreateCompleteJson() => JsonSerializer.Serialize(new
    {
        title = "Full Pipeline Test",
        subtitle = "Integration Team - April 2026",
        backlogLink = "https://dev.azure.com/org/project/_backlogs",
        currentMonth = "Apr",
        months = new[] { "Jan", "Feb", "Mar", "Apr" },
        timeline = new
        {
            startDate = "2026-01-01",
            endDate = "2026-06-30",
            nowDate = "2026-04-10",
            tracks = new[]
            {
                new
                {
                    name = "M1",
                    label = "Chatbot & MS Role",
                    color = "#0078D4",
                    milestones = new[]
                    {
                        new { date = "2026-01-12", type = "checkpoint", label = "Jan 12 Check" },
                        new { date = "2026-03-26", type = "poc", label = "Mar 26 PoC" },
                        new { date = "2026-05-01", type = "production", label = "May 1 GA" }
                    }
                },
                new
                {
                    name = "M2",
                    label = "Privacy Policy Engine",
                    color = "#00897B",
                    milestones = new[]
                    {
                        new { date = "2026-02-05", type = "checkpoint", label = "Feb 5 Check" },
                        new { date = "2026-04-15", type = "poc", label = "Apr 15 PoC" }
                    }
                },
                new
                {
                    name = "M3",
                    label = "Audit & Compliance",
                    color = "#546E7A",
                    milestones = new[]
                    {
                        new { date = "2026-01-20", type = "checkpoint", label = "Jan 20 Check" },
                        new { date = "2026-03-01", type = "checkpoint", label = "Mar 1 Check" }
                    }
                }
            }
        },
        heatmap = new
        {
            shipped = new Dictionary<string, string[]>
            {
                ["jan"] = new[] { "Auth Module", "CI Pipeline" },
                ["feb"] = new[] { "Search Feature", "Dashboard v1" },
                ["mar"] = new[] { "Report Builder" }
            },
            inProgress = new Dictionary<string, string[]>
            {
                ["mar"] = new[] { "Analytics Engine" },
                ["apr"] = new[] { "Export API", "Bulk Operations" }
            },
            carryover = new Dictionary<string, string[]>
            {
                ["apr"] = new[] { "Legacy Migration" }
            },
            blockers = new Dictionary<string, string[]>
            {
                ["mar"] = new[] { "Vendor Dependency" },
                ["apr"] = new[] { "License Review" }
            }
        }
    }, JsonOpts);

    #region Full Nested Data Verification

    [Fact]
    public async Task Pipeline_CompleteJson_AllTracksDeserialized()
    {
        var path = WriteJson(CreateCompleteJson());
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Equal(3, svc.Data!.Timeline.Tracks.Count);
        Assert.Equal("M1", svc.Data.Timeline.Tracks[0].Name);
        Assert.Equal("M2", svc.Data.Timeline.Tracks[1].Name);
        Assert.Equal("M3", svc.Data.Timeline.Tracks[2].Name);
    }

    [Fact]
    public async Task Pipeline_CompleteJson_MilestoneCountsCorrect()
    {
        var path = WriteJson(CreateCompleteJson());
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Equal(3, svc.Data!.Timeline.Tracks[0].Milestones.Count);
        Assert.Equal(2, svc.Data.Timeline.Tracks[1].Milestones.Count);
        Assert.Equal(2, svc.Data.Timeline.Tracks[2].Milestones.Count);
    }

    [Fact]
    public async Task Pipeline_CompleteJson_MilestoneTypesPreserved()
    {
        var path = WriteJson(CreateCompleteJson());
        var svc = CreateService();

        await svc.LoadAsync(path);

        var m1 = svc.Data!.Timeline.Tracks[0].Milestones;
        Assert.Equal("checkpoint", m1[0].Type);
        Assert.Equal("poc", m1[1].Type);
        Assert.Equal("production", m1[2].Type);
    }

    [Fact]
    public async Task Pipeline_CompleteJson_MilestoneDatesPreserved()
    {
        var path = WriteJson(CreateCompleteJson());
        var svc = CreateService();

        await svc.LoadAsync(path);

        var m1 = svc.Data!.Timeline.Tracks[0].Milestones;
        Assert.Equal("2026-01-12", m1[0].Date);
        Assert.Equal("2026-03-26", m1[1].Date);
        Assert.Equal("2026-05-01", m1[2].Date);
    }

    [Fact]
    public async Task Pipeline_CompleteJson_TrackColorsPreserved()
    {
        var path = WriteJson(CreateCompleteJson());
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Equal("#0078D4", svc.Data!.Timeline.Tracks[0].Color);
        Assert.Equal("#00897B", svc.Data.Timeline.Tracks[1].Color);
        Assert.Equal("#546E7A", svc.Data.Timeline.Tracks[2].Color);
    }

    [Fact]
    public async Task Pipeline_CompleteJson_AllHeatmapCategoriesPopulated()
    {
        var path = WriteJson(CreateCompleteJson());
        var svc = CreateService();

        await svc.LoadAsync(path);

        var hm = svc.Data!.Heatmap;
        Assert.Equal(3, hm.Shipped.Count);
        Assert.Equal(2, hm.InProgress.Count);
        Assert.Single(hm.Carryover);
        Assert.Equal(2, hm.Blockers.Count);
    }

    [Fact]
    public async Task Pipeline_CompleteJson_ShippedItemCountsCorrect()
    {
        var path = WriteJson(CreateCompleteJson());
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Equal(2, svc.Data!.Heatmap.Shipped["jan"].Count);
        Assert.Equal(2, svc.Data.Heatmap.Shipped["feb"].Count);
        Assert.Single(svc.Data.Heatmap.Shipped["mar"]);
    }

    [Fact]
    public async Task Pipeline_CompleteJson_NowDatePreserved()
    {
        var path = WriteJson(CreateCompleteJson());
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Equal("2026-04-10", svc.Data!.Timeline.NowDate);
    }

    #endregion

    #region Minified JSON

    [Fact]
    public async Task Pipeline_MinifiedJson_ParsesSuccessfully()
    {
        var json = """{"title":"T","subtitle":"S","backlogLink":"https://link","currentMonth":"Apr","months":["Apr"],"timeline":{"startDate":"2026-01-01","endDate":"2026-07-01","nowDate":"2026-04-10","tracks":[{"name":"M1","label":"L","color":"#000","milestones":[{"date":"2026-03-15","type":"poc","label":"PoC"}]}]},"heatmap":{"shipped":{"jan":["A","B"]},"inProgress":{},"carryover":{},"blockers":{}}}""";
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Equal("T", svc.Data!.Title);
        Assert.Equal(2, svc.Data.Heatmap.Shipped["jan"].Count);
        Assert.Equal("poc", svc.Data.Timeline.Tracks[0].Milestones[0].Type);
    }

    #endregion

    #region Extra Unknown Fields

    [Fact]
    public async Task Pipeline_ExtraFields_IgnoredGracefully()
    {
        var json = """
        {
            "title": "T",
            "subtitle": "S",
            "backlogLink": "https://link",
            "currentMonth": "Apr",
            "unknownTopLevel": "ignored",
            "extraNumber": 42,
            "months": ["Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "extraTimelineField": true,
                "tracks": [{
                    "name": "M1",
                    "label": "L",
                    "color": "#000",
                    "extraTrackField": "ignored",
                    "milestones": [{
                        "date": "2026-03-15",
                        "type": "poc",
                        "label": "PoC",
                        "extraMilestoneField": 99
                    }]
                }]
            },
            "heatmap": {
                "shipped": {},
                "inProgress": {},
                "carryover": {},
                "blockers": {},
                "extraCategory": {"jan": ["should be ignored"]}
            }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("T", svc.Data!.Title);
        Assert.Equal("poc", svc.Data.Timeline.Tracks[0].Milestones[0].Type);
    }

    #endregion

    #region UTF-8 BOM

    [Fact]
    public async Task Pipeline_Utf8Bom_ParsesSuccessfully()
    {
        var json = CreateCompleteJson();
        var path = Path.Combine(_tempDir, $"bom_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        var svc = CreateService();
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Equal("Full Pipeline Test", svc.Data!.Title);
    }

    #endregion

    #region Unicode Content

    [Fact]
    public async Task Pipeline_UnicodeInAllFields_PreservedCorrectly()
    {
        var json = JsonSerializer.Serialize(new
        {
            title = "Équipe développement – Dashboard",
            subtitle = "日本語チーム — April 2026",
            backlogLink = "https://dev.azure.com/org/项目",
            currentMonth = "四月",
            months = new[] { "四月" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "テスト", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]> { ["四月"] = new[] { "功能Ä" } },
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, JsonOpts);
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("Équipe développement – Dashboard", svc.Data!.Title);
        Assert.Equal("日本語チーム — April 2026", svc.Data.Subtitle);
        Assert.Equal("テスト", svc.Data.Timeline.Tracks[0].Label);
        Assert.Contains("四月", svc.Data.Heatmap.Shipped.Keys);
    }

    #endregion

    #region Large Dataset

    [Fact]
    public async Task Pipeline_LargeDataset_LoadsSuccessfully()
    {
        var tracks = Enumerable.Range(1, 10).Select(i => new
        {
            name = $"M{i}",
            label = $"Track {i} with a longer descriptive name",
            color = $"#{i:X2}{i:X2}{i:X2}",
            milestones = Enumerable.Range(1, 5).Select(j => new
            {
                date = $"2026-{(j % 6 + 1):D2}-{(j * 5 % 28 + 1):D2}",
                type = j % 3 == 0 ? "production" : j % 2 == 0 ? "poc" : "checkpoint",
                label = $"Milestone {i}.{j}"
            }).ToArray()
        }).ToArray();

        var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
        var shipped = new Dictionary<string, string[]>();
        foreach (var m in months)
        {
            shipped[m.ToLower()] = Enumerable.Range(1, 5).Select(i => $"Shipped {m} #{i}").ToArray();
        }

        var json = JsonSerializer.Serialize(new
        {
            title = "Large Dataset",
            subtitle = "Stress Test",
            backlogLink = "https://link",
            currentMonth = "Apr",
            months,
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks
            },
            heatmap = new
            {
                shipped,
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, JsonOpts);
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(10, svc.Data!.Timeline.Tracks.Count);
        Assert.Equal(5, svc.Data.Timeline.Tracks[0].Milestones.Count);
        Assert.Equal(6, svc.Data.Heatmap.Shipped.Count);
        Assert.Equal(5, svc.Data.Heatmap.Shipped["jan"].Count);
    }

    #endregion

    #region Sequential Loads with Full Data Replacement

    [Fact]
    public async Task Pipeline_SequentialLoads_FullyReplaceData()
    {
        var svc = CreateService();

        // Load first dataset
        var json1 = JsonSerializer.Serialize(new
        {
            title = "Version 1",
            subtitle = "Sub 1",
            backlogLink = "https://link1",
            currentMonth = "Jan",
            months = new[] { "Jan" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                tracks = new[] { new { name = "M1", label = "Track 1", color = "#111", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]> { ["jan"] = new[] { "Item A" } },
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, JsonOpts);
        var path1 = WriteJson(json1);
        await svc.LoadAsync(path1);

        Assert.Equal("Version 1", svc.Data!.Title);
        Assert.Equal("Track 1", svc.Data.Timeline.Tracks[0].Label);
        Assert.Contains("jan", svc.Data.Heatmap.Shipped.Keys);

        // Load second dataset - completely different data
        var json2 = JsonSerializer.Serialize(new
        {
            title = "Version 2",
            subtitle = "Sub 2",
            backlogLink = "https://link2",
            currentMonth = "Mar",
            months = new[] { "Feb", "Mar" },
            timeline = new
            {
                startDate = "2026-02-01",
                endDate = "2026-08-01",
                tracks = new[]
                {
                    new { name = "X1", label = "New Track A", color = "#AAA", milestones = Array.Empty<object>() },
                    new { name = "X2", label = "New Track B", color = "#BBB", milestones = Array.Empty<object>() }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]> { ["mar"] = new[] { "New Item" } },
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, JsonOpts);
        var path2 = WriteJson(json2);
        await svc.LoadAsync(path2);

        Assert.Equal("Version 2", svc.Data!.Title);
        Assert.Equal("Sub 2", svc.Data.Subtitle);
        Assert.Equal(2, svc.Data.Timeline.Tracks.Count);
        Assert.Equal("X1", svc.Data.Timeline.Tracks[0].Name);
        Assert.Empty(svc.Data.Heatmap.Shipped);
        Assert.Contains("mar", svc.Data.Heatmap.InProgress.Keys);
    }

    #endregion

    #region Case-Insensitive Property Deserialization

    [Fact]
    public async Task Pipeline_MixedCasePropertyNames_DeserializesCorrectly()
    {
        // Use PascalCase property names (service uses PropertyNameCaseInsensitive = true)
        var json = """
        {
            "Title": "PascalCase Title",
            "Subtitle": "PascalCase Sub",
            "BacklogLink": "https://link",
            "CurrentMonth": "Apr",
            "Months": ["Apr"],
            "Timeline": {
                "StartDate": "2026-01-01",
                "EndDate": "2026-07-01",
                "NowDate": "2026-04-10",
                "Tracks": [{
                    "Name": "M1",
                    "Label": "L",
                    "Color": "#000",
                    "Milestones": []
                }]
            },
            "Heatmap": {
                "Shipped": {},
                "InProgress": {},
                "Carryover": {},
                "Blockers": {}
            }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("PascalCase Title", svc.Data!.Title);
    }

    #endregion

    #region Empty Heatmap Categories

    [Fact]
    public async Task Pipeline_EmptyHeatmapCategories_DefaultToEmptyDicts()
    {
        var json = JsonSerializer.Serialize(new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "Apr",
            months = new[] { "Apr" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, JsonOpts);
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Empty(svc.Data!.Heatmap.Shipped);
        Assert.Empty(svc.Data.Heatmap.InProgress);
        Assert.Empty(svc.Data.Heatmap.Carryover);
        Assert.Empty(svc.Data.Heatmap.Blockers);
    }

    #endregion

    #region Tracks with Empty Milestones

    [Fact]
    public async Task Pipeline_TracksWithNoMilestones_LoadSuccessfully()
    {
        var json = JsonSerializer.Serialize(new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "Apr",
            months = new[] { "Apr" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                tracks = new[]
                {
                    new { name = "M1", label = "Empty Track", color = "#000", milestones = Array.Empty<object>() },
                    new { name = "M2", label = "Also Empty", color = "#111", milestones = Array.Empty<object>() }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, JsonOpts);
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(2, svc.Data!.Timeline.Tracks.Count);
        Assert.Empty(svc.Data.Timeline.Tracks[0].Milestones);
        Assert.Empty(svc.Data.Timeline.Tracks[1].Milestones);
    }

    #endregion
}