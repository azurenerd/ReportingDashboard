using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;

namespace ReportingDashboard.Tests.Unit.Services;

public class DashboardDataServiceTests : IDisposable
{
    private readonly DashboardDataService _service;
    private readonly string _tempDir;

    public DashboardDataServiceTests()
    {
        _service = new DashboardDataService(NullLogger<DashboardDataService>.Instance);
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_SetsError()
    {
        var nonExistentPath = Path.Combine(_tempDir, "nonexistent.json");

        await _service.LoadAsync(nonExistentPath);

        Assert.True(_service.IsError);
        Assert.Null(_service.Data);
        Assert.Contains("not found", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_InvalidJson_SetsError()
    {
        var path = Path.Combine(_tempDir, "bad.json");
        await File.WriteAllTextAsync(path, "{ invalid json !!! }");

        await _service.LoadAsync(path);

        Assert.True(_service.IsError);
        Assert.Null(_service.Data);
        Assert.Contains("Failed to parse", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_EmptyTitle_SetsValidationError()
    {
        var json = """
        {
            "title": "",
            "subtitle": "Sub",
            "backlogLink": "https://example.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-01",
                "tracks": [{ "name": "M1", "label": "Track", "color": "#000", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = Path.Combine(_tempDir, "empty-title.json");
        await File.WriteAllTextAsync(path, json);

        await _service.LoadAsync(path);

        Assert.True(_service.IsError);
        Assert.Contains("'title' is required", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_EmptyMonths_SetsValidationError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://example.com",
            "currentMonth": "Jan",
            "months": [],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-01",
                "tracks": [{ "name": "M1", "label": "Track", "color": "#000", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = Path.Combine(_tempDir, "empty-months.json");
        await File.WriteAllTextAsync(path, json);

        await _service.LoadAsync(path);

        Assert.True(_service.IsError);
        Assert.Contains("'months' must not be empty", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_InvalidMilestoneType_SetsValidationError()
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
                "nowDate": "2026-04-01",
                "tracks": [{
                    "name": "M1",
                    "label": "Track",
                    "color": "#000",
                    "milestones": [
                        { "date": "2026-02-01", "type": "invalid_type", "label": "Bad" }
                    ]
                }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = Path.Combine(_tempDir, "bad-milestone.json");
        await File.WriteAllTextAsync(path, json);

        await _service.LoadAsync(path);

        Assert.True(_service.IsError);
        Assert.Contains("invalid_type", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_ValidJson_LoadsData()
    {
        var json = """
        {
            "title": "Test Dashboard",
            "subtitle": "Team · Stream · Month",
            "backlogLink": "https://dev.azure.com/test",
            "currentMonth": "Mar",
            "months": ["Jan", "Feb", "Mar"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-03-15",
                "tracks": [{
                    "name": "M1",
                    "label": "Feature One",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-02-01", "type": "checkpoint", "label": "Feb 1" }
                    ]
                }]
            },
            "heatmap": {
                "shipped": { "jan": ["Item A"] },
                "inProgress": { "jan": [] },
                "carryover": { "jan": [] },
                "blockers": { "jan": [] }
            }
        }
        """;
        var path = Path.Combine(_tempDir, "valid.json");
        await File.WriteAllTextAsync(path, json);

        await _service.LoadAsync(path);

        Assert.False(_service.IsError);
        Assert.Null(_service.ErrorMessage);
        Assert.NotNull(_service.Data);
        Assert.Equal("Test Dashboard", _service.Data!.Title);
        Assert.Equal(3, _service.Data.Months.Count);
        Assert.Single(_service.Data.Timeline.Tracks);
        Assert.Single(_service.Data.Heatmap.Shipped["jan"]);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Best effort cleanup
        }
        GC.SuppressFinalize(this);
    }
}