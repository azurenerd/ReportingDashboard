using System.Text.Json;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

/// <summary>
/// Unit tests for HeatmapData model focusing on dictionary structure,
/// JSON serialization/deserialization, and edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class HeatmapDataTests
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    #region Default Values

    [Fact]
    public void HeatmapData_Defaults_AllDictionariesEmpty()
    {
        var hm = new HeatmapData();

        Assert.NotNull(hm.Shipped);
        Assert.Empty(hm.Shipped);
        Assert.NotNull(hm.InProgress);
        Assert.Empty(hm.InProgress);
        Assert.NotNull(hm.Carryover);
        Assert.Empty(hm.Carryover);
        Assert.NotNull(hm.Blockers);
        Assert.Empty(hm.Blockers);
    }

    #endregion

    #region Property Assignment

    [Fact]
    public void Shipped_CanAddAndRetrieve()
    {
        var hm = new HeatmapData();
        hm.Shipped["jan"] = new List<string> { "Item A" };

        Assert.Single(hm.Shipped);
        Assert.Contains("Item A", hm.Shipped["jan"]);
    }

    [Fact]
    public void InProgress_CanAddAndRetrieve()
    {
        var hm = new HeatmapData();
        hm.InProgress["apr"] = new List<string> { "Item B", "Item C" };

        Assert.Equal(2, hm.InProgress["apr"].Count);
    }

    [Fact]
    public void Carryover_CanAddAndRetrieve()
    {
        var hm = new HeatmapData();
        hm.Carryover["mar"] = new List<string> { "Debt" };

        Assert.Single(hm.Carryover["mar"]);
    }

    [Fact]
    public void Blockers_CanAddAndRetrieve()
    {
        var hm = new HeatmapData();
        hm.Blockers["feb"] = new List<string> { "Bug X" };

        Assert.Single(hm.Blockers["feb"]);
    }

    #endregion

    #region JSON Deserialization

    [Fact]
    public void HeatmapData_Deserialize_JsonPropertyNames()
    {
        var json = """
        {
            "shipped": {"jan": ["A"]},
            "inProgress": {"feb": ["B"]},
            "carryover": {"mar": ["C"]},
            "blockers": {"apr": ["D"]}
        }
        """;

        var hm = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.NotNull(hm);
        Assert.Single(hm!.Shipped["jan"]);
        Assert.Single(hm.InProgress["feb"]);
        Assert.Single(hm.Carryover["mar"]);
        Assert.Single(hm.Blockers["apr"]);
    }

    [Fact]
    public void HeatmapData_Deserialize_EmptyDictionaries()
    {
        var json = """
        {
            "shipped": {},
            "inProgress": {},
            "carryover": {},
            "blockers": {}
        }
        """;

        var hm = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.NotNull(hm);
        Assert.Empty(hm!.Shipped);
        Assert.Empty(hm.InProgress);
    }

    [Fact]
    public void HeatmapData_Deserialize_MultipleItemsPerMonth()
    {
        var json = """
        {
            "shipped": {"jan": ["A", "B", "C", "D", "E"]},
            "inProgress": {},
            "carryover": {},
            "blockers": {}
        }
        """;

        var hm = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);
        Assert.Equal(5, hm!.Shipped["jan"].Count);
    }

    [Fact]
    public void HeatmapData_Deserialize_MultipleMonths()
    {
        var json = """
        {
            "shipped": {"jan": ["A"], "feb": ["B"], "mar": ["C"]},
            "inProgress": {},
            "carryover": {},
            "blockers": {}
        }
        """;

        var hm = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);
        Assert.Equal(3, hm!.Shipped.Count);
    }

    #endregion

    #region JSON Round-Trip

    [Fact]
    public void HeatmapData_RoundTrip_PreservesData()
    {
        var original = new HeatmapData
        {
            Shipped = new() { ["jan"] = new() { "X", "Y" } },
            InProgress = new() { ["apr"] = new() { "Z" } },
            Carryover = new(),
            Blockers = new() { ["feb"] = new() { "Bug" } }
        };

        var json = JsonSerializer.Serialize(original, JsonOpts);
        var restored = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.Equal(2, restored!.Shipped["jan"].Count);
        Assert.Single(restored.InProgress["apr"]);
        Assert.Empty(restored.Carryover);
        Assert.Single(restored.Blockers["feb"]);
    }

    [Fact]
    public void HeatmapData_Serialize_UsesCamelCasePropertyNames()
    {
        var hm = new HeatmapData();
        var json = JsonSerializer.Serialize(hm, JsonOpts);

        Assert.Contains("\"shipped\"", json);
        Assert.Contains("\"inProgress\"", json);
        Assert.Contains("\"carryover\"", json);
        Assert.Contains("\"blockers\"", json);
        Assert.DoesNotContain("\"Shipped\"", json);
        Assert.DoesNotContain("\"InProgress\"", json);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void HeatmapData_EmptyStringItems_Preserved()
    {
        var hm = new HeatmapData();
        hm.Shipped["jan"] = new List<string> { "", "Valid", "" };

        var json = JsonSerializer.Serialize(hm, JsonOpts);
        var restored = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.Equal(3, restored!.Shipped["jan"].Count);
        Assert.Equal("", restored.Shipped["jan"][0]);
    }

    [Fact]
    public void HeatmapData_SpecialCharacterItems_Preserved()
    {
        var hm = new HeatmapData();
        hm.Shipped["jan"] = new List<string> { "Feature <Alpha>", "Bug & \"Fix\"" };

        var json = JsonSerializer.Serialize(hm, JsonOpts);
        var restored = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.Contains("Feature <Alpha>", restored!.Shipped["jan"]);
        Assert.Contains("Bug & \"Fix\"", restored.Shipped["jan"]);
    }

    #endregion
}