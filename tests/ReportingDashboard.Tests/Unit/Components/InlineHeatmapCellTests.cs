using Bunit;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for Components/HeatmapCell.razor (root-level inline-styled version).
/// Uses 'apr' class for current month (not 'cur'), inline-styled empty state,
/// and 'it' class for items without CSS-prefix dots.
/// </summary>
[Trait("Category", "Unit")]
public class InlineHeatmapCellTests : TestContext
{
    [Fact]
    public void HeatmapCell_EmptyItems_ShowsDash()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        Assert.Contains("-", cut.Markup);
    }

    [Fact]
    public void HeatmapCell_EmptyItems_DashHasInlineStyle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        // Root version uses inline style color:#AAA; text-align:center
        Assert.Contains("color:#AAA", cut.Markup);
        Assert.Contains("text-align:center", cut.Markup);
    }

    [Fact]
    public void HeatmapCell_NullItems_ShowsDash()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, (List<string>)null!)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        Assert.Contains("-", cut.Markup);
    }

    [Fact]
    public void HeatmapCell_WithItems_RendersAll()
    {
        var items = new List<string> { "Feature A", "Feature B", "Feature C" };
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        Assert.Contains("Feature A", cut.Markup);
        Assert.Contains("Feature B", cut.Markup);
        Assert.Contains("Feature C", cut.Markup);
    }

    [Fact]
    public void HeatmapCell_WithItems_UsesItClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Item 1" })
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var item = cut.Find(".it");
        Assert.NotNull(item);
        Assert.Equal("Item 1", item.TextContent);
    }

    [Fact]
    public void HeatmapCell_AppliesCssPrefixToCell()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.IsCurrentMonth, false));

        var cell = cut.Find(".hm-cell");
        Assert.Contains("prog-cell", cell.GetAttribute("class") ?? "");
    }

    [Fact]
    public void HeatmapCell_CurrentMonth_AddsAprClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, true));

        var cell = cut.Find(".hm-cell");
        Assert.Contains("apr", cell.GetAttribute("class") ?? "");
    }

    [Fact]
    public void HeatmapCell_NotCurrentMonth_NoAprClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var cell = cut.Find(".hm-cell");
        var classes = cell.GetAttribute("class") ?? "";
        // Should not have standalone "apr" class
        Assert.DoesNotContain(" apr", classes);
    }

    [Fact]
    public void HeatmapCell_ManyItems_RendersAll()
    {
        var items = Enumerable.Range(1, 8).Select(i => $"Item {i}").ToList();
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var rendered = cut.FindAll(".it");
        Assert.Equal(8, rendered.Count);
    }

    [Fact]
    public void HeatmapCell_SingleItem_RendersOneIt()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Solo" })
            .Add(x => x.CssPrefix, "carry")
            .Add(x => x.IsCurrentMonth, false));

        var items = cut.FindAll(".it");
        Assert.Single(items);
    }

    [Fact]
    public void HeatmapCell_WithItems_NoDashDisplayed()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Something" })
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        // Should not contain the dash placeholder
        Assert.DoesNotContain("color:#AAA", cut.Markup);
    }

    [Fact]
    public void HeatmapCell_SpecialCharacters_EncodedInItems()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "<script>alert(1)</script>" })
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        Assert.DoesNotContain("<script>alert", cut.Markup);
        Assert.Contains("&lt;script&gt;", cut.Markup);
    }
}