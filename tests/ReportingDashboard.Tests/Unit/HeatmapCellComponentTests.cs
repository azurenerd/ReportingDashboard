using Bunit;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class HeatmapCellComponentTests : TestContext
{
    [Fact]
    public void RendersItems_WithCorrectCount()
    {
        var items = new List<string> { "Feature A", "Feature B", "Feature C" };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.CssClass, "ship-cell")
            .Add(x => x.AccentClass, "ship"));

        var renderedItems = cut.FindAll(".it");
        renderedItems.Should().HaveCount(3);
        cut.Markup.Should().Contain("Feature A");
        cut.Markup.Should().Contain("Feature B");
    }

    [Fact]
    public void RendersEmptyDash_WhenNoItems()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.CssClass, "prog-cell")
            .Add(x => x.AccentClass, "prog"));

        cut.Find(".it.empty").TextContent.Should().Contain("-");
    }

    [Fact]
    public void AppliesHighlightClass_WhenHighlighted()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Item" })
            .Add(x => x.CssClass, "carry-cell")
            .Add(x => x.IsHighlighted, true)
            .Add(x => x.AccentClass, "carry"));

        var cell = cut.Find(".hm-cell");
        cell.ClassList.Should().Contain("carry-hl");
    }

    [Fact]
    public void NoHighlightClass_WhenNotHighlighted()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Item" })
            .Add(x => x.CssClass, "block-cell")
            .Add(x => x.IsHighlighted, false)
            .Add(x => x.AccentClass, "block"));

        var cell = cut.Find(".hm-cell");
        cell.ClassList.Should().NotContain("block-hl");
    }

    [Fact]
    public void RemovesBorderRight_WhenLastColumn()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Item" })
            .Add(x => x.CssClass, "ship-cell")
            .Add(x => x.IsLastColumn, true)
            .Add(x => x.AccentClass, "ship"));

        var cell = cut.Find(".hm-cell");
        cell.GetAttribute("style").Should().Contain("border-right:none");
    }
}