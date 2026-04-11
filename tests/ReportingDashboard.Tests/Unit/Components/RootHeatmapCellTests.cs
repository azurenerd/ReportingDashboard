using Bunit;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Unit tests for Components/HeatmapCell.razor (root-level version from PR #521).
/// Uses "hm-empty" class for empty cells and computes CellClass from CssPrefix + IsCurrentMonth.
/// </summary>
[Trait("Category", "Unit")]
public class RootHeatmapCellTests : TestContext
{
    [Fact]
    public void HeatmapCell_EmptyItems_ShowsDash()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        Assert.Contains("-", cut.Markup);
        Assert.Contains("hm-empty", cut.Markup);
    }

    [Fact]
    public void HeatmapCell_NullItems_ShowsDash()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, (List<string>)null!)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        Assert.Contains("-", cut.Markup);
        Assert.Contains("hm-empty", cut.Markup);
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
    public void HeatmapCell_WithItems_NoEmptyMarker()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Item" })
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        Assert.DoesNotContain("hm-empty", cut.Markup);
    }

    [Fact]
    public void HeatmapCell_CellClass_ContainsPrefixCell()
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
        var classes = cell.GetAttribute("class") ?? "";
        Assert.Contains("apr", classes);
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
        Assert.DoesNotContain(" apr", classes);
    }

    [Fact]
    public void HeatmapCell_CellClass_ShipCurrentMonth_IsShipCellApr()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, true));

        var cell = cut.Find(".hm-cell");
        var classes = cell.GetAttribute("class") ?? "";
        Assert.Contains("ship-cell", classes);
        Assert.Contains("apr", classes);
    }

    [Fact]
    public void HeatmapCell_ItemsRenderedWithItClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Item 1" })
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var items = cut.FindAll(".it");
        Assert.Single(items);
        Assert.Contains("Item 1", items[0].TextContent);
    }

    [Fact]
    public void HeatmapCell_ManyItems_RendersAll()
    {
        var items = Enumerable.Range(1, 15).Select(i => $"Item {i}").ToList();
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "block")
            .Add(x => x.IsCurrentMonth, false));

        var rendered = cut.FindAll(".it");
        Assert.Equal(15, rendered.Count);
    }

    [Fact]
    public void HeatmapCell_AllCssPrefixes_GenerateCorrectClass()
    {
        var prefixes = new[] { "ship", "prog", "carry", "block" };
        foreach (var prefix in prefixes)
        {
            var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
                .Add(x => x.Items, new List<string>())
                .Add(x => x.CssPrefix, prefix)
                .Add(x => x.IsCurrentMonth, false));

            var cell = cut.Find(".hm-cell");
            Assert.Contains($"{prefix}-cell", cell.GetAttribute("class") ?? "");
        }
    }

    [Fact]
    public void HeatmapCell_EmptyPrefix_StillRendersCell()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "")
            .Add(x => x.IsCurrentMonth, false));

        var cell = cut.Find(".hm-cell");
        Assert.Contains("-cell", cell.GetAttribute("class") ?? "");
    }

    [Fact]
    public void HeatmapCell_ItemWithSpecialChars_RendersEncoded()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Feature <Beta> & \"Release\"" })
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        Assert.DoesNotContain("<Beta>", cut.Markup);
        Assert.Contains("&lt;Beta&gt;", cut.Markup);
    }
}