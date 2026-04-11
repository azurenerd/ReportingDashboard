using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for DashboardDataService state transitions specifically
/// related to header data fields. Tests the service lifecycle: valid → error → valid,
/// concurrent loads, and file mutation between loads.
/// </summary>
[Trait("Category", "Integration")]
public class HeaderServiceStateTransitionTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public HeaderServiceStateTransitionTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HdrState_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteValidJson(string title = "Test", string currentMonth = "April")
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        var json = JsonSerializer.Serialize(new
        {
            title,
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth,
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
        }, JsonOpts);
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public async Task LoadAsync_ValidThenModifyTitle_ReflectsNewTitle()
    {
        var path = WriteValidJson(title: "Version 1");
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);
        Assert.Equal("Version 1", svc.Data!.Title);

        // Overwrite with new title
        var newJson = JsonSerializer.Serialize(new
        {
            title = "Version 2",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        }, JsonOpts);
        File.WriteAllText(path, newJson);

        await svc.LoadAsync(path);
        Assert.Equal("Version 2", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_ValidThenRemoveTitle_TransitionsToError()
    {
        var path = WriteValidJson(title: "Good Title");
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);
        Assert.False(svc.IsError);
        Assert.Equal("Good Title", svc.Data!.Title);

        // Overwrite with missing title
        var badJson = JsonSerializer.Serialize(new
        {
            title = "",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        }, JsonOpts);
        File.WriteAllText(path, badJson);

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_ErrorThenFixTitle_RecoversProperly()
    {
        var svc = new DashboardDataService(_logger);

        // Load with empty title → error
        var badPath = Path.Combine(_tempDir, $"bad_{Guid.NewGuid():N}.json");
        File.WriteAllText(badPath, JsonSerializer.Serialize(new
        {
            title = "",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        }, JsonOpts));

        await svc.LoadAsync(badPath);
        Assert.True(svc.IsError);

        // Now load valid
        var goodPath = WriteValidJson(title: "Fixed Title");
        await svc.LoadAsync(goodPath);

        Assert.False(svc.IsError);
        Assert.Equal("Fixed Title", svc.Data!.Title);
        Assert.Null(svc.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_ChangeCurrentMonth_LegendDataUpdated()
    {
        var path1 = WriteValidJson(currentMonth: "January");
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path1);
        Assert.Equal("January", svc.Data!.CurrentMonth);

        var path2 = WriteValidJson(currentMonth: "December");
        await svc.LoadAsync(path2);
        Assert.Equal("December", svc.Data!.CurrentMonth);
    }

    [Fact]
    public async Task LoadAsync_FileDeletedBetweenLoads_ErrorState()
    {
        var path = WriteValidJson();
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);
        Assert.False(svc.IsError);

        File.Delete(path);

        await svc.LoadAsync(path);
        Assert.True(svc.IsError);
        Assert.Contains("not found", svc.ErrorMessage!);
    }

    [Fact]
    public async Task LoadAsync_MultipleSequentialLoads_LastOneWins()
    {
        var svc = new DashboardDataService(_logger);

        for (int i = 1; i <= 5; i++)
        {
            var path = WriteValidJson(title: $"Load {i}");
            await svc.LoadAsync(path);

            Assert.False(svc.IsError);
            Assert.Equal($"Load {i}", svc.Data!.Title);
        }
    }

    [Fact]
    public async Task LoadAsync_CorruptThenValid_HeaderFieldsIntact()
    {
        var svc = new DashboardDataService(_logger);

        var corruptPath = Path.Combine(_tempDir, $"corrupt_{Guid.NewGuid():N}.json");
        File.WriteAllText(corruptPath, "{{{{not json}}}}");
        await svc.LoadAsync(corruptPath);
        Assert.True(svc.IsError);

        var validPath = WriteValidJson(title: "After Corrupt");
        await svc.LoadAsync(validPath);

        Assert.False(svc.IsError);
        Assert.Equal("After Corrupt", svc.Data!.Title);
        Assert.Equal("Sub", svc.Data.Subtitle);
        Assert.Equal("https://link", svc.Data.BacklogLink);
        Assert.Equal("April", svc.Data.CurrentMonth);
    }
}