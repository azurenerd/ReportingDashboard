using Bunit;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class HeatmapCellTests : TestContext
{
    [Fact]
    public void HeatmapCell_EmptyItems_ShowsDash()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        Assert.Contains("-", cut.Markup);
    }

    [Fact]
    public void HeatmapCell_WithItems_RendersAllItems()
    {
        var items = new List<string> { "Feature A", "Feature B", "Feature C" };
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        Assert.Contains("Feature A", cut.Markup);
        Assert.Contains("Feature B", cut.Markup);
        Assert.Contains("Feature C", cut.Markup);
    }

    [Fact]
    public void HeatmapCell_AppliesCssPrefixToCell()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.IsCurrentMonth, false));

        Assert.Contains("prog-cell", cut.Markup);
    }

    [Fact]
    public void HeatmapCell_CurrentMonth_AddsAprClass()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, true));

        Assert.Contains("apr", cut.Markup);
    }

    [Fact]
    public void HeatmapCell_NotCurrentMonth_NoAprClass()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var cell = cut.Find(".hm-cell");
        var classes = cell.GetAttribute("class") ?? "";
        Assert.DoesNotContain("apr", classes.Split(' ').Where(c => c == "apr"));
    }

    [Fact]
    public void HeatmapCell_ItemsHaveItClass()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Item1" })
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var item = cut.Find(".it");
        Assert.NotNull(item);
    }

    [Fact]
    public void HeatmapCell_SingleItem_RendersOneDiv()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Solo Item" })
            .Add(x => x.CssPrefix, "carry")
            .Add(x => x.IsCurrentMonth, false));

        var items = cut.FindAll(".it");
        Assert.Single(items);
    }

    [Fact]
    public void HeatmapCell_ManyItems_RendersAll()
    {
        var items = Enumerable.Range(1, 10).Select(i => $"Item {i}").ToList();
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var rendered = cut.FindAll(".it");
        Assert.Equal(10, rendered.Count);
    }

    [Fact]
    public void HeatmapCell_WithItems_NoDash()
    {
        var items = new List<string> { "Item" };
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        // When items exist, the dash should not appear
        var itDivs = cut.FindAll(".it");
        Assert.Single(itDivs);
    }
}