using Bunit;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for HeatmapCell.razor targeting the ACTUAL component implementation.
/// The existing HeatmapCellTests.cs references wrong namespace (Components.Sections)
/// and wrong CSS classes (empty-cell, cur, block-dot). This file tests the real markup.
/// </summary>
[Trait("Category", "Unit")]
public class HeatmapCellActualTests : TestContext
{
    #region Empty/Null Items - Dash Placeholder

    [Fact]
    public void HeatmapCell_EmptyItems_RendersHmEmptyWithDash()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var empty = cut.Find(".hm-empty");
        Assert.Equal("-", empty.TextContent);
    }

    [Fact]
    public void HeatmapCell_NullItems_RendersHmEmptyWithDash()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, (List<string>)null!)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var empty = cut.Find(".hm-empty");
        Assert.Equal("-", empty.TextContent);
    }

    [Fact]
    public void HeatmapCell_EmptyItems_DoesNotRenderItDivs()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var items = cut.FindAll(".it");
        Assert.Empty(items);
    }

    #endregion

    #region Items Rendering

    [Fact]
    public void HeatmapCell_WithItems_RendersAllItemsAsItDivs()
    {
        var items = new List<string> { "Feature A", "Feature B", "Feature C" };
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var rendered = cut.FindAll(".it");
        Assert.Equal(3, rendered.Count);
        Assert.Equal("Feature A", rendered[0].TextContent);
        Assert.Equal("Feature B", rendered[1].TextContent);
        Assert.Equal("Feature C", rendered[2].TextContent);
    }

    [Fact]
    public void HeatmapCell_WithItems_DoesNotRenderHmEmpty()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Item" })
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var empties = cut.FindAll(".hm-empty");
        Assert.Empty(empties);
    }

    [Fact]
    public void HeatmapCell_SingleItem_RendersOneItDiv()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Solo" })
            .Add(x => x.CssPrefix, "carry")
            .Add(x => x.IsCurrentMonth, false));

        Assert.Single(cut.FindAll(".it"));
    }

    [Fact]
    public void HeatmapCell_ManyItems_RendersAll()
    {
        var items = Enumerable.Range(1, 15).Select(i => $"Item {i}").ToList();
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        Assert.Equal(15, cut.FindAll(".it").Count);
    }

    #endregion

    #region CSS Class Construction

    [Theory]
    [InlineData("ship", "ship-cell")]
    [InlineData("prog", "prog-cell")]
    [InlineData("carry", "carry-cell")]
    [InlineData("block", "block-cell")]
    public void HeatmapCell_AppliesCssPrefixCellClass(string prefix, string expectedClass)
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, prefix)
            .Add(x => x.IsCurrentMonth, false));

        var cell = cut.Find(".hm-cell");
        Assert.Contains(expectedClass, cell.GetAttribute("class")!);
    }

    [Fact]
    public void HeatmapCell_IsCurrentMonth_AddsAprClass()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, true));

        var cell = cut.Find(".hm-cell");
        var classes = cell.GetAttribute("class")!;
        Assert.Contains("apr", classes);
        Assert.Contains("ship-cell", classes);
    }

    [Fact]
    public void HeatmapCell_NotCurrentMonth_NoAprClass()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var cell = cut.Find(".hm-cell");
        var classes = cell.GetAttribute("class")!;
        Assert.DoesNotContain(" apr", classes);
    }

    [Fact]
    public void HeatmapCell_CurrentMonthWithItems_HasAprAndItems()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Item A" })
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.IsCurrentMonth, true));

        var cell = cut.Find(".hm-cell");
        Assert.Contains("apr", cell.GetAttribute("class")!);
        Assert.Contains("prog-cell", cell.GetAttribute("class")!);
        Assert.Equal("Item A", cut.Find(".it").TextContent);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void HeatmapCell_EmptyStringItem_StillRendersItDiv()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "" })
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        Assert.Single(cut.FindAll(".it"));
    }

    [Fact]
    public void HeatmapCell_EmptyCssPrefix_StillRendersCell()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Test" })
            .Add(x => x.CssPrefix, "")
            .Add(x => x.IsCurrentMonth, false));

        Assert.NotNull(cut.Find(".hm-cell"));
    }

    [Fact]
    public void HeatmapCell_SpecialCharactersInItems_RendersEncoded()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Feature <script>alert('xss')</script>" })
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        Assert.DoesNotContain("<script>", cut.Markup);
        Assert.Contains("&lt;script&gt;", cut.Markup);
    }

    #endregion
}