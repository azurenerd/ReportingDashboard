using Bunit;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Unit tests for Components/Heatmap.razor (root-level version from PR #521).
/// This version uses "apr-hdr" class for current month and delegates to root HeatmapRow/HeatmapCell.
/// </summary>
[Trait("Category", "Unit")]
public class RootHeatmapTests : TestContext
{
    private static HeatmapData CreateEmptyHeatmap() => new()
    {
        Shipped = new Dictionary<string, List<string>>(),
        InProgress = new Dictionary<string, List<string>>(),
        Carryover = new Dictionary<string, List<string>>(),
        Blockers = new Dictionary<string, List<string>>()
    };

    [Fact]
    public void Heatmap_RendersHmWrapDiv()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, "January"));

        Assert.NotNull(cut.Find(".hm-wrap"));
    }

    [Fact]
    public void Heatmap_RendersTitle_WithExpectedText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, "January"));

        var title = cut.Find(".hm-title");
        Assert.Contains("MONTHLY EXECUTION HEATMAP", title.TextContent);
        Assert.Contains("SHIPPED", title.TextContent);
        Assert.Contains("IN PROGRESS", title.TextContent);
        Assert.Contains("CARRYOVER", title.TextContent);
        Assert.Contains("BLOCKERS", title.TextContent);
    }

    [Fact]
    public void Heatmap_Grid_HasCorrectColumnTemplate()
    {
        var months = new List<string> { "January", "February", "March" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "January"));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("160px", style);
        Assert.Contains("repeat(3, 1fr)", style);
    }

    [Fact]
    public void Heatmap_Grid_HasCorrectRowTemplate()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("36px", style);
        Assert.Contains("repeat(4, 1fr)", style);
    }

    [Fact]
    public void Heatmap_RendersStatusCornerCell()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        var corner = cut.Find(".hm-corner");
        Assert.Equal("STATUS", corner.TextContent);
    }

    [Fact]
    public void Heatmap_RendersMonthHeaders()
    {
        var months = new List<string> { "January", "February", "March", "April" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.Equal(4, headers.Count);
        Assert.Contains("January", headers[0].TextContent);
        Assert.Contains("February", headers[1].TextContent);
        Assert.Contains("March", headers[2].TextContent);
        Assert.Contains("April", headers[3].TextContent);
    }

    [Fact]
    public void Heatmap_CurrentMonthHeader_HasAprHdrClass()
    {
        var months = new List<string> { "January", "April" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "April"));

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.DoesNotContain("apr-hdr", headers[0].GetAttribute("class") ?? "");
        Assert.Contains("apr-hdr", headers[1].GetAttribute("class") ?? "");
    }

    [Fact]
    public void Heatmap_CurrentMonthMatch_IsCaseInsensitive()
    {
        var months = new List<string> { "APRIL" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "april"));

        var header = cut.Find(".hm-col-hdr");
        Assert.Contains("apr-hdr", header.GetAttribute("class") ?? "");
    }

    [Fact]
    public void Heatmap_NonCurrentMonthHeaders_NoAprHdrClass()
    {
        var months = new List<string> { "January", "February" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "March"));

        var headers = cut.FindAll(".hm-col-hdr");
        foreach (var h in headers)
        {
            Assert.DoesNotContain("apr-hdr", h.GetAttribute("class") ?? "");
        }
    }

    [Fact]
    public void Heatmap_RendersFourCategoryRows()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        Assert.Equal(4, rowHeaders.Count);
    }

    [Fact]
    public void Heatmap_CategoryLabels_InCorrectOrder()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        Assert.Contains("SHIPPED", rowHeaders[0].TextContent);
        Assert.Contains("IN PROGRESS", rowHeaders[1].TextContent);
        Assert.Contains("CARRYOVER", rowHeaders[2].TextContent);
        Assert.Contains("BLOCKERS", rowHeaders[3].TextContent);
    }

    [Fact]
    public void Heatmap_CategoryRows_HaveCorrectCssPrefixes()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        Assert.NotNull(cut.Find(".ship-hdr"));
        Assert.NotNull(cut.Find(".prog-hdr"));
        Assert.NotNull(cut.Find(".carry-hdr"));
        Assert.NotNull(cut.Find(".block-hdr"));
    }

    [Fact]
    public void Heatmap_WithShippedData_RendersItems()
    {
        var heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["january"] = new() { "Feature X", "Feature Y" }
            },
            InProgress = new(),
            Carryover = new(),
            Blockers = new()
        };

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, heatmap)
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("Feature X", cut.Markup);
        Assert.Contains("Feature Y", cut.Markup);
    }

    [Fact]
    public void Heatmap_SingleMonth_GridHasRepeat1()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "April" })
            .Add(x => x.CurrentMonth, ""));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("repeat(1, 1fr)", style);
    }

    [Fact]
    public void Heatmap_SixMonths_GridHasRepeat6()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("repeat(6, 1fr)", style);
    }

    [Fact]
    public void Heatmap_DefaultParameters_DoNotThrow()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>();

        Assert.NotNull(cut.Find(".hm-wrap"));
    }
}