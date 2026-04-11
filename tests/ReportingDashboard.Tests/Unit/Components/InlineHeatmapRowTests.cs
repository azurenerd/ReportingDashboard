using Bunit;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for Components/HeatmapRow.razor (root-level version).
/// Uses ToLowerInvariant() for month key lookup and case-insensitive current month matching.
/// </summary>
[Trait("Category", "Unit")]
public class InlineHeatmapRowTests : TestContext
{
    [Fact]
    public void HeatmapRow_RendersCategoryLabel()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✓ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Jan"));

        Assert.Contains("SHIPPED", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_AppliesCssPrefixToHeader()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "IN PROGRESS")
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("prog-hdr", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_RendersOneCellPerMonth()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr" };
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var cells = cut.FindAll(".hm-cell");
        Assert.Equal(4, cells.Count);
    }

    [Fact]
    public void HeatmapRow_LowercaseMonthKeyLookup_FindsItems()
    {
        var items = new Dictionary<string, List<string>>
        {
            ["jan"] = new() { "Feature A" }
        };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("Feature A", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_OriginalCaseKeyLookup_FindsItems()
    {
        // The component tries lowercase first, then original case as fallback
        var items = new Dictionary<string, List<string>>
        {
            ["Jan"] = new() { "Feature B" }
        };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("Feature B", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_MissingMonthKey_RendersDash()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Mar" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("-", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_CurrentMonthCell_GetsAprClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Apr" })
            .Add(x => x.CurrentMonth, "Apr"));

        var cells = cut.FindAll(".hm-cell");
        Assert.Contains("apr", cells[0].GetAttribute("class") ?? "");
    }

    [Fact]
    public void HeatmapRow_CaseInsensitiveCurrentMonthMatch()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "APRIL" })
            .Add(x => x.CurrentMonth, "april"));

        var cells = cut.FindAll(".hm-cell");
        Assert.Contains("apr", cells[0].GetAttribute("class") ?? "");
    }

    [Fact]
    public void HeatmapRow_NonCurrentMonthCell_NoAprClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Apr"));

        var cells = cut.FindAll(".hm-cell");
        var cls = cells[0].GetAttribute("class") ?? "";
        Assert.DoesNotContain(" apr", cls);
    }

    [Fact]
    public void HeatmapRow_MultipleMonths_ItemsMappedCorrectly()
    {
        var items = new Dictionary<string, List<string>>
        {
            ["jan"] = new() { "Item A" },
            ["mar"] = new() { "Item B", "Item C" }
        };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("Item A", cut.Markup);
        Assert.Contains("Item B", cut.Markup);
        Assert.Contains("Item C", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_HasRowHeaderClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "BLOCKERS")
            .Add(x => x.CssPrefix, "block")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("hm-row-hdr", cut.Markup);
    }
}