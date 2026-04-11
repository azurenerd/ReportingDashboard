using Bunit;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for Components/Heatmap.razor (root-level inline-styled version).
/// Uses inline grid styles and 'apr-hdr' class for current month (differs from Sections version).
/// </summary>
[Trait("Category", "Unit")]
public class InlineHeatmapTests : TestContext
{
    private static HeatmapData CreateEmptyHeatmap() => new()
    {
        Shipped = new Dictionary<string, List<string>>(),
        InProgress = new Dictionary<string, List<string>>(),
        Carryover = new Dictionary<string, List<string>>(),
        Blockers = new Dictionary<string, List<string>>()
    };

    #region Title

    [Fact]
    public void Heatmap_RendersTitle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Jan"));

        var title = cut.Find(".hm-title");
        Assert.Contains("MONTHLY EXECUTION HEATMAP", title.TextContent);
    }

    #endregion

    #region Grid Structure

    [Fact]
    public void Heatmap_RendersGrid()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb" })
            .Add(x => x.CurrentMonth, "Jan"));

        var grid = cut.Find(".hm-grid");
        Assert.NotNull(grid);
    }

    [Fact]
    public void Heatmap_GridColumnsMatchMonthCount()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Jan"));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("160px repeat(4, 1fr)", style);
    }

    [Fact]
    public void Heatmap_GridRowsIs36pxHeaderPlusFourDataRows()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Jan"));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("36px repeat(4, 1fr)", style);
    }

    [Fact]
    public void Heatmap_RendersCornerCell_WithStatusText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Jan"));

        var corner = cut.Find(".hm-corner");
        Assert.Equal("STATUS", corner.TextContent);
    }

    #endregion

    #region Month Column Headers

    [Fact]
    public void Heatmap_RendersColumnHeadersForEachMonth()
    {
        var months = new List<string> { "Jan", "Feb", "Mar" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.Equal(3, headers.Count);
        Assert.Contains("Jan", headers[0].TextContent);
        Assert.Contains("Feb", headers[1].TextContent);
        Assert.Contains("Mar", headers[2].TextContent);
    }

    [Fact]
    public void Heatmap_CurrentMonthHeader_HasAprHdrClass()
    {
        var months = new List<string> { "Jan", "Feb" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Feb"));

        var headers = cut.FindAll(".hm-col-hdr");
        var febHeader = headers[1];
        Assert.Contains("apr-hdr", febHeader.GetAttribute("class") ?? "");
    }

    [Fact]
    public void Heatmap_NonCurrentMonthHeader_NoAprHdrClass()
    {
        var months = new List<string> { "Jan", "Feb" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Feb"));

        var headers = cut.FindAll(".hm-col-hdr");
        var janHeader = headers[0];
        Assert.DoesNotContain("apr-hdr", janHeader.GetAttribute("class") ?? "");
    }

    [Fact]
    public void Heatmap_CurrentMonthHeader_ShowsNowLabel()
    {
        var months = new List<string> { "Apr" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr"));

        var header = cut.Find(".apr-hdr");
        Assert.Contains("Now", header.TextContent);
    }

    [Fact]
    public void Heatmap_CurrentMonthComparison_IsCaseInsensitive()
    {
        var months = new List<string> { "apr" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "APR"));

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.Contains("apr-hdr", headers[0].GetAttribute("class") ?? "");
    }

    #endregion

    #region Category Rows

    [Fact]
    public void Heatmap_RendersFourCategoryRows()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        Assert.Equal(4, rowHeaders.Count);
    }

    [Fact]
    public void Heatmap_CategoryLabels_InCorrectOrder()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        var headers = cut.FindAll(".hm-row-hdr");
        Assert.Contains("SHIPPED", headers[0].TextContent);
        Assert.Contains("IN PROGRESS", headers[1].TextContent);
        Assert.Contains("CARRYOVER", headers[2].TextContent);
        Assert.Contains("BLOCKERS", headers[3].TextContent);
    }

    [Fact]
    public void Heatmap_CategoryHeaders_HaveCorrectCssPrefixes()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        Assert.NotNull(cut.Find(".ship-hdr"));
        Assert.NotNull(cut.Find(".prog-hdr"));
        Assert.NotNull(cut.Find(".carry-hdr"));
        Assert.NotNull(cut.Find(".block-hdr"));
    }

    #endregion

    #region Data Rendering

    [Fact]
    public void Heatmap_WithShippedItems_RendersInCells()
    {
        var heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["jan"] = new() { "Auth Module", "CI Pipeline" }
            },
            InProgress = new(),
            Carryover = new(),
            Blockers = new()
        };

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, heatmap)
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("Auth Module", cut.Markup);
        Assert.Contains("CI Pipeline", cut.Markup);
    }

    [Fact]
    public void Heatmap_SingleMonth_GridColumnsHasRepeat1()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Jan"));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("repeat(1, 1fr)", style);
    }

    #endregion
}