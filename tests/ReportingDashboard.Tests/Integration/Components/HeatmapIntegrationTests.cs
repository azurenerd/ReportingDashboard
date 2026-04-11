using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class HeatmapIntegrationTests : TestContext
{
    [Fact]
    public void Heatmap_WithFullData_ShouldRenderRowsAndCellsTogether()
    {
        var heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["Jan"] = new() { "Feature A", "Feature B" },
                ["Feb"] = new() { "Feature C" }
            },
            InProgress = new Dictionary<string, List<string>>
            {
                ["Feb"] = new() { "Task X" },
                ["Mar"] = new() { "Task Y", "Task Z" }
            },
            Carryover = new Dictionary<string, List<string>>
            {
                ["Mar"] = new() { "Old Item" }
            },
            Blockers = new Dictionary<string, List<string>>
            {
                ["Mar"] = new() { "Blocker A" }
            }
        };
        var months = new List<string> { "Jan", "Feb", "Mar" };

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapModel, heatmap)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Mar"));

        // 4 row headers (shipped, prog, carry, block)
        cut.FindAll(".hm-row-hdr").Should().HaveCount(4);

        // 12 cells total (4 rows × 3 months)
        cut.FindAll(".hm-cell").Should().HaveCount(12);

        // Verify items render in correct cells
        cut.Markup.Should().Contain("Feature A");
        cut.Markup.Should().Contain("Feature B");
        cut.Markup.Should().Contain("Task Y");
        cut.Markup.Should().Contain("Blocker A");

        // Current month cells highlighted
        cut.FindAll(".shipped-cur").Should().NotBeEmpty();
        cut.FindAll(".prog-cur").Should().NotBeEmpty();
        cut.FindAll(".carry-cur").Should().NotBeEmpty();
        cut.FindAll(".block-cur").Should().NotBeEmpty();
    }

    [Fact]
    public void Heatmap_EmptyCategoriesWithMonths_ShouldShowAllDashes()
    {
        var heatmap = new HeatmapData();
        var months = new List<string> { "Jan", "Feb" };

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapModel, heatmap)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Jan"));

        // 8 cells (4 rows × 2 months), all should show dash
        var emptyCells = cut.FindAll(".empty-cell");
        emptyCells.Should().HaveCount(8);
    }

    [Fact]
    public void Heatmap_CurrentMonthNotInList_ShouldNotHighlightAny()
    {
        var heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["Jan"] = new() { "Item" }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapModel, heatmap)
            .Add(x => x.Months, new List<string> { "Jan", "Feb" })
            .Add(x => x.CurrentMonth, "Dec"));

        cut.FindAll(".cur-month-hdr").Should().BeEmpty();
        cut.Markup.Should().NotContain("shipped-cur");
    }

    [Fact]
    public void HeatmapRow_PassesCorrectItemsToCells()
    {
        var items = new Dictionary<string, List<string>>
        {
            ["Jan"] = new() { "A", "B" },
            ["Mar"] = new() { "C" }
        };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.Category, "shipped")
            .Add(x => x.Label, "Shipped")
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar" })
            .Add(x => x.CurrentMonth, "Mar")
            .Add(x => x.Items, items));

        var cells = cut.FindAll(".hm-cell");
        cells.Should().HaveCount(3);

        // Jan has 2 items
        cells[0].QuerySelectorAll(".it").Length.Should().Be(2);
        // Feb has no items - shows dash
        cells[1].QuerySelector(".empty-cell").Should().NotBeNull();
        // Mar has 1 item and is current
        cells[2].QuerySelectorAll(".it").Length.Should().Be(1);
        cells[2].ClassList.Should().Contain("shipped-cur");
    }

    [Fact]
    public void HeatmapCell_ItemsWithSpecialCharacters_ShouldRenderSafely()
    {
        var items = new List<string>
        {
            "Feature with <tags>",
            "Item & more",
            "Quotes \"test\"",
            "Unicode: 日本語"
        };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p => p
            .Add(x => x.Category, "shipped")
            .Add(x => x.Items, items)
            .Add(x => x.IsCurrent, false));

        cut.FindAll(".it").Should().HaveCount(4);
        // bUnit HTML-encodes, so no raw tags
        cut.Markup.Should().NotContain("<tags>");
        cut.Markup.Should().Contain("Unicode:");
    }
}