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
    public void Renders_GridWithCorrectColumnCount()
    {
        var data = CreateDefaultData();

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapGrid>(p =>
            p.Add(x => x.Data, data));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style");
        style.Should().Contain("grid-template-columns: 160px repeat(4, 1fr)");
        style.Should().Contain("grid-template-rows: 36px repeat(4, 1fr)");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Renders_CurrentMonthHighlighted()
    {
        var data = CreateDefaultData(currentDate: "2026-04-14");

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapGrid>(p =>
            p.Add(x => x.Data, data));

        var aprHeader = cut.FindAll(".hm-col-hdr").FirstOrDefault(el =>
            el.ClassList.Contains("apr-hdr"));
        aprHeader.Should().NotBeNull();
        aprHeader!.TextContent.Should().Contain("Apr");
        aprHeader.TextContent.Should().Contain("Now");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Renders_EmptyCellsWithDash()
    {
        var data = CreateDefaultData(
            months: new List<string> { "Jan", "Feb", "Mar", "Apr" },
            statusRows: new StatusRowsModel
            {
                Shipped = new Dictionary<string, List<string>>(),
                InProgress = new Dictionary<string, List<string>>(),
                Carryover = new Dictionary<string, List<string>>(),
                Blockers = new Dictionary<string, List<string>>()
            });

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapGrid>(p =>
            p.Add(x => x.Data, data));

        var emptyCells = cut.FindAll(".empty-cell");
        emptyCells.Should().HaveCountGreaterThan(0);
        emptyCells.All(e => e.TextContent == "-").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Renders_FourStatusRows()
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
    public void Renders_NoHighlight_WhenCurrentDateInvalid()
    {
        var data = CreateDefaultData(currentDate: "not-a-date");

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapGrid>(p =>
            p.Add(x => x.Data, data));

        var highlightedHeaders = cut.FindAll(".apr-hdr");
        highlightedHeaders.Should().BeEmpty();

        var highlightedCells = cut.FindAll(".apr");
        highlightedCells.Should().BeEmpty();
    }
}