using System.Diagnostics;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

/// <summary>
/// Covers validation edge cases and performance not tested by DashboardDataServiceTests.
/// </summary>
public class DashboardDataServiceValidationTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardDataServiceValidationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashValTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private DashboardDataService CreateService()
        => new(NullLogger<DashboardDataService>.Instance);

    private string WriteJson(string json, string fileName = "data.json")
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_EmptyMonths_ValidationError()
    {
        var json = """
        {
            "title": "Dashboard",
            "subtitle": "Sub",
            "backlogLink": "https://example.com",
            "currentMonth": "Apr",
            "months": [],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        svc.IsError.Should().BeTrue();
        svc.ErrorMessage.Should().NotBeNullOrEmpty();
        svc.ErrorMessage.Should().Contain("months");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_CurrentMonthNotInMonths_ValidationError()
    {
        var json = """
        {
            "title": "Dashboard",
            "subtitle": "Sub",
            "backlogLink": "https://example.com",
            "currentMonth": "Dec",
            "months": ["Jan", "Feb", "Mar"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "milestones": [{ "date": "2026-03-01", "type": "poc", "label": "P" }] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        svc.IsError.Should().BeTrue("currentMonth 'Dec' is not in months array ['Jan','Feb','Mar']");
        svc.ErrorMessage.Should().Contain("currentMonth");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_EndDateBeforeStartDate_ValidationError()
    {
        var json = """
        {
            "title": "Dashboard",
            "subtitle": "Sub",
            "backlogLink": "https://example.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-06-30",
                "endDate": "2026-01-01",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "milestones": [{ "date": "2026-03-01", "type": "poc", "label": "P" }] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        svc.IsError.Should().BeTrue("endDate 2026-01-01 is before startDate 2026-06-30");
        svc.ErrorMessage.Should().Contain("endDate");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_NullOptionalHeatmapFields_DefaultsGracefully()
    {
        var json = """
        {
            "title": "Dashboard",
            "subtitle": "Sub",
            "backlogLink": "https://example.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "milestones": [{ "date": "2026-03-01", "type": "poc", "label": "P" }] }]
            },
            "heatmap": {
                "shipped": { "Jan": ["Feature A"] }
            }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        svc.IsError.Should().BeFalse();
        svc.Data.Should().NotBeNull();
        svc.Data!.Heatmap.Should().NotBeNull();
        svc.Data.Heatmap.Shipped.Should().ContainKey("Jan");
        svc.Data.Heatmap.InProgress.Should().NotBeNull();
        svc.Data.Heatmap.Carryover.Should().NotBeNull();
        svc.Data.Heatmap.Blockers.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_LargeFile_CompletesUnder100ms()
    {
        var sb = new StringBuilder();
        sb.Append("""
        {
            "title": "Perf Test",
            "subtitle": "Sub",
            "backlogLink": "https://example.com",
            "currentMonth": "Jan",
            "months": ["Jan","Feb","Mar","Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "milestones": [{ "date": "2026-03-01", "type": "poc", "label": "PoC" }] }]
            },
            "heatmap": {
                "shipped": { "Jan": [
        """);
        for (int i = 0; i < 200; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append($"\"Shipped feature item number {i:D4} with padding text here\"");
        }
        sb.Append("""
                ]},
                "inProgress": { "Feb": [
        """);
        for (int i = 0; i < 200; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append($"\"In progress work item number {i:D4} with padding\"");
        }
        sb.Append("""
                ]},
                "carryover": {},
                "blockers": {}
            }
        }
        """);

        var json = sb.ToString();
        json.Length.Should().BeGreaterThan(20000, "test file should be large enough to be meaningful");

        var path = WriteJson(json, "large_data.json");
        var svc = CreateService();

        var sw = Stopwatch.StartNew();
        await svc.LoadAsync(path);
        sw.Stop();

        svc.IsError.Should().BeFalse();
        svc.Data.Should().NotBeNull();
        sw.ElapsedMilliseconds.Should().BeLessThan(100, "JSON parsing should complete under 100ms for files up to 50KB");
    }
}