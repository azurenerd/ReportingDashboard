using Xunit;
using ReportingDashboard.Models;

namespace ReportingDashboard.Tests;

public class HeatmapDataTests
{
    [Fact]
    public void GetItems_ExistingMonth_ReturnsItemList()
    {
        var category = CreateCategory("shipped", new Dictionary<string, List<string>>
        {
            ["Jan"] = new() { "Feature A", "Feature B" },
            ["Feb"] = new() { "Feature C" }
        });

        var items = GetItems(category, "Jan");

        Assert.Equal(2, items.Count);
        Assert.Equal("Feature A", items[0]);
        Assert.Equal("Feature B", items[1]);
    }

    [Fact]
    public void GetItems_MissingMonth_ReturnsEmptyList()
    {
        var category = CreateCategory("shipped", new Dictionary<string, List<string>>
        {
            ["Jan"] = new() { "Feature A" }
        });

        var items = GetItems(category, "Mar");

        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public void GetItems_EmptyMonthList_ReturnsEmptyList()
    {
        var category = CreateCategory("shipped", new Dictionary<string, List<string>>
        {
            ["Jan"] = new()
        });

        var items = GetItems(category, "Jan");

        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public void GetItems_EmptyItemsDictionary_ReturnsEmptyList()
    {
        var category = CreateCategory("shipped", new Dictionary<string, List<string>>());

        var items = GetItems(category, "Jan");

        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public void CategoryKey_AllValidKeysAccepted()
    {
        var validKeys = new[] { "shipped", "inProgress", "carryover", "blockers" };

        foreach (var key in validKeys)
        {
            var category = CreateCategory(key, new Dictionary<string, List<string>>());
            Assert.Equal(key, category.Key);
        }
    }

    [Fact]
    public void HeatmapCategory_ItemsPreservesOrder()
    {
        var category = CreateCategory("shipped", new Dictionary<string, List<string>>
        {
            ["Apr"] = new() { "First", "Second", "Third" }
        });

        var items = GetItems(category, "Apr");

        Assert.Equal(3, items.Count);
        Assert.Equal("First", items[0]);
        Assert.Equal("Second", items[1]);
        Assert.Equal("Third", items[2]);
    }

    [Fact]
    public void HeatmapCategory_MultipleMonths_IndependentItemLists()
    {
        var category = CreateCategory("inProgress", new Dictionary<string, List<string>>
        {
            ["Jan"] = new() { "A" },
            ["Feb"] = new() { "B", "C" },
            ["Mar"] = new() { "D", "E", "F" }
        });

        Assert.Single(GetItems(category, "Jan"));
        Assert.Equal(2, GetItems(category, "Feb").Count);
        Assert.Equal(3, GetItems(category, "Mar").Count);
    }

    [Fact]
    public void HeatmapCategory_MaxItemsPerCell_HandledCorrectly()
    {
        var items = Enumerable.Range(1, 8).Select(i => $"Item {i}").ToList();
        var category = CreateCategory("shipped", new Dictionary<string, List<string>>
        {
            ["Apr"] = items
        });

        var result = GetItems(category, "Apr");
        Assert.Equal(8, result.Count);
    }

    [Fact]
    public void GetItems_CaseSensitiveMonthKey()
    {
        var category = CreateCategory("shipped", new Dictionary<string, List<string>>
        {
            ["Jan"] = new() { "Feature A" }
        });

        // "jan" (lowercase) should not match "Jan"
        var items = GetItems(category, "jan");
        Assert.Empty(items);
    }

    /// <summary>
    /// Mirrors the static GetItems helper used in Heatmap.razor
    /// </summary>
    private static List<string> GetItems(HeatmapCategory category, string month)
    {
        if (category.Items.TryGetValue(month, out var items))
            return items;
        return new List<string>();
    }

    private static HeatmapCategory CreateCategory(string key, Dictionary<string, List<string>> items) => new()
    {
        Name = key switch
        {
            "shipped" => "Shipped",
            "inProgress" => "In Progress",
            "carryover" => "Carryover",
            "blockers" => "Blockers",
            _ => key
        },
        Key = key,
        Items = items
    };
}