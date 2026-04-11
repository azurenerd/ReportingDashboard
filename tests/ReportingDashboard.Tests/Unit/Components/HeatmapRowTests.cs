using Bunit;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class HeatmapRowTests : TestContext
{
    [Fact]
    public void HeatmapRow_ShouldRenderRowHeader()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.Category, "shipped")
            .Add(x => x.Label, "✅ Shipped")
            .Add(x => x.Months, new List<string> { "Jan", "Feb" })
            .Add(x => x.CurrentMonth, "Feb")
            .Add(x => x.Items, new Dictionary<string, List<string>>()));

        cut.Find(".hm-row-hdr").TextContent.Should().Contain("Shipped");
    }

    [Fact]
    public void HeatmapRow_ShouldApplyCategoryCssClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.Category, "shipped")
            .Add(x => x.Label, "Shipped")
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Jan")
            .Add(x => x.Items, new Dictionary<string, List<string>>()));

        cut.Find(".shipped-hdr").Should().NotBeNull();
    }

    [Fact]
    public void HeatmapRow_ShouldRenderCellForEachMonth()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr" };
        var items = new Dictionary<string, List<string>>
        {
            ["Jan"] = new() { "A" },
            ["Mar"] = new() { "B", "C" }
        };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.Category, "prog")
            .Add(x => x.Label, "In Progress")
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Mar")
            .Add(x => x.Items, items));

        var cells = cut.FindAll(".hm-cell");
        cells.Should().HaveCount(4);
    }

    [Fact]
    public void HeatmapRow_MonthWithNoItems_ShouldRenderEmptyCell()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.Category, "shipped")
            .Add(x => x.Label, "Shipped")
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "")
            .Add(x => x.Items, new Dictionary<string, List<string>>()));

        cut.FindAll(".empty-cell").Should().HaveCount(1);
    }

    [Fact]
    public void HeatmapRow_CurrentMonth_ShouldMarkCellAsCurrent()
    {
        var items = new Dictionary<string, List<string>>
        {
            ["Feb"] = new() { "Item" }
        };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.Category, "shipped")
            .Add(x => x.Label, "Shipped")
            .Add(x => x.Months, new List<string> { "Jan", "Feb" })
            .Add(x => x.CurrentMonth, "Feb")
            .Add(x => x.Items, items));

        cut.Markup.Should().Contain("shipped-cur");
    }

    [Fact]
    public void HeatmapRow_EmptyMonths_ShouldRenderOnlyHeader()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.Category, "block")
            .Add(x => x.Label, "Blockers")
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, "")
            .Add(x => x.Items, new Dictionary<string, List<string>>()));

        cut.Find(".hm-row-hdr").Should().NotBeNull();
        cut.FindAll(".hm-cell").Should().BeEmpty();
    }

    [Theory]
    [InlineData("shipped", "shipped-hdr")]
    [InlineData("prog", "prog-hdr")]
    [InlineData("carry", "carry-hdr")]
    [InlineData("block", "block-hdr")]
    public void HeatmapRow_AllCategories_ShouldApplyCorrectHeaderClass(string category, string expectedClass)
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.Category, category)
            .Add(x => x.Label, "Label")
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "")
            .Add(x => x.Items, new Dictionary<string, List<string>>()));

        cut.Find($".{expectedClass}").Should().NotBeNull();
    }
}