using System.Diagnostics;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

/// <summary>
/// Comprehensive unit tests for DashboardDataService covering:
/// happy path, file errors, validation errors, edge cases, and performance.
/// </summary>
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashSvcTests_{Guid.NewGuid():N}");
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

    private static string BuildValidJson() => """
        {
            "title": "Test Dashboard",
            "subtitle": "Team \u00b7 Workstream \u00b7 April",
            "backlogLink": "https://example.com",
            "currentMonth": "Apr",
            "months": ["Jan","Feb","Mar","Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "name": "M1",
                        "label": "Track 1",
                        "color": "#4285F4",
                        "milestones": [
                            { "date": "2026-03-01", "type": "poc", "label": "PoC" }
                        ]
                    }
                ]
            },
            "heatmap": {
                "shipped": { "Jan": ["Item A"] },
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;

    // --- 1. Happy path ---

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_ValidJson_PopulatesData()
    {
        var path = WriteJson(BuildValidJson());
        var svc = CreateService();

        await svc.LoadAsync(path);

        svc.IsError.Should().BeFalse();
        svc.ErrorMessage.Should().BeNull();
        svc.Data.Should().NotBeNull();
        svc.Data!.Title.Should().Be("Test Dashboard");
        svc.Data.Months.Should().HaveCount(4);
        svc.Data.Timeline.Tracks.Should().HaveCount(1);
        svc.Data.Heatmap.Shipped.Should().ContainKey("Jan");
    }

    // --- 2. File not found ---

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_FileNotFound_SetsIsError()
    {
        var svc = CreateService();

        await svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json"));

        svc.IsError.Should().BeTrue();
        svc.ErrorMessage.Should().Contain("not found");
        svc.Data.Should().BeNull();
    }

    // --- 3. Malformed JSON ---

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_MalformedJson_SetsErrorMessage()
    {
        var path = WriteJson("{ this is not valid json !!! }");
        var svc = CreateService();

        await svc.LoadAsync(path);

        svc.IsError.Should().BeTrue();
        svc.ErrorMessage.Should().Contain("Failed to parse data.json");
        svc.Data.Should().BeNull();
    }

    // --- 4. Empty months array triggers validation ---

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

    // --- 5. Null optional heatmap fields default gracefully ---

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
        svc.Data!.Heatmap.Shipped.Should().ContainKey("Jan");
        svc.Data.Heatmap.InProgress.Should().NotBeNull();
        svc.Data.Heatmap.Carryover.Should().NotBeNull();
        svc.Data.Heatmap.Blockers.Should().NotBeNull();
    }

    // --- 6. Missing title validation ---

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_MissingTitle_ValidationError()
    {
        var json = """
        {
            "title": "",
            "subtitle": "Has subtitle",
            "backlogLink": "https://example.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
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
        svc.ErrorMessage.Should().Contain("title");
    }

    // --- 7. Invalid milestone type ---

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_InvalidMilestoneType_ValidationError()
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
                "tracks": [{
                    "name": "M1",
                    "milestones": [{ "date": "2026-01-01", "type": "invalid_type", "label": "Bad" }]
                }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        svc.IsError.Should().BeTrue();
        svc.ErrorMessage.Should().Contain("invalid_type");
    }

    // --- 8. currentMonth not in months array ---
    // Documents actual service behavior: service does not currently validate this.

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_CurrentMonthNotInMonths_LoadsWithoutError()
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

        // Service does not currently enforce currentMonth membership in months array
        svc.IsError.Should().BeFalse();
        svc.Data.Should().NotBeNull();
        svc.Data!.CurrentMonth.Should().Be("Dec");
    }

    // --- 9. endDate before startDate ---
    // Documents actual service behavior: service does not currently validate date ordering.

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_EndDateBeforeStartDate_LoadsWithoutError()
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

        // Service does not currently enforce endDate > startDate ordering
        svc.IsError.Should().BeFalse();
        svc.Data.Should().NotBeNull();
    }

    // --- 10. Performance under 100ms ---

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
        sw.ElapsedMilliseconds.Should().BeLessThan(100,
            "JSON parsing should complete under 100ms for files up to 50KB");
    }
}