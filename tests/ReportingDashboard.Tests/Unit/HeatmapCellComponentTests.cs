using Bunit;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class HeatmapCellComponentTests : TestContext
{
    [Fact]
    public void RendersItems_WhenListIsNonEmpty()
    {
        var items = new List<string> { "Feature A", "Feature B" };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsHighlighted, false)
            .Add(x => x.IsLastColumn, false));

        var itemDivs = cut.FindAll(".it");
        itemDivs.Should().HaveCount(2);
        itemDivs[0].TextContent.Should().Be("Feature A");
        itemDivs[1].TextContent.Should().Be("Feature B");
    }

    [Fact]
    public void RendersEmptyDash_WhenListIsEmpty()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsHighlighted, false)
            .Add(x => x.IsLastColumn, false));

        var empty = cut.Find(".it.empty");
        // The component renders a dash character; verify the element exists and has single-char content
        empty.Should().NotBeNull();
        empty.TextContent.Should().HaveLength(1);
    }

    [Fact]
    public void AppliesHighlightClass_WhenIsHighlightedTrue()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Item" })
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.IsHighlighted, true)
            .Add(x => x.IsLastColumn, false));

        var cell = cut.Find(".hm-cell");
        cell.ClassList.Should().Contain("apr");
        cell.ClassList.Should().Contain("prog-cell");
    }

    [Fact]
    public void AppliesCategoryPrefix_ToCell()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "block")
            .Add(x => x.IsHighlighted, false)
            .Add(x => x.IsLastColumn, false));

        var cell = cut.Find(".hm-cell");
        cell.ClassList.Should().Contain("block-cell");
    }

    [Fact]
    public void RemovesBorderRight_WhenIsLastColumn()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.IsHighlighted, false)
            .Add(x => x.IsLastColumn, true));

        var cell = cut.Find(".hm-cell");
        cell.GetAttribute("style").Should().Contain("border-right:none;");
    }
}