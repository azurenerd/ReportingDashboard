using Bunit;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class HeatmapCellTests : TestContext
{
    [Fact]
    public void HeatmapCell_WithItems_ShouldRenderAllItems()
    {
        var items = new List<string> { "Feature A", "Feature B", "Feature C" };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Category, "shipped")
            .Add(x => x.Items, items)
            .Add(x => x.IsCurrent, false));

        var renderedItems = cut.FindAll(".it");
        renderedItems.Should().HaveCount(3);
        renderedItems[0].TextContent.Should().Be("Feature A");
        renderedItems[1].TextContent.Should().Be("Feature B");
        renderedItems[2].TextContent.Should().Be("Feature C");
    }

    [Fact]
    public void HeatmapCell_WithNullItems_ShouldRenderDash()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Category, "shipped")
            .Add(x => x.Items, (List<string>?)null)
            .Add(x => x.IsCurrent, false));

        cut.Find(".empty-cell").Should().NotBeNull();
    }

    [Fact]
    public void HeatmapCell_WithEmptyItems_ShouldRenderDash()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Category, "shipped")
            .Add(x => x.Items, new List<string>())
            .Add(x => x.IsCurrent, false));

        cut.Find(".empty-cell").Should().NotBeNull();
    }

    [Fact]
    public void HeatmapCell_WithCategory_ShouldApplyCategoryCssClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Category, "shipped")
            .Add(x => x.Items, new List<string> { "Item" })
            .Add(x => x.IsCurrent, false));

        cut.Find(".shipped-cell").Should().NotBeNull();
    }

    [Theory]
    [InlineData("shipped")]
    [InlineData("prog")]
    [InlineData("carry")]
    [InlineData("block")]
    public void HeatmapCell_AllCategories_ShouldApplyCorrectCssClass(string category)
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Category, category)
            .Add(x => x.Items, new List<string> { "Item" })
            .Add(x => x.IsCurrent, false));

        cut.Find($".{category}-cell").Should().NotBeNull();
        cut.Find($".{category}-dot").Should().NotBeNull();
    }

    [Fact]
    public void HeatmapCell_WhenCurrent_ShouldApplyCurrentCssClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Category, "shipped")
            .Add(x => x.Items, new List<string> { "Item" })
            .Add(x => x.IsCurrent, true));

        cut.Find(".shipped-cur").Should().NotBeNull();
    }

    [Fact]
    public void HeatmapCell_WhenNotCurrent_ShouldNotApplyCurrentCssClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Category, "shipped")
            .Add(x => x.Items, new List<string> { "Item" })
            .Add(x => x.IsCurrent, false));

        cut.Markup.Should().NotContain("shipped-cur");
    }

    [Fact]
    public void HeatmapCell_WithSingleItem_ShouldRenderOneItem()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Category, "prog")
            .Add(x => x.Items, new List<string> { "Only Item" })
            .Add(x => x.IsCurrent, false));

        cut.FindAll(".it").Should().ContainSingle();
        cut.Find(".it").TextContent.Should().Be("Only Item");
    }

    [Fact]
    public void HeatmapCell_WithManyItems_ShouldRenderAll()
    {
        var items = Enumerable.Range(1, 20).Select(i => $"Item {i}").ToList();

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Category, "carry")
            .Add(x => x.Items, items)
            .Add(x => x.IsCurrent, false));

        cut.FindAll(".it").Should().HaveCount(20);
    }
}