using System.Text.Json;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class HeatmapDataTests
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    #region Defaults

    [Fact]
    public void HeatmapData_DefaultShipped_IsEmptyDictionary()
    {
        var hm = new HeatmapData();
        Assert.NotNull(hm.Shipped);
        Assert.Empty(hm.Shipped);
    }

    [Fact]
    public void HeatmapData_DefaultInProgress_IsEmptyDictionary()
    {
        var hm = new HeatmapData();
        Assert.NotNull(hm.InProgress);
        Assert.Empty(hm.InProgress);
    }

    [Fact]
    public void HeatmapData_DefaultCarryover_IsEmptyDictionary()
    {
        var hm = new HeatmapData();
        Assert.NotNull(hm.Carryover);
        Assert.Empty(hm.Carryover);
    }

    [Fact]
    public void HeatmapData_DefaultBlockers_IsEmptyDictionary()
    {
        var hm = new HeatmapData();
        Assert.NotNull(hm.Blockers);
        Assert.Empty(hm.Blockers);
    }

    #endregion

    #region Deserialization

    [Fact]
    public void HeatmapData_DeserializeFullData_AllCategoriesPopulated()
    {
        var json = """
        {
            "shipped": {
                "jan": ["Auth Module", "CI Pipeline"],
                "feb": ["Search Feature"],
                "mar": ["Dashboard v1"],
                "apr": []
            },
            "inProgress": {
                "apr": ["Analytics Engine", "Export API"]
            },
            "carryover": {
                "mar": ["Legacy Migration"]
            },
            "blockers": {
                "apr": ["Vendor License"]
            }
        }
        """;
        var hm = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.NotNull(hm);
        Assert.Equal(4, hm!.Shipped.Count);
        Assert.Equal(2, hm.Shipped["jan"].Count);
        Assert.Single(hm.Shipped["feb"]);
        Assert.Empty(hm.Shipped["apr"]);
        Assert.Equal(2, hm.InProgress["apr"].Count);
        Assert.Single(hm.Carryover["mar"]);
        Assert.Single(hm.Blockers["apr"]);
    }

    [Fact]
    public void HeatmapData_DeserializeEmptyCategories_Succeeds()
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
        Assert.Empty(hm.Carryover);
        Assert.Empty(hm.Blockers);
    }

    [Fact]
    public void HeatmapData_DeserializeMissingCategories_UsesDefaults()
    {
        var json = """{ "shipped": { "jan": ["A"] } }""";
        var hm = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.NotNull(hm);
        Assert.Single(hm!.Shipped);
        Assert.NotNull(hm.InProgress);
        Assert.NotNull(hm.Carryover);
        Assert.NotNull(hm.Blockers);
    }

    [Fact]
    public void HeatmapData_DeserializeWithEmptyLists_PreservesKeys()
    {
        var json = """
        {
            "shipped": { "jan": [], "feb": [], "mar": [], "apr": [] },
            "inProgress": {},
            "carryover": {},
            "blockers": {}
        }
        """;
        var hm = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.Equal(4, hm!.Shipped.Count);
        Assert.All(hm.Shipped.Values, list => Assert.Empty(list));
    }

    #endregion

    #region Round-trip serialization

    [Fact]
    public void HeatmapData_JsonRoundTrip_PreservesData()
    {
        var original = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["jan"] = new() { "Feature A", "Feature B" },
                ["feb"] = new() { "Feature C" }
            },
            InProgress = new Dictionary<string, List<string>>
            {
                ["apr"] = new() { "Feature D" }
            },
            Carryover = new Dictionary<string, List<string>>(),
            Blockers = new Dictionary<string, List<string>>
            {
                ["mar"] = new() { "Blocker 1" }
            }
        };

        var json = JsonSerializer.Serialize(original, JsonOpts);
        var deserialized = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.Equal(2, deserialized!.Shipped.Count);
        Assert.Equal(2, deserialized.Shipped["jan"].Count);
        Assert.Single(deserialized.InProgress);
        Assert.Empty(deserialized.Carryover);
        Assert.Single(deserialized.Blockers);
    }

    [Fact]
    public void HeatmapData_Serialization_UsesJsonPropertyNames()
    {
        var hm = new HeatmapData
        {
            InProgress = new Dictionary<string, List<string>>
            {
                ["apr"] = new() { "Item" }
            }
        };
        var json = JsonSerializer.Serialize(hm, JsonOpts);

        Assert.Contains("\"shipped\"", json);
        Assert.Contains("\"inProgress\"", json);
        Assert.Contains("\"carryover\"", json);
        Assert.Contains("\"blockers\"", json);
    }

    #endregion

    #region Edge cases

    [Fact]
    public void HeatmapData_LargeNumberOfItems_HandledCorrectly()
    {
        var items = Enumerable.Range(1, 50).Select(i => $"Item {i}").ToList();
        var hm = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>> { ["jan"] = items }
        };

        var json = JsonSerializer.Serialize(hm, JsonOpts);
        var deserialized = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.Equal(50, deserialized!.Shipped["jan"].Count);
    }

    [Fact]
    public void HeatmapData_ItemsWithSpecialCharacters_PreservedInRoundTrip()
    {
        var hm = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["jan"] = new() { "Feature with \"quotes\"", "Feature with <angle>", "Feature — with em dash" }
            }
        };

        var json = JsonSerializer.Serialize(hm, JsonOpts);
        var deserialized = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.Equal(3, deserialized!.Shipped["jan"].Count);
        Assert.Contains("Feature with \"quotes\"", deserialized.Shipped["jan"]);
    }

    [Fact]
    public void HeatmapData_ManyMonthKeys_AllPreserved()
    {
        var hm = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["jan"] = new() { "A" },
                ["feb"] = new() { "B" },
                ["mar"] = new() { "C" },
                ["apr"] = new() { "D" },
                ["may"] = new() { "E" },
                ["jun"] = new() { "F" }
            }
        };

        var json = JsonSerializer.Serialize(hm, JsonOpts);
        var deserialized = JsonSerializer.Deserialize<HeatmapData>(json, JsonOpts);

        Assert.Equal(6, deserialized!.Shipped.Count);
    }

    #endregion
}