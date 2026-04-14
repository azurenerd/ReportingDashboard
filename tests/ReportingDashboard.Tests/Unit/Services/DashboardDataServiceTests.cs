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
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_SetsIsError()
    {
        await _service.LoadAsync(Path.Combine(_tempDir, "missing.json"));

        Assert.True(_service.IsError);
        Assert.Null(_service.Data);
        Assert.Contains("not found", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_SetsIsError()
    {
        var path = Path.Combine(_tempDir, "bad.json");
        await File.WriteAllTextAsync(path, "{ not valid json }}}");

        await _service.LoadAsync(path);

        Assert.True(_service.IsError);
        Assert.Null(_service.Data);
        Assert.Contains("Failed to parse", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_MissingTitle_SetsIsError()
    {
        var json = """
        {
            "title": "",
            "subtitle": "Sub",
            "months": ["Jan"],
            "timeline": { "tracks": [{ "name": "M1", "label": "L", "milestones": [] }] }
        }
        """;
        var path = Path.Combine(_tempDir, "no-title.json");
        await File.WriteAllTextAsync(path, json);

        await _service.LoadAsync(path);

        Assert.True(_service.IsError);
        Assert.Contains("'title' is required", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_EmptyMonths_SetsIsError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "months": [],
            "timeline": { "tracks": [{ "name": "M1", "label": "L", "milestones": [] }] }
        }
        """;
        var path = Path.Combine(_tempDir, "no-months.json");
        await File.WriteAllTextAsync(path, json);

        await _service.LoadAsync(path);

        Assert.True(_service.IsError);
        Assert.Contains("'months' must not be empty", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_InvalidMilestoneType_SetsIsError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "months": ["Jan"],
            "timeline": {
                "tracks": [{
                    "name": "M1",
                    "label": "L",
                    "milestones": [{ "date": "2026-01-15", "type": "invalid_type", "label": "Bad" }]
                }]
            }
        }
        """;
        var path = Path.Combine(_tempDir, "bad-type.json");
        await File.WriteAllTextAsync(path, json);

        await _service.LoadAsync(path);

        Assert.True(_service.IsError);
        Assert.Contains("invalid_type", _service.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_ValidJson_PopulatesData()
    {
        var json = """
        {
            "title": "Project Atlas",
            "subtitle": "Cloud · Infra · April 2026",
            "backlogLink": "https://dev.azure.com/test",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{
                    "name": "M1",
                    "label": "Core Platform",
                    "color": "#4285F4",
                    "milestones": [
                        { "date": "2026-02-14", "type": "poc", "label": "PoC Complete" }
                    ]
                }]
            },
            "heatmap": {
                "shipped": { "Jan": ["Item A"] },
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        var path = Path.Combine(_tempDir, "valid.json");
        await File.WriteAllTextAsync(path, json);

        await _service.LoadAsync(path);

        Assert.False(_service.IsError);
        Assert.Null(_service.ErrorMessage);
        Assert.NotNull(_service.Data);
        Assert.Equal("Project Atlas", _service.Data!.Title);
        Assert.Equal(4, _service.Data.Months.Count);
        Assert.Single(_service.Data.Timeline.Tracks);
    }
}