using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

/// <summary>
/// Tests for DashboardDataService behavior under concurrent and sequential load scenarios.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardDataServiceConcurrencyTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DashboardDataServiceConcurrencyTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashConcur_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteValidJson(string title = "Test")
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            title,
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, JsonOpts));
        return path;
    }

    [Fact]
    public async Task LoadAsync_MultipleSequentialLoads_LastWins()
    {
        var svc = new DashboardDataService(_logger);

        var path1 = WriteValidJson("First");
        var path2 = WriteValidJson("Second");
        var path3 = WriteValidJson("Third");

        await svc.LoadAsync(path1);
        Assert.Equal("First", svc.Data!.Title);

        await svc.LoadAsync(path2);
        Assert.Equal("Second", svc.Data!.Title);

        await svc.LoadAsync(path3);
        Assert.Equal("Third", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_ValidThenMissing_ErrorClearsData()
    {
        var svc = new DashboardDataService(_logger);
        var path = WriteValidJson("Good");

        await svc.LoadAsync(path);
        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);

        await svc.LoadAsync(Path.Combine(_tempDir, "gone.json"));
        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_MissingThenValid_RecoversFully()
    {
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(Path.Combine(_tempDir, "missing.json"));
        Assert.True(svc.IsError);

        var path = WriteValidJson("Recovered");
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Null(svc.ErrorMessage);
        Assert.NotNull(svc.Data);
        Assert.Equal("Recovered", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_ConcurrentLoads_DoNotThrow()
    {
        var svc = new DashboardDataService(_logger);
        var path = WriteValidJson("Concurrent");

        var tasks = Enumerable.Range(0, 10).Select(_ => svc.LoadAsync(path));
        await Task.WhenAll(tasks);

        // Service should be in a consistent state
        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
    }

    [Fact]
    public async Task Service_FreshInstance_DataIsNull()
    {
        var svc = new DashboardDataService(_logger);

        Assert.Null(svc.Data);
        Assert.False(svc.IsError);
        Assert.Null(svc.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_EmptyPath_SetsError()
    {
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync("");

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_WhitespaceTitle_ValidationError()
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            title = "   ",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
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
        }, JsonOpts));

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_NullTimeline_ValidationError()
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        // Manually craft JSON with null timeline
        File.WriteAllText(path, """
        {
            "title": "T",
            "subtitle": "S",
            "backlogLink": "https://link",
            "currentMonth": "April",
            "months": ["April"],
            "timeline": null,
            "heatmap": {"shipped":{}, "inProgress":{}, "carryover":{}, "blockers":{}}
        }
        """);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("timeline", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_EmptyTracks_ValidationError()
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, """
        {
            "title": "T",
            "subtitle": "S",
            "backlogLink": "https://link",
            "currentMonth": "April",
            "months": ["April"],
            "timeline": {"startDate": "2026-01-01", "endDate": "2026-07-01", "tracks": []},
            "heatmap": {"shipped":{}, "inProgress":{}, "carryover":{}, "blockers":{}}
        }
        """);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("tracks", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingTimelineStartDate_ValidationError()
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, """
        {
            "title": "T",
            "subtitle": "S",
            "backlogLink": "https://link",
            "currentMonth": "April",
            "months": ["April"],
            "timeline": {"startDate": "", "endDate": "2026-07-01", "tracks": [{"name":"M1","label":"L","color":"#000","milestones":[]}]},
            "heatmap": {"shipped":{}, "inProgress":{}, "carryover":{}, "blockers":{}}
        }
        """);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("startDate", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingTimelineEndDate_ValidationError()
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, """
        {
            "title": "T",
            "subtitle": "S",
            "backlogLink": "https://link",
            "currentMonth": "April",
            "months": ["April"],
            "timeline": {"startDate": "2026-01-01", "endDate": "", "tracks": [{"name":"M1","label":"L","color":"#000","milestones":[]}]},
            "heatmap": {"shipped":{}, "inProgress":{}, "carryover":{}, "blockers":{}}
        }
        """);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("endDate", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MultipleValidationErrors_AllReported()
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, """
        {
            "title": "",
            "subtitle": "",
            "backlogLink": "",
            "currentMonth": "",
            "months": [],
            "timeline": null,
            "heatmap": {"shipped":{}, "inProgress":{}, "carryover":{}, "blockers":{}}
        }
        """);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("subtitle", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("backlogLink", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("currentMonth", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("months", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("timeline", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }
}