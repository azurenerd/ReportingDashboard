using System.Text.Json;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

/// <summary>
/// Focused tests for HeatmapData model: defaults, JSON serialization,
/// dictionary behavior, and edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class HeatmapDataModelTests
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    #region Defaults

    [Fact]
    public void HeatmapData_AllDictionaries_NotNull()
    {
        var hm = new HeatmapData();
        Assert.NotNull(hm.Shipped);
        Assert.NotNull(hm.InProgress);
        Assert.NotNull(hm.Carryover);
        Assert.NotNull(hm.Blockers);
    }

    [Fact]
    public void HeatmapData_AllDictionaries_Empty()
    {
        var hm = new HeatmapData();
        Assert.Empty(hm.Shipped);
        Assert.Empty(hm.InProgress);
        Assert.Empty(hm.Carryover);
        Assert.Empty(hm.Blockers);
    }

    [Fact]
    public void HeatmapData_CanAddToShipped_WithoutNullReference()
    {
        var hm = new HeatmapData();
        hm.Shipped["jan"] = new List<string> { "Item A" };
        Assert.Single(hm.Shipped);
        Assert.Equal("Item A", hm.Shipped["jan"][0]);
    }

    #endregion

    #region JSON Round-Trip

    [Fact]
    public void HeatmapData_JsonRoundTrip_AllCategories()
    {
        var original = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["jan"] = new() { "A", "B" },
                ["feb"] = new() { "C" }
            },
            InProgress = new Dictionary<string, List<string>>
            {
                ["mar"] = new() { "D" }
            },
            Carryover = new Dictionary<string, List<string>>
            {
                ["apr"] = new() { "E", "F", "G" }
            },
            Blockers = new Dictionary<string, List<string>>
            {
                ["mar"] = new() { "H" }
            }
        };

        var json = JsonSerializer.Serialize(original, JsonOpts);
        var result = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Shipped.Count);
        Assert.Equal(2, result.Shipped["jan"].Count);
        Assert.Single(result.Shipped["feb"]);
        Assert.Single(result.InProgress);
        Assert.Equal(3, result.Carryover["apr"].Count);
        Assert.Single(result.Blockers["mar"]);
    }

    [Fact]
    public void HeatmapData_Serialization_UsesJsonPropertyNames()
    {
        var hm = new HeatmapData
        {
            InProgress = new Dictionary<string, List<string>>
            {
                ["apr"] = new() { "Test" }
            }
        };

        var json = JsonSerializer.Serialize(hm, JsonOpts);
        Assert.Contains("\"inProgress\"", json);
        Assert.Contains("\"shipped\"", json);
        Assert.Contains("\"carryover\"", json);
        Assert.Contains("\"blockers\"", json);
    }

    [Fact]
    public void HeatmapData_Deserialize_MissingCategories_DefaultToEmpty()
    {
        var json = """{"shipped":{}}""";
        var result = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.NotNull(result);
        Assert.Empty(result!.InProgress);
        Assert.Empty(result.Carryover);
        Assert.Empty(result.Blockers);
    }

    [Fact]
    public void HeatmapData_Deserialize_EmptyLists_Preserved()
    {
        var json = """{"shipped":{"jan":[]},"inProgress":{},"carryover":{},"blockers":{}}""";
        var result = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.NotNull(result);
        Assert.True(result!.Shipped.ContainsKey("jan"));
        Assert.Empty(result.Shipped["jan"]);
    }

    [Fact]
    public void HeatmapData_Deserialize_SpecialCharactersInItems()
    {
        var json = """{"shipped":{"jan":["Item <A> & \"B\"","Item\nC"]},"inProgress":{},"carryover":{},"blockers":{}}""";
        var result = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.NotNull(result);
        Assert.Equal("Item <A> & \"B\"", result!.Shipped["jan"][0]);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void HeatmapData_EmptyObject_DeserializesWithDefaults()
    {
        var json = "{}";
        var result = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.NotNull(result);
        Assert.Empty(result!.Shipped);
        Assert.Empty(result.InProgress);
        Assert.Empty(result.Carryover);
        Assert.Empty(result.Blockers);
    }

    [Fact]
    public void HeatmapData_ManyMonthKeys_AllPreserved()
    {
        var shipped = new Dictionary<string, List<string>>();
        for (int i = 1; i <= 12; i++)
        {
            shipped[$"month{i}"] = new List<string> { $"Item for month {i}" };
        }

        var hm = new HeatmapData { Shipped = shipped };
        var json = JsonSerializer.Serialize(hm, JsonOpts);
        var result = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.Equal(12, result!.Shipped.Count);
    }

    [Fact]
    public void HeatmapData_LargeItemList_Preserved()
    {
        var items = Enumerable.Range(1, 50).Select(i => $"Item {i}").ToList();
        var hm = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>> { ["jan"] = items }
        };

        var json = JsonSerializer.Serialize(hm, JsonOpts);
        var result = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.Equal(50, result!.Shipped["jan"].Count);
    }

    #endregion
}