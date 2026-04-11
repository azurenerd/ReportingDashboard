using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for DashboardDataService interacting with the real file system.
/// Covers file I/O edge cases, concurrent access, large files, encoding, and
/// end-to-end load→validate→expose pipeline scenarios not covered by existing tests.
/// </summary>
[Trait("Category", "Integration")]
public class DataServiceFileSystemIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public DataServiceFileSystemIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashFSInteg_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteFile(string content, string name = "data.json")
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    private string WriteJsonObject(object data, string name = "data.json")
    {
        return WriteFile(JsonSerializer.Serialize(data, JsonOpts), name);
    }

    private static object CreateValidData(string title = "Test Project") => new
    {
        title,
        subtitle = "Team - April 2026",
        backlogLink = "https://ado.example.com",
        currentMonth = "April",
        months = new[] { "January", "February", "March", "April" },
        timeline = new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            nowDate = "2026-04-10",
            tracks = new[]
            {
                new
                {
                    name = "M1", label = "Core", color = "#4285F4",
                    milestones = new[] { new { date = "2026-02-15", type = "poc", label = "PoC" } }
                }
            }
        },
        heatmap = new
        {
            shipped = new Dictionary<string, string[]> { ["jan"] = new[] { "Feature A" } },
            inProgress = new Dictionary<string, string[]>(),
            carryover = new Dictionary<string, string[]>(),
            blockers = new Dictionary<string, string[]>()
        }
    };

    #region File encoding edge cases

    [Fact]
    public async Task LoadAsync_Utf8WithBom_DeserializesCorrectly()
    {
        var json = JsonSerializer.Serialize(CreateValidData("BOM Test"), JsonOpts);
        var path = Path.Combine(_tempDir, "bom.json");
        await File.WriteAllTextAsync(path, json, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("BOM Test", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_UnicodeContent_PreservedInModel()
    {
        var data = CreateValidData("Privacy Automation — Release Roadmap™");
        var path = WriteJsonObject(data);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Contains("—", svc.Data!.Title);
        Assert.Contains("™", svc.Data.Title);
    }

    #endregion

    #region Large data scenarios

    [Fact]
    public async Task LoadAsync_ManyTracksAndMilestones_AllDeserializedCorrectly()
    {
        var tracks = Enumerable.Range(1, 20).Select(i => new
        {
            name = $"M{i}",
            label = $"Workstream {i}",
            color = $"#{i * 111111 % 0xFFFFFF:X6}",
            milestones = Enumerable.Range(1, 10).Select(m => new
            {
                date = $"2026-{m:D2}-15",
                type = m % 3 == 0 ? "production" : m % 2 == 0 ? "poc" : "checkpoint",
                label = $"MS {i}.{m}"
            }).ToArray()
        }).ToArray();

        var data = new
        {
            title = "Large Project",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan", "Feb", "Mar", "Apr" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-12-31", nowDate = "2026-04-10", tracks },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        };
        var path = WriteJsonObject(data);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(20, svc.Data!.Timeline.Tracks.Count);
        Assert.Equal(10, svc.Data.Timeline.Tracks[0].Milestones.Count);
        Assert.Equal(10, svc.Data.Timeline.Tracks[19].Milestones.Count);
    }

    [Fact]
    public async Task LoadAsync_LargeHeatmapData_AllCategoriesDeserialized()
    {
        var makeItems = (int count) => Enumerable.Range(1, count).Select(i => $"Work Item {i}").ToArray();
        var data = new
        {
            title = "Heatmap Stress",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
            timeline = new
            {
                startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>
                {
                    ["jan"] = makeItems(5), ["feb"] = makeItems(8), ["mar"] = makeItems(3),
                    ["apr"] = makeItems(10), ["may"] = makeItems(2), ["jun"] = makeItems(1)
                },
                inProgress = new Dictionary<string, string[]>
                {
                    ["apr"] = makeItems(6), ["may"] = makeItems(4)
                },
                carryover = new Dictionary<string, string[]>
                {
                    ["feb"] = makeItems(2), ["mar"] = makeItems(3)
                },
                blockers = new Dictionary<string, string[]>
                {
                    ["apr"] = makeItems(3)
                }
            }
        };
        var path = WriteJsonObject(data);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(6, svc.Data!.Heatmap.Shipped.Count);
        Assert.Equal(10, svc.Data.Heatmap.Shipped["apr"].Count);
        Assert.Equal(2, svc.Data.Heatmap.InProgress.Count);
        Assert.Equal(2, svc.Data.Heatmap.Carryover.Count);
        Assert.Single(svc.Data.Heatmap.Blockers);
    }

    #endregion

    #region Multiple sequential loads (state machine transitions)

    [Fact]
    public async Task LoadAsync_SequentialLoads_EachLoadFullyReplacesState()
    {
        var svc = new DashboardDataService(_logger);

        // Load 1: valid
        var path1 = WriteJsonObject(CreateValidData("Version 1"), "v1.json");
        await svc.LoadAsync(path1);
        Assert.False(svc.IsError);
        Assert.Equal("Version 1", svc.Data!.Title);

        // Load 2: different valid file
        var path2 = WriteJsonObject(CreateValidData("Version 2"), "v2.json");
        await svc.LoadAsync(path2);
        Assert.False(svc.IsError);
        Assert.Equal("Version 2", svc.Data!.Title);

        // Load 3: error
        await svc.LoadAsync(Path.Combine(_tempDir, "gone.json"));
        Assert.True(svc.IsError);
        Assert.Null(svc.Data);

        // Load 4: recover
        var path4 = WriteJsonObject(CreateValidData("Version 4"), "v4.json");
        await svc.LoadAsync(path4);
        Assert.False(svc.IsError);
        Assert.Equal("Version 4", svc.Data!.Title);
        Assert.Null(svc.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_ErrorToMalformedToValid_TransitionsCorrectly()
    {
        var svc = new DashboardDataService(_logger);

        // File not found
        await svc.LoadAsync(Path.Combine(_tempDir, "x.json"));
        Assert.True(svc.IsError);
        Assert.Contains("not found", svc.ErrorMessage!);

        // Malformed JSON
        var badPath = WriteFile("{{{bad}}}", "bad.json");
        await svc.LoadAsync(badPath);
        Assert.True(svc.IsError);
        Assert.Contains("parse", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);

        // Validation error (empty required fields)
        var emptyPath = WriteFile("{}", "empty.json");
        await svc.LoadAsync(emptyPath);
        Assert.True(svc.IsError);
        Assert.Contains("validation", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);

        // Valid
        var validPath = WriteJsonObject(CreateValidData("Recovery"), "ok.json");
        await svc.LoadAsync(validPath);
        Assert.False(svc.IsError);
        Assert.Equal("Recovery", svc.Data!.Title);
    }

    #endregion

    #region Path edge cases

    [Fact]
    public async Task LoadAsync_PathWithSpaces_LoadsSuccessfully()
    {
        var spacedDir = Path.Combine(_tempDir, "path with spaces");
        Directory.CreateDirectory(spacedDir);
        var json = JsonSerializer.Serialize(CreateValidData("Spaced Path"), JsonOpts);
        var path = Path.Combine(spacedDir, "data.json");
        File.WriteAllText(path, json);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("Spaced Path", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_DeepNestedPath_LoadsSuccessfully()
    {
        var deepDir = Path.Combine(_tempDir, "a", "b", "c", "d", "e");
        Directory.CreateDirectory(deepDir);
        var json = JsonSerializer.Serialize(CreateValidData("Deep Path"), JsonOpts);
        var path = Path.Combine(deepDir, "data.json");
        File.WriteAllText(path, json);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("Deep Path", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_EmptyStringPath_SetsError()
    {
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync("");

        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
    }

    #endregion

    #region JSON structural edge cases

    [Fact]
    public async Task LoadAsync_ExtraUnrecognizedFields_IgnoredGracefully()
    {
        var json = """
        {
            "title": "Extra Fields",
            "subtitle": "Sub",
            "backlogLink": "https://link",
            "currentMonth": "April",
            "months": ["Jan"],
            "futureFeature": true,
            "nestedUnknown": { "a": 1, "b": [1,2,3] },
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-04-10",
                "extraTimelineField": "ignored",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [], "extraTrackField": 42 }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {}, "extraHeatmap": "ignored" }
        }
        """;
        var path = WriteFile(json);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("Extra Fields", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_MinifiedJson_ParsesCorrectly()
    {
        var data = CreateValidData("Minified");
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
        var path = WriteFile(json);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("Minified", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_JsonWithComments_SetsError()
    {
        // Standard JSON does not allow comments; System.Text.Json should reject
        var json = """
        {
            // This is a comment
            "title": "Comments"
        }
        """;
        var path = WriteFile(json);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_TrailingComma_SetsError()
    {
        var json = """
        {
            "title": "Trailing",
            "subtitle": "Sub",
        }
        """;
        var path = WriteFile(json);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    #endregion

    #region Concurrent access (same service instance)

    [Fact]
    public async Task LoadAsync_ConcurrentLoads_DoNotCorruptState()
    {
        // Write multiple valid files
        var paths = Enumerable.Range(1, 5).Select(i =>
        {
            return WriteJsonObject(CreateValidData($"Concurrent {i}"), $"c{i}.json");
        }).ToArray();

        var svc = new DashboardDataService(_logger);

        // Fire all loads concurrently
        var tasks = paths.Select(p => svc.LoadAsync(p)).ToArray();
        await Task.WhenAll(tasks);

        // Final state should be consistent (one of the valid loads won)
        // Key invariant: either IsError=false with Data!=null, or IsError=true with Data=null
        if (!svc.IsError)
        {
            Assert.NotNull(svc.Data);
            Assert.StartsWith("Concurrent", svc.Data!.Title);
        }
        else
        {
            Assert.Null(svc.Data);
        }
    }

    #endregion

    #region Data integrity - full pipeline verification

    [Fact]
    public async Task LoadAsync_CompleteDataset_AllNestedFieldsAccessible()
    {
        var data = new
        {
            title = "Full Pipeline",
            subtitle = "Team X - April 2026",
            backlogLink = "https://dev.azure.com/org/project/_backlogs",
            currentMonth = "April",
            months = new[] { "January", "February", "March", "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-06-30",
                nowDate = "2026-04-10",
                tracks = new[]
                {
                    new
                    {
                        name = "M1", label = "Core Platform", color = "#0078D4",
                        milestones = new[]
                        {
                            new { date = "2026-02-15", type = "poc", label = "Feb PoC" },
                            new { date = "2026-04-01", type = "production", label = "Apr GA" },
                            new { date = "2026-03-01", type = "checkpoint", label = "Mar Check" }
                        }
                    },
                    new
                    {
                        name = "M2", label = "Data Pipeline", color = "#00897B",
                        milestones = new[]
                        {
                            new { date = "2026-03-15", type = "poc", label = "Mar PoC" },
                            new { date = "2026-05-15", type = "production", label = "May GA" }
                        }
                    },
                    new
                    {
                        name = "M3", label = "Analytics", color = "#546E7A",
                        milestones = new[]
                        {
                            new { date = "2026-04-01", type = "checkpoint", label = "Apr Check" }
                        }
                    }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>
                {
                    ["jan"] = new[] { "Auth Module", "CI Pipeline" },
                    ["feb"] = new[] { "Search Feature" },
                    ["mar"] = new[] { "Dashboard v1" },
                    ["apr"] = Array.Empty<string>()
                },
                inProgress = new Dictionary<string, string[]>
                {
                    ["apr"] = new[] { "Analytics Engine", "Export API" }
                },
                carryover = new Dictionary<string, string[]>
                {
                    ["mar"] = new[] { "Legacy Migration" }
                },
                blockers = new Dictionary<string, string[]>
                {
                    ["apr"] = new[] { "Vendor License Delay" }
                }
            }
        };
        var path = WriteJsonObject(data);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Null(svc.ErrorMessage);

        var d = svc.Data!;
        Assert.Equal("Full Pipeline", d.Title);
        Assert.Equal("Team X - April 2026", d.Subtitle);
        Assert.Equal("https://dev.azure.com/org/project/_backlogs", d.BacklogLink);
        Assert.Equal("April", d.CurrentMonth);
        Assert.Equal(4, d.Months.Count);

        // Timeline
        Assert.Equal("2026-01-01", d.Timeline.StartDate);
        Assert.Equal("2026-06-30", d.Timeline.EndDate);
        Assert.Equal("2026-04-10", d.Timeline.NowDate);
        Assert.Equal(3, d.Timeline.Tracks.Count);

        // Track 1
        Assert.Equal("M1", d.Timeline.Tracks[0].Name);
        Assert.Equal("Core Platform", d.Timeline.Tracks[0].Label);
        Assert.Equal("#0078D4", d.Timeline.Tracks[0].Color);
        Assert.Equal(3, d.Timeline.Tracks[0].Milestones.Count);
        Assert.Equal("poc", d.Timeline.Tracks[0].Milestones[0].Type);
        Assert.Equal("production", d.Timeline.Tracks[0].Milestones[1].Type);
        Assert.Equal("checkpoint", d.Timeline.Tracks[0].Milestones[2].Type);

        // Track 2
        Assert.Equal("M2", d.Timeline.Tracks[1].Name);
        Assert.Equal(2, d.Timeline.Tracks[1].Milestones.Count);

        // Track 3
        Assert.Equal("M3", d.Timeline.Tracks[2].Name);
        Assert.Single(d.Timeline.Tracks[2].Milestones);

        // Heatmap
        Assert.Equal(4, d.Heatmap.Shipped.Count);
        Assert.Equal(2, d.Heatmap.Shipped["jan"].Count);
        Assert.Empty(d.Heatmap.Shipped["apr"]);
        Assert.Equal(2, d.Heatmap.InProgress["apr"].Count);
        Assert.Single(d.Heatmap.Carryover["mar"]);
        Assert.Single(d.Heatmap.Blockers["apr"]);
        Assert.Equal("Vendor License Delay", d.Heatmap.Blockers["apr"][0]);
    }

    #endregion

    #region File modification between loads

    [Fact]
    public async Task LoadAsync_FileModifiedBetweenLoads_ReadsLatestContent()
    {
        var path = WriteJsonObject(CreateValidData("Original"));
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);
        Assert.Equal("Original", svc.Data!.Title);

        // Modify file
        File.WriteAllText(path, JsonSerializer.Serialize(CreateValidData("Modified"), JsonOpts));
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("Modified", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_FileDeletedBetweenLoads_TransitionsToError()
    {
        var path = WriteJsonObject(CreateValidData("ToDelete"));
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);
        Assert.False(svc.IsError);

        File.Delete(path);
        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("not found", svc.ErrorMessage!);
        Assert.Null(svc.Data);
    }

    #endregion
}