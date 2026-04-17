using Bunit;
using FluentAssertions;
using ReportingDashboard.Components.Shared;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class HeatmapCellComponentTests : TestContext
{
    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapCell_WithItems_RendersItemDivs()
    {
        var items = new List<string> { "Feature A", "Feature B", "Feature C" };

        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssClass, "ship-cell"));

        var itemDivs = cut.FindAll("div.it");
        itemDivs.Should().HaveCount(3);
        itemDivs[0].TextContent.Should().Be("Feature A");
        itemDivs[1].TextContent.Should().Be("Feature B");
        itemDivs[2].TextContent.Should().Be("Feature C");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapCell_WithNullItems_RendersDash()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, (List<string>?)null)
            .Add(x => x.CssClass, "ship-cell"));

        var span = cut.Find("span");
        span.TextContent.Should().Be("-");
        span.GetAttribute("style").Should().Contain("color:#999");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapCell_WithEmptyList_RendersDash()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssClass, "prog-cell"));

        var span = cut.Find("span");
        span.TextContent.Should().Be("-");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapCell_AppliesCssClassToRoot()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, (List<string>?)null)
            .Add(x => x.CssClass, "block-cell current"));

        var root = cut.Find("div.hm-cell");
        root.ClassName.Should().Contain("block-cell");
        root.ClassName.Should().Contain("current");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapCell_DefaultCssClass_IsEmpty()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Item1" }));

        var root = cut.Find("div");
        root.ClassName!.Trim().Should().Be("hm-cell");
    }
}