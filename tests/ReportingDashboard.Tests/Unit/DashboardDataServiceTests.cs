using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Data;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteTempJson(string json)
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
        return path;
    }

    private static string BuildValidJson() => JsonSerializer.Serialize(new
    {
        project = new { title = "Test", subtitle = "Sub", backlogUrl = "http://example.com" },
        timeline = new
        {
            startDate = "2026-01-01",
            endDate = "2026-06-30",
            tracks = new[]
            {
                new
                {
                    id = "M1", name = "Track 1", color = "#0078D4",
                    milestones = new[] { new { date = "2026-03-01", type = "start", label = "Start" } }
                }
            }
        },
        heatmap = new
        {
            columns = new[] { "Jan", "Feb" },
            currentColumn = "Feb",
            rows = new[]
            {
                new { category = "Shipped", items = new Dictionary<string, List<string>> { ["Jan"] = new() { "Item1" }, ["Feb"] = new() } }
            }
        }
    });

    [Fact]
    public void ValidJson_SetsHasDataTrue()
    {
        var path = WriteTempJson(BuildValidJson());
        var svc = new DashboardDataService(path);

        svc.HasData.Should().BeTrue();
        svc.Data.Should().NotBeNull();
        svc.ErrorMessage.Should().BeNull();
        svc.Data!.Project.Title.Should().Be("Test");
    }

    [Fact]
    public void MissingFile_SetsErrorMessage()
    {
        var path = Path.Combine(_tempDir, "nonexistent.json");
        var svc = new DashboardDataService(path);

        svc.HasData.Should().BeFalse();
        svc.ErrorMessage.Should().Contain("Dashboard data not found");
        svc.ErrorMessage.Should().Contain("data.json exists in the Data/ directory");
    }

    [Fact]
    public void MalformedJson_SetsErrorWithDetails()
    {
        var path = WriteTempJson("{ broken json");
        var svc = new DashboardDataService(path);

        svc.HasData.Should().BeFalse();
        svc.ErrorMessage.Should().Contain("Error reading data.json:");
        svc.ErrorMessage.Should().Contain("Please fix the JSON syntax");
    }

    [Fact]
    public void EmptyTracks_FailsValidation()
    {
        var json = JsonSerializer.Serialize(new
        {
            project = new { title = "T", subtitle = "S", backlogUrl = "http://x.com" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-06-30", tracks = Array.Empty<object>() },
            heatmap = new
            {
                columns = new[] { "Jan" }, currentColumn = "Jan",
                rows = new[] { new { category = "Shipped", items = new Dictionary<string, List<string>> { ["Jan"] = new() } } }
            }
        });
        var path = WriteTempJson(json);
        var svc = new DashboardDataService(path);

        svc.HasData.Should().BeFalse();
        svc.ErrorMessage.Should().Contain("timeline.tracks must have at least 1 entry");
    }

    [Fact]
    public void InvalidCurrentColumn_FailsValidation()
    {
        var json = JsonSerializer.Serialize(new
        {
            project = new { title = "T", subtitle = "S", backlogUrl = "http://x.com" },
            timeline = new
            {
                startDate = "2026-01-01", endDate = "2026-06-30",
                tracks = new[] { new { id = "M1", name = "T1", color = "#000", milestones = new[] { new { date = "2026-02-01", type = "start" } } } }
            },
            heatmap = new
            {
                columns = new[] { "Jan", "Feb" }, currentColumn = "March",
                rows = new[] { new { category = "Shipped", items = new Dictionary<string, List<string>> { ["Jan"] = new() } } }
            }
        });
        var path = WriteTempJson(json);
        var svc = new DashboardDataService(path);

        svc.HasData.Should().BeFalse();
        svc.ErrorMessage.Should().Contain("heatmap.currentColumn");
        svc.ErrorMessage.Should().Contain("March");
    }
}