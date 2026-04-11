using Bunit;
using FluentAssertions;
using ReportingDashboard.Tests.Unit.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class HeatmapCellTests : BunitTestBase
{
    [Fact]
    public void Render_WithItems_RendersItemDivsWithItClass()
    {
        var items = new List<string> { "Feature A", "Feature B" };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var itDivs = cut.FindAll(".it");
        itDivs.Should().HaveCount(2);
        itDivs[0].TextContent.Should().Be("Feature A");
        itDivs[1].TextContent.Should().Be("Feature B");
    }

    [Fact]
    public void Render_WithItems_DoesNotRenderEmptyDash()
    {
        var items = new List<string> { "Item 1" };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        cut.Markup.Should().NotContain("color:#AAA");
        cut.Markup.Should().NotContain(">-<");
    }

    [Fact]
    public void Render_WithNullItems_RendersGrayDash()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, (List<string>?)null)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var dashDiv = cut.Find("div > div");
        dashDiv.TextContent.Should().Be("-");
        dashDiv.GetAttribute("style").Should().Contain("color:#AAA");
        dashDiv.GetAttribute("style").Should().Contain("text-align:center");
    }

    [Fact]
    public void Render_WithEmptyList_RendersGrayDash()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.IsCurrentMonth, false));

        var dashDiv = cut.Find("div > div");
        dashDiv.TextContent.Should().Be("-");
        dashDiv.GetAttribute("style").Should().Contain("color:#AAA");
    }

    [Fact]
    public void Render_WithEmptyList_DoesNotRenderItDivs()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        cut.FindAll(".it").Should().BeEmpty();
    }

    [Fact]
    public void CellClass_WithShipPrefix_NotCurrentMonth_ReturnsCorrectClasses()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "X" })
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var outerDiv = cut.Find("div");
        outerDiv.ClassList.Should().Contain("hm-cell");
        outerDiv.ClassList.Should().Contain("ship-cell");
        outerDiv.ClassList.Should().NotContain("apr");
    }

    [Fact]
    public void CellClass_WithProgPrefix_NotCurrentMonth_ReturnsCorrectClasses()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "X" })
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.IsCurrentMonth, false));

        var outerDiv = cut.Find("div");
        outerDiv.ClassList.Should().Contain("hm-cell");
        outerDiv.ClassList.Should().Contain("prog-cell");
    }

    [Fact]
    public void CellClass_WithCarryPrefix_NotCurrentMonth_ReturnsCorrectClasses()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "X" })
            .Add(x => x.CssPrefix, "carry")
            .Add(x => x.IsCurrentMonth, false));

        var outerDiv = cut.Find("div");
        outerDiv.ClassList.Should().Contain("carry-cell");
    }

    [Fact]
    public void CellClass_WithBlockPrefix_NotCurrentMonth_ReturnsCorrectClasses()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "X" })
            .Add(x => x.CssPrefix, "block")
            .Add(x => x.IsCurrentMonth, false));

        var outerDiv = cut.Find("div");
        outerDiv.ClassList.Should().Contain("block-cell");
    }

    [Fact]
    public void CellClass_IsCurrentMonth_IncludesAprClass()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "X" })
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, true));

        var outerDiv = cut.Find("div");
        outerDiv.ClassList.Should().Contain("hm-cell");
        outerDiv.ClassList.Should().Contain("ship-cell");
        outerDiv.ClassList.Should().Contain("apr");
    }

    [Fact]
    public void CellClass_IsCurrentMonthFalse_DoesNotIncludeAprClass()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "X" })
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        var outerDiv = cut.Find("div");
        outerDiv.ClassList.Should().NotContain("apr");
    }

    [Fact]
    public void Render_WithSingleItem_RendersOneItDiv()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Only Item" })
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.IsCurrentMonth, false));

        cut.FindAll(".it").Should().HaveCount(1);
        cut.Find(".it").TextContent.Should().Be("Only Item");
    }

    [Fact]
    public void Render_WithManyItems_RendersAllItDivs()
    {
        var items = Enumerable.Range(1, 10).Select(i => $"Item {i}").ToList();

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        cut.FindAll(".it").Should().HaveCount(10);
    }

    [Fact]
    public void Render_EmptyItemsWithCurrentMonth_RendersGrayDashWithAprClass()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, true));

        var outerDiv = cut.Find("div");
        outerDiv.ClassList.Should().Contain("apr");
        cut.Find("div > div").TextContent.Should().Be("-");
    }

    [Fact]
    public void Render_DefaultCssPrefix_ProducesBaseClassOnly()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "X" })
            .Add(x => x.IsCurrentMonth, false));

        var outerDiv = cut.Find("div");
        outerDiv.ClassList.Should().Contain("hm-cell");
        outerDiv.ClassList.Should().Contain("-cell");
    }

    [Fact]
    public void Render_ItemWithSpecialCharacters_RendersCorrectly()
    {
        var items = new List<string> { "Feature <beta> & \"release\"" };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        cut.Find(".it").TextContent.Should().Contain("Feature");
        cut.Find(".it").TextContent.Should().Contain("beta");
    }

    [Fact]
    public void Render_ItemWithEmptyString_RendersItDivWithEmptyContent()
    {
        var items = new List<string> { "" };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsCurrentMonth, false));

        cut.FindAll(".it").Should().HaveCount(1);
        cut.Find(".it").TextContent.Should().BeEmpty();
    }
}