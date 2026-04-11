using Bunit;
using ReportingDashboard.Components.Sections;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class HeatmapRowTests : TestContext
{
    [Fact]
    public void HeatmapRow_RendersCategoryLabel()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, "January"));

        Assert.Contains("SHIPPED", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_AppliesCssPrefixToHeader()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "IN PROGRESS")
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("prog-hdr", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_RendersOneCellPerMonth()
    {
        var months = new List<string> { "January", "February", "March", "April" };
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
    public void HeatmapRow_MapsMonthKeyToItems()
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
            .Add(x => x.Months, new List<string> { "January", "February" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("Feature A", cut.Markup);
        Assert.Contains("Feature B", cut.Markup);
        Assert.Contains("Feature C", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_MissingMonthKey_RendersDash()
    {
        var items = new Dictionary<string, List<string>>();

        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "March" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("empty-cell", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_CurrentMonthCell_GetsCurClass()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "April" })
            .Add(x => x.CurrentMonth, "April"));

        Assert.Contains("cur", cut.Markup);
    }

    [Fact]
    public void HeatmapRow_NonCurrentMonth_NoCurClass()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, "April"));

        var cells = cut.FindAll(".hm-cell");
        foreach (var cell in cells)
        {
            var classes = cell.GetAttribute("class") ?? "";
            Assert.DoesNotContain(" cur", classes);
        }
    }

    [Fact]
    public void HeatmapRow_HasRowHeaderClass()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "BLOCKERS")
            .Add(x => x.CssPrefix, "block")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("hm-row-hdr", cut.Markup);
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

        Assert.Contains("cur", cut.Markup);
    }
}