using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class HeatmapGridTests : TestContext
{
    private static DashboardData CreateDefaultData(
        List<string>? months = null,
        string currentDate = "2026-04-14",
        StatusRowsModel? statusRows = null)
    {
        return new DashboardData
        {
            Months = months ?? new List<string> { "Jan", "Feb", "Mar", "Apr" },
            CurrentDate = currentDate,
            StatusRows = statusRows ?? new StatusRowsModel
            {
                Shipped = new Dictionary<string, List<string>>
                {
                    ["Jan"] = new() { "Feature A", "Feature B" },
                    ["Feb"] = new() { "Feature C" },
                    ["Mar"] = new() { "Feature D" },
                    ["Apr"] = new()
                },
                InProgress = new Dictionary<string, List<string>>
                {
                    ["Apr"] = new() { "Task X", "Task Y" }
                },
                Carryover = new Dictionary<string, List<string>>
                {
                    ["Feb"] = new() { "Debt item 1" }
                },
                Blockers = new Dictionary<string, List<string>>
                {
                    ["Mar"] = new() { "Blocker 1" }
                }
            },
            Title = "Test Dashboard",
            Subtitle = "Test Subtitle",
            BacklogUrl = "#",
            Tracks = new List<TrackModel>()
        };
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Renders_GridWithCorrectDynamicColumnStyle()
    {
        var data = CreateDefaultData();

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapGrid>(p =>
            p.Add(x => x.Data, data));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style");
        style.Should().Contain("grid-template-columns: 160px repeat(4, 1fr)");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Renders_CurrentMonthHeaderHighlightedWithNowSuffix()
    {
        var data = CreateDefaultData(currentDate: "2026-04-14");

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapGrid>(p =>
            p.Add(x => x.Data, data));

        var aprHeader = cut.FindAll(".hm-col-hdr").FirstOrDefault(el =>
            el.ClassList.Contains("apr-hdr"));
        aprHeader.Should().NotBeNull("April header should have apr-hdr class");
        aprHeader!.TextContent.Should().Contain("Apr");
        aprHeader.TextContent.Should().Contain("Now");

        // Non-current month headers should NOT have apr-hdr
        var janHeader = cut.FindAll(".hm-col-hdr").First(el =>
            el.TextContent.Contains("Jan"));
        janHeader.ClassList.Should().NotContain("apr-hdr");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Renders_FourStatusRowsWithCorrectHeaderClasses()
    {
        var data = CreateDefaultData();

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapGrid>(p =>
            p.Add(x => x.Data, data));

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        rowHeaders.Should().HaveCount(4);

        rowHeaders[0].TextContent.Should().Contain("SHIPPED");
        rowHeaders[0].ClassList.Should().Contain("ship-hdr");

        rowHeaders[1].TextContent.Should().Contain("IN PROGRESS");
        rowHeaders[1].ClassList.Should().Contain("prog-hdr");

        rowHeaders[2].TextContent.Should().Contain("CARRYOVER");
        rowHeaders[2].ClassList.Should().Contain("carry-hdr");

        rowHeaders[3].TextContent.Should().Contain("BLOCKERS");
        rowHeaders[3].ClassList.Should().Contain("block-hdr");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Renders_DataItemsAndCurrentMonthCellHighlighting()
    {
        var data = CreateDefaultData(currentDate: "2026-04-14");

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapGrid>(p =>
            p.Add(x => x.Data, data));

        // Shipped row Jan cell should have 2 items
        var shipCells = cut.FindAll(".ship-cell");
        shipCells.Should().HaveCount(4, "one ship-cell per month");

        var janShipItems = shipCells[0].QuerySelectorAll(".it");
        janShipItems.Length.Should().Be(2);
        janShipItems[0].TextContent.Should().Be("Feature A");
        janShipItems[1].TextContent.Should().Be("Feature B");

        // InProgress Apr cell should have items and apr class
        var progCells = cut.FindAll(".prog-cell");
        var aprProgCell = progCells[3]; // Apr is 4th month (index 3)
        aprProgCell.ClassList.Should().Contain("apr", "current month data cells should have apr class");
        var aprProgItems = aprProgCell.QuerySelectorAll(".it");
        aprProgItems.Length.Should().Be(2);
        aprProgItems[0].TextContent.Should().Be("Task X");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Renders_DashesForEmptyCells_AndHandlesNullStatusRows()
    {
        // Test with null StatusRows - all cells should show dash
        var data = CreateDefaultData(
            months: new List<string> { "Jan", "Feb" },
            statusRows: null);
        // StatusRows is null, component should not crash
        data.StatusRows = null!;

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapGrid>(p =>
            p.Add(x => x.Data, data));

        var emptyCells = cut.FindAll(".empty-cell");
        // 4 rows x 2 months = 8 empty cells
        emptyCells.Should().HaveCount(8);
        // &ndash; renders as en-dash U+2013
        emptyCells.Should().AllSatisfy(e =>
            e.TextContent.Should().Be("\u2013"));

        // Also verify no crash, grid still renders
        cut.Find(".hm-corner").TextContent.Should().Contain("STATUS");
        cut.Find(".hm-title").TextContent.Should().Contain("MONTHLY EXECUTION HEATMAP");
    }
}