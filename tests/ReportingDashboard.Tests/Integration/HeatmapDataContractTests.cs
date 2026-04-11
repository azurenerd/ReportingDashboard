using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying the data contract between data.json heatmap fields
/// and the DashboardDataService → HeatmapData model pipeline.
/// Tests all four category dictionaries (shipped, inProgress, carryover, blockers)
/// through file I/O and JSON deserialization.
/// </summary>
[Trait("Category", "Integration")]
public class HeatmapDataContractTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public HeatmapDataContractTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HmContract_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteJsonObj(object data)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOpts));
        return path;
    }

    private DashboardDataService CreateService() => new(_logger);

    private object CreateValidDataWith(object heatmap) => new
    {
        title = "T",
        subtitle = "S",
        backlogLink = "https://link",
        currentMonth = "April",
        months = new[] { "January", "February", "March", "April" },
        timeline = new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            nowDate = "2026-04-10",
            tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
        },
        heatmap
    };

    #region Full Heatmap Deserialization

    [Fact]
    public async Task LoadAsync_FullHeatmap_AllCategoriesDeserialized()
    {
        var data = CreateValidDataWith(new
        {
            shipped = new Dictionary<string, string[]>
            {
                ["jan"] = new[] { "Auth Module", "CI Pipeline" },
                ["feb"] = new[] { "Search Feature" },
                ["mar"] = new[] { "Dashboard v1" }
            },
            inProgress = new Dictionary<string, string[]>
            {
                ["apr"] = new[] { "Analytics Engine", "Export API" }
            },
            carryover = new Dictionary<string, string[]>
            {
                ["mar"] = new[] { "Legacy Migration" }
            },
            blockers = new Dictionary<string, string[]>
            {
                ["apr"] = new[] { "Vendor SDK Delay" }
            }
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        var hm = svc.Data!.Heatmap;

        Assert.Equal(3, hm.Shipped.Count);
        Assert.Equal(2, hm.Shipped["jan"].Count);
        Assert.Contains("Auth Module", hm.Shipped["jan"]);
        Assert.Contains("CI Pipeline", hm.Shipped["jan"]);
        Assert.Single(hm.Shipped["feb"]);
        Assert.Single(hm.Shipped["mar"]);

        Assert.Single(hm.InProgress);
        Assert.Equal(2, hm.InProgress["apr"].Count);

        Assert.Single(hm.Carryover);
        Assert.Contains("Legacy Migration", hm.Carryover["mar"]);

        Assert.Single(hm.Blockers);
        Assert.Contains("Vendor SDK Delay", hm.Blockers["apr"]);
    }

    [Fact]
    public async Task LoadAsync_EmptyHeatmapCategories_AllEmptyDictionaries()
    {
        var data = CreateValidDataWith(new
        {
            shipped = new Dictionary<string, string[]>(),
            inProgress = new Dictionary<string, string[]>(),
            carryover = new Dictionary<string, string[]>(),
            blockers = new Dictionary<string, string[]>()
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Empty(svc.Data!.Heatmap.Shipped);
        Assert.Empty(svc.Data.Heatmap.InProgress);
        Assert.Empty(svc.Data.Heatmap.Carryover);
        Assert.Empty(svc.Data.Heatmap.Blockers);
    }

    #endregion

    #region Month Key Variations

    [Theory]
    [InlineData("jan")]
    [InlineData("january")]
    [InlineData("Jan")]
    [InlineData("JANUARY")]
    public async Task LoadAsync_ShippedMonthKeys_PreservedExactly(string key)
    {
        var shipped = new Dictionary<string, string[]> { [key] = new[] { "Item" } };
        var data = CreateValidDataWith(new
        {
            shipped,
            inProgress = new Dictionary<string, string[]>(),
            carryover = new Dictionary<string, string[]>(),
            blockers = new Dictionary<string, string[]>()
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.True(svc.Data!.Heatmap.Shipped.ContainsKey(key));
    }

    [Fact]
    public async Task LoadAsync_ManyMonthKeys_AllPreserved()
    {
        var shipped = new Dictionary<string, string[]>
        {
            ["jan"] = new[] { "A" },
            ["feb"] = new[] { "B" },
            ["mar"] = new[] { "C" },
            ["apr"] = new[] { "D" },
            ["may"] = new[] { "E" },
            ["jun"] = new[] { "F" }
        };
        var data = CreateValidDataWith(new
        {
            shipped,
            inProgress = new Dictionary<string, string[]>(),
            carryover = new Dictionary<string, string[]>(),
            blockers = new Dictionary<string, string[]>()
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(6, svc.Data!.Heatmap.Shipped.Count);
    }

    #endregion

    #region Item Content Variations

    [Fact]
    public async Task LoadAsync_ManyItemsPerMonth_AllPreserved()
    {
        var items = Enumerable.Range(1, 20).Select(i => $"Item {i}").ToArray();
        var data = CreateValidDataWith(new
        {
            shipped = new Dictionary<string, string[]> { ["jan"] = items },
            inProgress = new Dictionary<string, string[]>(),
            carryover = new Dictionary<string, string[]>(),
            blockers = new Dictionary<string, string[]>()
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(20, svc.Data!.Heatmap.Shipped["jan"].Count);
        Assert.Contains("Item 1", svc.Data.Heatmap.Shipped["jan"]);
        Assert.Contains("Item 20", svc.Data.Heatmap.Shipped["jan"]);
    }

    [Fact]
    public async Task LoadAsync_ItemsWithSpecialCharacters_Preserved()
    {
        var data = CreateValidDataWith(new
        {
            shipped = new Dictionary<string, string[]>
            {
                ["jan"] = new[] { "Feature <Alpha> & \"Beta\"", "Design: C# → F#", "日本語テスト" }
            },
            inProgress = new Dictionary<string, string[]>(),
            carryover = new Dictionary<string, string[]>(),
            blockers = new Dictionary<string, string[]>()
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Contains("Feature <Alpha> & \"Beta\"", svc.Data!.Heatmap.Shipped["jan"]);
    }

    [Fact]
    public async Task LoadAsync_EmptyItemsList_PreservedAsEmptyList()
    {
        var data = CreateValidDataWith(new
        {
            shipped = new Dictionary<string, string[]> { ["jan"] = Array.Empty<string>() },
            inProgress = new Dictionary<string, string[]>(),
            carryover = new Dictionary<string, string[]>(),
            blockers = new Dictionary<string, string[]>()
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.True(svc.Data!.Heatmap.Shipped.ContainsKey("jan"));
        Assert.Empty(svc.Data.Heatmap.Shipped["jan"]);
    }

    #endregion

    #region Cross-Category Data Independence

    [Fact]
    public async Task LoadAsync_SameMonthKeyAcrossCategories_IndependentData()
    {
        var data = CreateValidDataWith(new
        {
            shipped = new Dictionary<string, string[]> { ["apr"] = new[] { "Shipped Item" } },
            inProgress = new Dictionary<string, string[]> { ["apr"] = new[] { "InProgress Item" } },
            carryover = new Dictionary<string, string[]> { ["apr"] = new[] { "Carryover Item" } },
            blockers = new Dictionary<string, string[]> { ["apr"] = new[] { "Blocker Item" } }
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Single(svc.Data!.Heatmap.Shipped["apr"]);
        Assert.Contains("Shipped Item", svc.Data.Heatmap.Shipped["apr"]);
        Assert.Single(svc.Data.Heatmap.InProgress["apr"]);
        Assert.Contains("InProgress Item", svc.Data.Heatmap.InProgress["apr"]);
        Assert.Single(svc.Data.Heatmap.Carryover["apr"]);
        Assert.Contains("Carryover Item", svc.Data.Heatmap.Carryover["apr"]);
        Assert.Single(svc.Data.Heatmap.Blockers["apr"]);
        Assert.Contains("Blocker Item", svc.Data.Heatmap.Blockers["apr"]);
    }

    #endregion
}