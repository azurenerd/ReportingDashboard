using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

/// <summary>
/// Tests the actual DashboardDataService from PR #1171 source code.
/// Constructor takes ILogger&lt;DashboardDataService&gt;; LoadAsync takes a file path string.
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
    {
        return new DashboardDataService(NullLogger<DashboardDataService>.Instance);
    }

    private string WriteJson(string json)
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
        return path;
    }

    private static string BuildValidJson() => """
        {
            "title": "Test Dashboard",
            "subtitle": "Team · Workstream · April",
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
        svc.Data.Timeline.Tracks.Should().HaveCount(1);
        svc.Data.Heatmap.Shipped.Should().ContainKey("Jan");
    }

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

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_MissingTitle_ValidationError()
    {
        var json = """
        {
            "title": "",
            "subtitle": "Has subtitle",
            "months": ["Jan"],
            "timeline": { "tracks": [{ "name": "M1", "milestones": [] }] }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        svc.IsError.Should().BeTrue();
        svc.ErrorMessage.Should().Contain("'title' is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoadAsync_InvalidMilestoneType_ValidationError()
    {
        var json = """
        {
            "title": "Dashboard",
            "subtitle": "Sub",
            "months": ["Jan"],
            "timeline": {
                "tracks": [{
                    "name": "M1",
                    "milestones": [{ "date": "2026-01-01", "type": "invalid_type", "label": "Bad" }]
                }]
            }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        svc.IsError.Should().BeTrue();
        svc.ErrorMessage.Should().Contain("milestone type 'invalid_type' is not valid");
        svc.ErrorMessage.Should().Contain("checkpoint");
        svc.ErrorMessage.Should().Contain("poc");
        svc.ErrorMessage.Should().Contain("production");
    }
}