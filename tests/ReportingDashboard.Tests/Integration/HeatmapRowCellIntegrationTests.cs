using Bunit;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for HeatmapRow → HeatmapCell rendering pipeline.
/// Validates that row-level parameters correctly propagate to child cells.
/// </summary>
[Trait("Category", "Integration")]
public class HeatmapRowCellIntegrationTests : TestContext
{
    [Fact]
    public void HeatmapRow_WithMultipleMonthsAndData_RendersCorrectCellContents()
    {
        var items = new Dictionary<string, List<string>>
        {
            ["jan"] = new() { "Alpha", "Beta" },
            ["feb"] = new() { "Gamma" },
            // mar missing → dash
            ["apr"] = new() { "Delta", "Epsilon", "Zeta" }
        };

        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "🟢 SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr" })
            .Add(x => x.CurrentMonth, "Apr"));

        // 4 cells rendered
        var cells = cut.FindAll(".hm-cell");
        Assert.Equal(4, cells.Count);

        // Jan cell: 2 items
        Assert.Contains("Alpha", cut.Markup);
        Assert.Contains("Beta", cut.Markup);

        // Feb cell: 1 item
        Assert.Contains("Gamma", cut.Markup);

        // Mar cell: dash
        var empties = cut.FindAll(".hm-empty");
        Assert.Single(empties);

        // Apr cell: 3 items
        Assert.Contains("Delta", cut.Markup);
        Assert.Contains("Epsilon", cut.Markup);
        Assert.Contains("Zeta", cut.Markup);

        // Total .it divs = 2 + 1 + 3 = 6
        var itDivs = cut.FindAll(".it");
        Assert.Equal(6, itDivs.Count);
    }

    [Fact]
    public void HeatmapRow_CurrentMonth_OnlyMatchingCellGetsApr()
    {
        var items = new Dictionary<string, List<string>>();

        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr" })
            .Add(x => x.CurrentMonth, "Mar"));

        var cells = cut.FindAll(".hm-cell");
        Assert.Equal(4, cells.Count);

        // Only Mar (index 2) should have "apr" class
        Assert.DoesNotContain(" apr", cells[0].GetAttribute("class") ?? "");
        Assert.DoesNotContain(" apr", cells[1].GetAttribute("class") ?? "");
        Assert.Contains("apr", cells[2].GetAttribute("class") ?? "");
        Assert.DoesNotContain(" apr", cells[3].GetAttribute("class") ?? "");
    }

    [Fact]
    public void HeatmapRow_CssPrefixPropagation_AllCellsReceivePrefix()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "BLOCKERS")
            .Add(x => x.CssPrefix, "block")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Jan", "Feb" })
            .Add(x => x.CurrentMonth, ""));

        var cells = cut.FindAll(".hm-cell");
        Assert.Equal(2, cells.Count);
        Assert.All(cells, cell =>
            Assert.Contains("block-cell", cell.GetAttribute("class") ?? ""));
    }

    [Fact]
    public void HeatmapRow_RowHeader_RendersBeforeCells()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "🔵 IN PROGRESS")
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        // Row header appears in markup
        var header = cut.Find(".hm-row-hdr");
        Assert.Contains("IN PROGRESS", header.TextContent);
        Assert.Contains("prog-hdr", header.GetAttribute("class") ?? "");

        // Header markup appears before cell markup
        var markup = cut.Markup;
        var headerIndex = markup.IndexOf("hm-row-hdr");
        var cellIndex = markup.IndexOf("hm-cell");
        Assert.True(headerIndex < cellIndex, "Row header should appear before cells in markup");
    }

    [Fact]
    public void HeatmapRow_EmptyItemsDictionary_AllCellsAreEmpty()
    {
        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "CARRYOVER")
            .Add(x => x.CssPrefix, "carry")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar" })
            .Add(x => x.CurrentMonth, ""));

        var empties = cut.FindAll(".hm-empty");
        Assert.Equal(3, empties.Count);
        Assert.All(empties, e => Assert.Equal("-", e.TextContent));
    }

    [Fact]
    public void HeatmapRow_CaseInsensitiveMonthKeyLookup_WorksEndToEnd()
    {
        var items = new Dictionary<string, List<string>>
        {
            ["march"] = new() { "Found Item" }
        };

        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "March" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("Found Item", cut.Markup);
        Assert.Empty(cut.FindAll(".hm-empty"));
    }

    [Fact]
    public void HeatmapRow_UpperCaseMonth_MatchesLowercaseKey()
    {
        var items = new Dictionary<string, List<string>>
        {
            ["april"] = new() { "Upper Case Match" }
        };

        var cut = RenderComponent<HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, new List<string> { "APRIL" })
            .Add(x => x.CurrentMonth, "APRIL"));

        Assert.Contains("Upper Case Match", cut.Markup);

        var cell = cut.Find(".hm-cell");
        Assert.Contains("apr", cell.GetAttribute("class") ?? "");
    }
}