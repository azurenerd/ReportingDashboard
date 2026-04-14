using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

public class DashboardDataServiceTests : IDisposable
{
    private readonly DashboardDataService _service;
    private readonly string _tempDir;

    public DashboardDataServiceTests()
    {
        _service = new DashboardDataService(NullLogger<DashboardDataService>.Instance);
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteJson(string fileName, object data)
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, JsonSerializer.Serialize(data));
        return path;
    }

    private string WriteRawJson(string fileName, string json)
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, json);
        return path;
    }

    private static object CreateValidJsonObject() => new
    {
        title = "Privacy Automation Release Roadmap",
        subtitle = "Trusted Platform · Privacy Automation Workstream · April 2026",
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
                        new { date = "2026-01-15", type = "checkpoint", label = "Jan 15" },
                        new { date = "2026-03-26", type = "poc", label = "Mar 26 PoC" },
                        new { date = "2026-05-15", type = "production", label = "May Prod" }
                    }
                },
                new
                {
                    name = "M2",
                    label = "PDS Integration",
                    color = "#00897B",
                    milestones = new[]
                    {
                        new { date = "2026-02-05", type = "checkpoint", label = "Feb 5" },
                        new { date = "2026-04-15", type = "poc", label = "Apr 15 PoC" },
                        new { date = "2026-06-01", type = "production", label = "Jun Prod" }
                    }
                },
                new
                {
                    name = "M3",
                    label = "Auto Review Pipeline",
                    color = "#546E7A",
                    milestones = new[]
                    {
                        new { date = "2026-01-20", type = "checkpoint", label = "Jan 20" },
                        new { date = "2026-03-10", type = "poc", label = "Mar 10 PoC" },
                        new { date = "2026-05-15", type = "production", label = "May Prod" }
                    }
                }
            }
        },
        heatmap = new
        {
            shipped = new Dictionary<string, string[]>
            {
                ["jan"] = new[] { "Item A", "Item B" },
                ["feb"] = new[] { "Item C" },
                ["mar"] = new[] { "Item D" },
                ["apr"] = new[] { "Item E" }
            },
            inProgress = new Dictionary<string, string[]>
            {
                ["jan"] = Array.Empty<string>(),
                ["feb"] = Array.Empty<string>(),
                ["mar"] = new[] { "Item F" },
                ["apr"] = new[] { "Item G" }
            },
            carryover = new Dictionary<string, string[]>
            {
                ["jan"] = Array.Empty<string>(),
                ["feb"] = Array.Empty<string>(),
                ["mar"] = Array.Empty<string>(),
                ["apr"] = new[] { "Item H" }
            },
            blockers = new Dictionary<string, string[]>
            {
                ["jan"] = Array.Empty<string>(),
                ["feb"] = Array.Empty<string>(),
                ["mar"] = Array.Empty<string>(),
                ["apr"] = new[] { "Item I" }
            }
        }
    };

    [Fact]
    public async Task LoadAsync_ValidJson_PopulatesData()
    {
        var path = WriteJson("valid.json", CreateValidJsonObject());

        await _service.LoadAsync(path);

        Assert.False(_service.IsError);
        Assert.NotNull(_service.Data);
        Assert.Equal("Privacy Automation Release Roadmap", _service.Data!.Title);
        Assert.Equal(4, _service.Data.Months.Count);
        Assert.Equal(3, _service.Data.Timeline.Tracks.Count);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_SetsError()
    {
        var path = Path.Combine(_tempDir, "nonexistent.json");

        await _service.LoadAsync(path);

        Assert.True(_service.IsError);
        Assert.Contains("not found", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_SetsError()
    {
        var path = WriteRawJson("malformed.json", "{ invalid json }}");

        await _service.LoadAsync(path);

        Assert.True(_service.IsError);
        Assert.Contains("Failed to parse", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_EmptyArrays_DoesNotCrash()
    {
        var data = new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "https://example.com",
            currentMonth = "Jan",
            months = Array.Empty<string>(),
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-06-30",
                nowDate = "2026-04-10",
                tracks = Array.Empty<object>()
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        };
        var path = WriteJson("empty-arrays.json", data);

        await _service.LoadAsync(path);

        Assert.True(_service.IsError);
        Assert.Contains("months", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_NullOptionalFields_DefaultsGracefully()
    {
        // JSON with heatmap missing carryover key entirely - model defaults to empty dict
        var json = """
        {
            "title": "Test Project",
            "subtitle": "Test Sub",
            "backlogLink": "https://example.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "name": "M1",
                        "label": "Track 1",
                        "color": "#0078D4",
                        "milestones": [
                            { "date": "2026-01-15", "type": "checkpoint", "label": "Jan 15" }
                        ]
                    }
                ]
            },
            "heatmap": {
                "shipped": {},
                "inProgress": {},
                "blockers": {}
            }
        }
        """;
        var path = WriteRawJson("null-optional.json", json);

        await _service.LoadAsync(path);

        Assert.False(_service.IsError);
        Assert.NotNull(_service.Data);
        Assert.Empty(_service.Data!.Heatmap.Carryover);
    }

    [Fact]
    public async Task LoadAsync_MissingTitle_ValidationError()
    {
        var data = new
        {
            title = "",
            subtitle = "Sub",
            backlogLink = "https://example.com",
            currentMonth = "Jan",
            months = new[] { "Jan" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-06-30",
                nowDate = "2026-04-10",
                tracks = new[]
                {
                    new
                    {
                        name = "M1", label = "Track", color = "#000",
                        milestones = new[] { new { date = "2026-01-15", type = "checkpoint", label = "Jan" } }
                    }
                }
            },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        };
        var path = WriteJson("no-title.json", data);

        await _service.LoadAsync(path);

        Assert.True(_service.IsError);
        Assert.Contains("title", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_InvalidMilestoneType_ValidationError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://example.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "name": "M1",
                        "label": "Track 1",
                        "color": "#000",
                        "milestones": [
                            { "date": "2026-01-15", "type": "unknown", "label": "Bad" }
                        ]
                    }
                ]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteRawJson("invalid-type.json", json);

        await _service.LoadAsync(path);

        Assert.True(_service.IsError);
        Assert.Contains("invalid", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_CurrentMonthNotInMonths_LoadsSuccessfully()
    {
        // Service does not currently validate currentMonth membership in months
        var data = new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "https://example.com",
            currentMonth = "Dec",
            months = new[] { "Jan", "Feb" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-06-30",
                nowDate = "2026-04-10",
                tracks = new[]
                {
                    new
                    {
                        name = "M1", label = "Track", color = "#000",
                        milestones = new[] { new { date = "2026-01-15", type = "checkpoint", label = "Jan" } }
                    }
                }
            },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        };
        var path = WriteJson("bad-current-month.json", data);

        await _service.LoadAsync(path);

        Assert.False(_service.IsError);
        Assert.NotNull(_service.Data);
    }

    [Fact]
    public async Task LoadAsync_EndDateBeforeStartDate_LoadsSuccessfully()
    {
        // Service does not currently validate endDate > startDate ordering
        var data = new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "https://example.com",
            currentMonth = "Jan",
            months = new[] { "Jan" },
            timeline = new
            {
                startDate = "2026-06-30",
                endDate = "2026-01-01",
                nowDate = "2026-04-10",
                tracks = new[]
                {
                    new
                    {
                        name = "M1", label = "Track", color = "#000",
                        milestones = new[] { new { date = "2026-01-15", type = "checkpoint", label = "Jan" } }
                    }
                }
            },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        };
        var path = WriteJson("bad-dates.json", data);

        await _service.LoadAsync(path);

        Assert.False(_service.IsError);
        Assert.NotNull(_service.Data);
    }

    [Fact]
    public async Task LoadAsync_PerformanceUnder100ms()
    {
        // Build a ~50KB JSON with many heatmap items
        var shippedItems = new Dictionary<string, string[]>();
        for (int m = 1; m <= 12; m++)
        {
            var monthKey = new DateTime(2026, m, 1).ToString("MMM").ToLower();
            var items = new string[50];
            for (int i = 0; i < 50; i++)
                items[i] = $"Shipped work item {m}-{i} with enough text to pad the file size up";
            shippedItems[monthKey] = items;
        }

        var data = new
        {
            title = "Performance Test Project",
            subtitle = "Perf Test Sub",
            backlogLink = "https://example.com",
            currentMonth = "Jan",
            months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-12-31",
                nowDate = "2026-06-15",
                tracks = new[]
                {
                    new
                    {
                        name = "M1", label = "Track", color = "#000",
                        milestones = new[] { new { date = "2026-06-15", type = "checkpoint", label = "Mid" } }
                    }
                }
            },
            heatmap = new
            {
                shipped = shippedItems,
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        };
        var path = WriteJson("large.json", data);

        var fileSize = new FileInfo(path).Length;
        Assert.True(fileSize > 30_000, $"Test file should be large enough for a meaningful perf test, was {fileSize} bytes");

        var sw = Stopwatch.StartNew();
        await _service.LoadAsync(path);
        sw.Stop();

        Assert.False(_service.IsError, _service.ErrorMessage);
        Assert.True(sw.ElapsedMilliseconds < 100, $"LoadAsync took {sw.ElapsedMilliseconds}ms, expected < 100ms");
    }
}