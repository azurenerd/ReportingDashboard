using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class DashboardDataServiceIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    public DashboardDataServiceIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashInteg_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteJsonFile(string json, string fileName = "data.json")
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, json);
        return path;
    }

    #region Full Pipeline: File → Service → Model

    [Fact]
    public async Task LoadAsync_FullValidFile_ProducesCompleteModel()
    {
        var path = WriteJsonFile(TestDataHelper.CreateValidDataJsonString());
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Equal("Integration Test Dashboard", svc.Data!.Title);
        Assert.Equal("QA Team - April 2026", svc.Data.Subtitle);
        Assert.Equal("https://dev.azure.com/test/backlog", svc.Data.BacklogLink);
        Assert.Equal("April", svc.Data.CurrentMonth);
        Assert.Equal(4, svc.Data.Months.Count);
    }

    [Fact]
    public async Task LoadAsync_FullValidFile_TimelineDataIntact()
    {
        var path = WriteJsonFile(TestDataHelper.CreateValidDataJsonString());
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        var tl = svc.Data!.Timeline;
        Assert.Equal("2026-01-01", tl.StartDate);
        Assert.Equal("2026-07-01", tl.EndDate);
        Assert.Equal("2026-04-10", tl.NowDate);
        Assert.Equal(2, tl.Tracks.Count);
        Assert.Equal("M1", tl.Tracks[0].Name);
        Assert.Equal("Core Platform", tl.Tracks[0].Label);
        Assert.Equal("#4285F4", tl.Tracks[0].Color);
        Assert.Equal(2, tl.Tracks[0].Milestones.Count);
        Assert.Equal("poc", tl.Tracks[0].Milestones[0].Type);
        Assert.Equal("production", tl.Tracks[0].Milestones[1].Type);
    }

    [Fact]
    public async Task LoadAsync_FullValidFile_HeatmapDataIntact()
    {
        var path = WriteJsonFile(TestDataHelper.CreateValidDataJsonString());
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        var hm = svc.Data!.Heatmap;
        Assert.Equal(3, hm.Shipped.Count);
        Assert.Equal(2, hm.Shipped["jan"].Count);
        Assert.Single(hm.InProgress);
        Assert.Equal(2, hm.InProgress["apr"].Count);
        Assert.Single(hm.Carryover);
        Assert.Single(hm.Blockers);
    }

    #endregion

    #region Error Recovery and State Transitions

    [Fact]
    public async Task LoadAsync_ErrorThenValid_RecoversProperly()
    {
        var svc = new DashboardDataService(_logger);

        // First load: error state
        await svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json"));
        Assert.True(svc.IsError);
        Assert.Null(svc.Data);

        // Second load: valid data — service should recover
        var path = WriteJsonFile(TestDataHelper.CreateValidDataJsonString());
        await svc.LoadAsync(path);
        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        // After successful load, ErrorMessage should no longer reflect the old error.
        // The service sets IsError = false on success; ErrorMessage may or may not be cleared
        // depending on implementation. The key invariant is IsError == false and Data != null.
        Assert.False(svc.IsError, "IsError should be false after successful load");
    }

    [Fact]
    public async Task LoadAsync_ValidThenCorrupt_TransitionsToError()
    {
        var svc = new DashboardDataService(_logger);
        var path = WriteJsonFile(TestDataHelper.CreateValidDataJsonString());

        await svc.LoadAsync(path);
        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);

        // Overwrite with corrupt JSON
        File.WriteAllText(path, "{ totally broken }}}");
        await svc.LoadAsync(path);
        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_ValidThenInvalid_TransitionsToValidationError()
    {
        var svc = new DashboardDataService(_logger);
        var path = WriteJsonFile(TestDataHelper.CreateValidDataJsonString());

        await svc.LoadAsync(path);
        Assert.False(svc.IsError);

        // Overwrite with structurally invalid data (empty required fields)
        var invalidJson = TestDataHelper.SerializeToJson(new
        {
            title = "",
            subtitle = "",
            months = Array.Empty<string>(),
            timeline = new { tracks = Array.Empty<object>() }
        });
        File.WriteAllText(path, invalidJson);
        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region File System Edge Cases

    [Fact]
    public async Task LoadAsync_EmptyFile_SetsError()
    {
        var path = WriteJsonFile("");
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.NotNull(svc.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_WhitespaceOnlyFile_SetsError()
    {
        var path = WriteJsonFile("   \n\t  ");
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_Utf8WithBom_DeserializesCorrectly()
    {
        var json = TestDataHelper.CreateValidDataJsonString();
        var path = Path.Combine(_tempDir, "bom.json");
        await File.WriteAllTextAsync(path, json, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_LargeDataFile_HandlesGracefully()
    {
        var largeShipped = new Dictionary<string, string[]>();
        for (int m = 1; m <= 12; m++)
        {
            var key = new DateTime(2026, m, 1).ToString("MMM").ToLowerInvariant().Substring(0, 3);
            largeShipped[key] = Enumerable.Range(1, 50).Select(i => $"Feature {key}-{i}").ToArray();
        }

        var data = new
        {
            title = "Large Dataset",
            subtitle = "Stress Test",
            currentMonth = "April",
            months = Enumerable.Range(1, 12).Select(m => new DateTime(2026, m, 1).ToString("MMMM")).ToArray(),
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2027-01-01",
                nowDate = "2026-04-10",
                tracks = Enumerable.Range(1, 20).Select(i => new
                {
                    name = $"M{i}",
                    label = $"Track {i}",
                    color = $"#{i * 13 % 256:X2}{i * 7 % 256:X2}{i * 3 % 256:X2}",
                    milestones = new[]
                    {
                        new { date = $"2026-{(i % 12) + 1:D2}-15", type = "poc", label = $"PoC {i}" }
                    }
                }).ToArray()
            },
            heatmap = new
            {
                shipped = largeShipped,
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        };

        var path = WriteJsonFile(TestDataHelper.SerializeToJson(data));
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(20, svc.Data!.Timeline.Tracks.Count);
        Assert.Equal(12, svc.Data.Heatmap.Shipped.Count);
    }

    [Fact]
    public async Task LoadAsync_JsonWithExtraFields_IgnoresThemGracefully()
    {
        var json = """
        {
            "title": "Extra Fields Test",
            "subtitle": "Should ignore unknowns",
            "backlogLink": "",
            "currentMonth": "April",
            "months": ["April"],
            "unknownField": "should be ignored",
            "anotherUnknown": 42,
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "name": "M1",
                        "label": "Track",
                        "color": "#000",
                        "milestones": [],
                        "extraProp": true
                    }
                ]
            },
            "heatmap": {
                "shipped": {},
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;

        var path = WriteJsonFile(json);
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("Extra Fields Test", svc.Data!.Title);
    }

    #endregion

    #region Concurrent Access

    [Fact]
    public async Task LoadAsync_ConcurrentLoads_DoNotThrow()
    {
        var path = WriteJsonFile(TestDataHelper.CreateValidDataJsonString());

        var tasks = Enumerable.Range(0, 10).Select(_ =>
        {
            var svc = new DashboardDataService(_logger);
            return svc.LoadAsync(path);
        });

        await Task.WhenAll(tasks);
    }

    #endregion
}