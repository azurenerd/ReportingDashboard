using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests focused on DashboardDataService's ability to load and
/// produce correct HeatmapData for consumption by heatmap components.
/// Complements existing DashboardDataServiceIntegrationTests with heatmap-specific scenarios.
/// </summary>
[Trait("Category", "Integration")]
public class DashboardDataServiceHeatmapIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    public DashboardDataServiceHeatmapIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashHmInteg_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteJsonFile(object data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
        return path;
    }

    private object CreateDataWithHeatmap(
        Dictionary<string, string[]>? shipped = null,
        Dictionary<string, string[]>? inProgress = null,
        Dictionary<string, string[]>? carryover = null,
        Dictionary<string, string[]>? blockers = null)
    {
        return new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "https://example.com",
            currentMonth = "April",
            months = new[] { "January", "February", "March", "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[]
                {
                    new { name = "M1", label = "Track", color = "#000",
                        milestones = new[] { new { date = "2026-03-01", type = "poc", label = "Test" } } }
                }
            },
            heatmap = new
            {
                shipped = shipped ?? new Dictionary<string, string[]>(),
                inProgress = inProgress ?? new Dictionary<string, string[]>(),
                carryover = carryover ?? new Dictionary<string, string[]>(),
                blockers = blockers ?? new Dictionary<string, string[]>()
            }
        };
    }

    [Fact]
    public async Task LoadAsync_HeatmapWithMultipleMonths_AllDataAccessible()
    {
        var path = WriteJsonFile(CreateDataWithHeatmap(
            shipped: new Dictionary<string, string[]>
            {
                ["jan"] = new[] { "Feature A", "Feature B" },
                ["feb"] = new[] { "Feature C" },
                ["mar"] = new[] { "Feature D", "Feature E", "Feature F" },
                ["apr"] = new[] { "Feature G" }
            }));

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        var shipped = svc.Data!.Heatmap.Shipped;
        Assert.Equal(4, shipped.Count);
        Assert.Equal(2, shipped["jan"].Count);
        Assert.Single(shipped["feb"]);
        Assert.Equal(3, shipped["mar"].Count);
        Assert.Single(shipped["apr"]);
    }

    [Fact]
    public async Task LoadAsync_HeatmapAllCategoriesPopulated_DeserializesCorrectly()
    {
        var path = WriteJsonFile(CreateDataWithHeatmap(
            shipped: new Dictionary<string, string[]> { ["jan"] = new[] { "S1" } },
            inProgress: new Dictionary<string, string[]> { ["feb"] = new[] { "IP1", "IP2" } },
            carryover: new Dictionary<string, string[]> { ["mar"] = new[] { "CO1" } },
            blockers: new Dictionary<string, string[]> { ["apr"] = new[] { "BL1", "BL2", "BL3" } }
        ));

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Single(svc.Data!.Heatmap.Shipped["jan"]);
        Assert.Equal(2, svc.Data.Heatmap.InProgress["feb"].Count);
        Assert.Single(svc.Data.Heatmap.Carryover["mar"]);
        Assert.Equal(3, svc.Data.Heatmap.Blockers["apr"].Count);
    }

    [Fact]
    public async Task LoadAsync_HeatmapEmptyCategories_DeserializesAsEmptyDictionaries()
    {
        var path = WriteJsonFile(CreateDataWithHeatmap());

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Empty(svc.Data!.Heatmap.Shipped);
        Assert.Empty(svc.Data.Heatmap.InProgress);
        Assert.Empty(svc.Data.Heatmap.Carryover);
        Assert.Empty(svc.Data.Heatmap.Blockers);
    }

    [Fact]
    public async Task LoadAsync_HeatmapWithEmptyArrayValues_DeserializesCorrectly()
    {
        var path = WriteJsonFile(CreateDataWithHeatmap(
            shipped: new Dictionary<string, string[]>
            {
                ["jan"] = Array.Empty<string>(),
                ["feb"] = new[] { "Item" }
            }));

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Empty(svc.Data!.Heatmap.Shipped["jan"]);
        Assert.Single(svc.Data.Heatmap.Shipped["feb"]);
    }

    [Fact]
    public async Task LoadAsync_MonthsList_MatchesJsonOrder()
    {
        var path = WriteJsonFile(CreateDataWithHeatmap());

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        var months = svc.Data!.Months;
        Assert.Equal(4, months.Count);
        Assert.Equal("January", months[0]);
        Assert.Equal("February", months[1]);
        Assert.Equal("March", months[2]);
        Assert.Equal("April", months[3]);
    }

    [Fact]
    public async Task LoadAsync_CurrentMonth_PreservedFromJson()
    {
        var path = WriteJsonFile(CreateDataWithHeatmap());

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.Equal("April", svc.Data!.CurrentMonth);
    }

    [Fact]
    public async Task LoadAsync_HeatmapItemsWithSpecialCharacters_PreservedCorrectly()
    {
        var path = WriteJsonFile(CreateDataWithHeatmap(
            shipped: new Dictionary<string, string[]>
            {
                ["jan"] = new[] { "Feature with \"quotes\"", "Feature with <angle> brackets", "Feature & ampersand" }
            }));

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        var items = svc.Data!.Heatmap.Shipped["jan"];
        Assert.Equal(3, items.Count);
        Assert.Contains("Feature with \"quotes\"", items);
        Assert.Contains("Feature with <angle> brackets", items);
        Assert.Contains("Feature & ampersand", items);
    }
}