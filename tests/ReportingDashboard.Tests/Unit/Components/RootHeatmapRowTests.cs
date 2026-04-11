using Bunit;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Unit tests for Components/HeatmapRow.razor (root-level version from PR #521).
/// Uses ToLowerInvariant() for month key matching to items dictionary.
/// </summary>
[Trait("Category", "Unit")]
public class RootHeatmapRowTests : TestContext
{
    [Fact]
    public void HeatmapRow_RendersCategoryLabel()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("SHIPPED", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_AppliesCssPrefixToHeader()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "IN PROGRESS")
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("prog-hdr", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_RendersOneCellPerMonth()
    {
        var months = new List<string> { "January", "February", "March", "April" };
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
    public void HeatmapRow_MatchesItemsByLowercaseMonthKey()
    {
        // The component calls month.ToLowerInvariant() to look up items
        var items = new Dictionary<string, List<string>>
        {
            ["january"] = new() { "Feature A" },
            ["february"] = new() { "Feature B" }
        };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "January", "February" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("Feature A", cut.Markup);
        Assert.Contains("Feature B", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_MissingMonthKey_RendersDash()
    {
        var items = new Dictionary<string, List<string>>();

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "March" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("hm-empty", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_CurrentMonthCell_GetsAprClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "April" })
            .Add(x => x.CurrentMonth, "April"));

        Assert.Contains("apr", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_CaseInsensitiveCurrentMonthComparison()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "APRIL" })
            .Add(x => x.CurrentMonth, "april"));

        var cells = cut.FindAll(".hm-cell");
        Assert.Single(cells);
        Assert.Contains("apr", cells[0].GetAttribute("class") ?? "");
    }

    [Fact]
    public void HeatmapRow_NonCurrentMonthCell_NoAprClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, "April"));

        var cells = cut.FindAll(".hm-cell");
        foreach (var cell in cells)
        {
            Assert.DoesNotContain(" apr", cell.GetAttribute("class") ?? "");
        }
    }

    [Fact]
    public void HeatmapRow_HasRowHeaderClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "BLOCKERS")
            .Add(x => x.CssPrefix, "block")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("hm-row-hdr", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_EmptyMonthsList_RendersHeaderOnly()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("hm-row-hdr", cut.Markup);
        var cells = cut.FindAll(".hm-cell");
        Assert.Empty(cells);
    }

    [Fact]
    public void HeatmapRow_NullItemsValue_RendersEmptyCell()
    {
        // Dictionary has key but value is null - component guards against this
        var items = new Dictionary<string, List<string>>
        {
            ["january"] = null!
        };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        // Should not crash; renders empty cell
        Assert.Contains("hm-empty", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_MultipleItemsInMonth_AllRendered()
    {
        var items = new Dictionary<string, List<string>>
        {
            ["january"] = new() { "A", "B", "C", "D", "E" }
        };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        var renderedItems = cut.FindAll(".it");
        Assert.Equal(5, renderedItems.Count);
    }
}