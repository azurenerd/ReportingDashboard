using Bunit;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for HeatmapRow.razor targeting the ACTUAL component implementation.
/// Existing HeatmapRowTests.cs uses wrong namespace and wrong CSS classes.
/// </summary>
[Trait("Category", "Unit")]
public class HeatmapRowActualTests : TestContext
{
    #region Row Header Rendering

    [Fact]
    public void HeatmapRow_RendersRowHeaderWithCategoryLabel()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        var hdr = cut.Find(".hm-row-hdr");
        Assert.Contains("SHIPPED", hdr.TextContent);
    }

    [Theory]
    [InlineData("ship", "ship-hdr")]
    [InlineData("prog", "prog-hdr")]
    [InlineData("carry", "carry-hdr")]
    [InlineData("block", "block-hdr")]
    public void HeatmapRow_RowHeaderHasCorrectPrefixClass(string prefix, string expectedClass)
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "Test")
            .Add(x => x.CssPrefix, prefix)
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        var hdr = cut.Find(".hm-row-hdr");
        Assert.Contains(expectedClass, hdr.GetAttribute("class")!);
    }

    #endregion

    #region Cell Count Per Month

    [Fact]
    public void HeatmapRow_RendersOneCellPerMonth()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr" };
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var cells = cut.FindAll(".hm-cell");
        Assert.Equal(4, cells.Count);
    }

    [Fact]
    public void HeatmapRow_EmptyMonths_RendersNoCells()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, ""));

        var cells = cut.FindAll(".hm-cell");
        Assert.Empty(cells);
    }

    [Fact]
    public void HeatmapRow_TwelveMonths_RendersTwelveCells()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                         "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        Assert.Equal(12, cut.FindAll(".hm-cell").Count);
    }

    #endregion

    #region Month Key Mapping (ToLowerInvariant)

    [Fact]
    public void HeatmapRow_MapsLowercaseMonthKeyToItems()
    {
        var items = new Dictionary<string, List<string>>
        {
            ["jan"] = new() { "Feature A" },
            ["feb"] = new() { "Feature B", "Feature C" }
        };

        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "Jan", "Feb" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("Feature A", cut.Markup);
        Assert.Contains("Feature B", cut.Markup);
        Assert.Contains("Feature C", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_MissingMonthKey_RendersEmptyDash()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Mar" })
            .Add(x => x.CurrentMonth, ""));

        Assert.NotNull(cut.Find(".hm-empty"));
        Assert.Contains("-", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_MixedPresentAndMissingKeys_RendersCorrectly()
    {
        var items = new Dictionary<string, List<string>>
        {
            ["jan"] = new() { "Item 1" }
        };

        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "Jan", "Feb" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("Item 1", cut.Markup);
        Assert.NotNull(cut.Find(".hm-empty"));
    }

    #endregion

    #region Current Month Propagation

    [Fact]
    public void HeatmapRow_CurrentMonth_CellGetsAprClass()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Apr" })
            .Add(x => x.CurrentMonth, "Apr"));

        var cell = cut.Find(".hm-cell");
        Assert.Contains("apr", cell.GetAttribute("class")!);
    }

    [Fact]
    public void HeatmapRow_NonCurrentMonth_CellDoesNotGetAprClass()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Apr"));

        var cell = cut.Find(".hm-cell");
        Assert.DoesNotContain(" apr", cell.GetAttribute("class")!);
    }

    [Fact]
    public void HeatmapRow_CaseInsensitiveCurrentMonthMatch()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "APRIL" })
            .Add(x => x.CurrentMonth, "april"));

        var cell = cut.Find(".hm-cell");
        Assert.Contains("apr", cell.GetAttribute("class")!);
    }

    [Fact]
    public void HeatmapRow_MultipleMonths_OnlyCurrentGetsApr()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr" })
            .Add(x => x.CurrentMonth, "Apr"));

        var cells = cut.FindAll(".hm-cell");
        Assert.Equal(4, cells.Count);

        for (int i = 0; i < 3; i++)
        {
            Assert.DoesNotContain(" apr", cells[i].GetAttribute("class")!);
        }
        Assert.Contains("apr", cells[3].GetAttribute("class")!);
    }

    #endregion

    #region Cell CSS Prefix Propagation

    [Fact]
    public void HeatmapRow_CellsInheritCssPrefix()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "BLOCKERS")
            .Add(x => x.CssPrefix, "block")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        var cell = cut.Find(".hm-cell");
        Assert.Contains("block-cell", cell.GetAttribute("class")!);
    }

    #endregion
}