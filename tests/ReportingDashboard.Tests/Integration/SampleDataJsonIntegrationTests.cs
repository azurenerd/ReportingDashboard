using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests that load the ACTUAL wwwroot/data.json sample file
/// from the project to verify it is valid, well-formed, and passes all
/// service validation. This catches regressions where someone edits the
/// sample data and accidentally breaks the contract.
/// </summary>
[Trait("Category", "Integration")]
public class SampleDataJsonIntegrationTests
{
    private readonly ILogger<DashboardDataService> _logger;

    public SampleDataJsonIntegrationTests()
    {
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    private string? FindSampleDataJson()
    {
        // Walk up from test assembly location to find the src project
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            var candidate = Path.Combine(dir, "src", "ReportingDashboard", "wwwroot", "data.json");
            if (File.Exists(candidate))
                return candidate;

            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }

        // Also try relative path from common test output dirs
        var altPaths = new[]
        {
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "ReportingDashboard", "wwwroot", "data.json")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "ReportingDashboard", "wwwroot", "data.json")),
        };
        return altPaths.FirstOrDefault(File.Exists);
    }

    [Fact]
    public async Task SampleDataJson_LoadsWithoutError()
    {
        var path = FindSampleDataJson();
        if (path == null)
        {
            // Skip gracefully if file not found (CI without full repo)
            return;
        }

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError, $"Sample data.json failed: {svc.ErrorMessage}");
        Assert.NotNull(svc.Data);
    }

    [Fact]
    public async Task SampleDataJson_HasNonEmptyTitle()
    {
        var path = FindSampleDataJson();
        if (path == null) return;

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(string.IsNullOrWhiteSpace(svc.Data?.Title));
    }

    [Fact]
    public async Task SampleDataJson_HasAtLeastOneTrack()
    {
        var path = FindSampleDataJson();
        if (path == null) return;

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.NotNull(svc.Data?.Timeline);
        Assert.True(svc.Data!.Timeline.Tracks.Count >= 1,
            "Sample data should have at least 1 timeline track");
    }

    [Fact]
    public async Task SampleDataJson_HasAtLeastOneMonth()
    {
        var path = FindSampleDataJson();
        if (path == null) return;

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.True(svc.Data!.Months.Count >= 1,
            "Sample data should have at least 1 month");
    }

    [Fact]
    public async Task SampleDataJson_TimelineDatesAreValid()
    {
        var path = FindSampleDataJson();
        if (path == null) return;

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.True(DateTime.TryParse(svc.Data!.Timeline.StartDate, out _),
            "startDate should be a valid date");
        Assert.True(DateTime.TryParse(svc.Data.Timeline.EndDate, out _),
            "endDate should be a valid date");
    }

    [Fact]
    public async Task SampleDataJson_AllTrackMilestonesHaveValidDates()
    {
        var path = FindSampleDataJson();
        if (path == null) return;

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        foreach (var track in svc.Data!.Timeline.Tracks)
        {
            foreach (var ms in track.Milestones)
            {
                Assert.True(DateTime.TryParse(ms.Date, out _),
                    $"Milestone '{ms.Label}' in track '{track.Name}' has invalid date: {ms.Date}");
            }
        }
    }

    [Fact]
    public async Task SampleDataJson_AllTrackMilestonesHaveValidTypes()
    {
        var path = FindSampleDataJson();
        if (path == null) return;

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var validTypes = new[] { "checkpoint", "poc", "production" };
        foreach (var track in svc.Data!.Timeline.Tracks)
        {
            foreach (var ms in track.Milestones)
            {
                Assert.Contains(ms.Type, validTypes);
            }
        }
    }

    [Fact]
    public async Task SampleDataJson_AllTracksHaveDistinctColors()
    {
        var path = FindSampleDataJson();
        if (path == null) return;

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var colors = svc.Data!.Timeline.Tracks.Select(t => t.Color).ToList();
        Assert.Equal(colors.Count, colors.Distinct().Count());
    }

    [Fact]
    public async Task SampleDataJson_CurrentMonthIsInMonthsList()
    {
        var path = FindSampleDataJson();
        if (path == null) return;

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.Contains(svc.Data!.CurrentMonth,
            svc.Data.Months.Select(m => m),
            StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SampleDataJson_IsValidJson_DirectDeserialization()
    {
        var path = FindSampleDataJson();
        if (path == null) return;

        var json = await File.ReadAllTextAsync(path);
        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var data = JsonSerializer.Deserialize<DashboardData>(json, opts);

        Assert.NotNull(data);
        Assert.False(string.IsNullOrEmpty(data!.Title));
    }
}