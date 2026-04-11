using Bunit;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class HeatmapTests : TestContext
{
    private static HeatmapData CreateEmptyHeatmap() => new()
    {
        Shipped = new Dictionary<string, List<string>>(),
        InProgress = new Dictionary<string, List<string>>(),
        Carryover = new Dictionary<string, List<string>>(),
        Blockers = new Dictionary<string, List<string>>()
    };

    [Fact]
    public void Heatmap_RendersTitle()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, "January"));

        var title = cut.Find(".hm-title");
        Assert.Contains("MONTHLY EXECUTION HEATMAP", title.TextContent);
    }

    [Fact]
    public void Heatmap_RendersGridWithCorrectColumns()
    {
        var months = new List<string> { "January", "February", "March" };
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "January"));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("repeat(3, 1fr)", style);
    }

    [Fact]
    public void Heatmap_RendersMonthColumnHeaders()
    {
        var months = new List<string> { "January", "February" };
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.Equal(2, headers.Count);
        Assert.Contains("January", headers[0].TextContent);
        Assert.Contains("February", headers[1].TextContent);
    }

    [Fact]
    public void Heatmap_CurrentMonthHeader_HasAprHdrClass()
    {
        var months = new List<string> { "January", "February" };
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "February"));

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.DoesNotContain("apr-hdr", headers[0].GetAttribute("class") ?? "");
        Assert.Contains("apr-hdr", headers[1].GetAttribute("class") ?? "");
    }

    [Fact]
    public void Heatmap_CurrentMonthHeader_ShowsNowLabel()
    {
        var months = new List<string> { "April" };
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "April"));

        var header = cut.Find(".apr-hdr");
        Assert.Contains("Now", header.TextContent);
    }

    [Fact]
    public void Heatmap_RendersStatusCornerCell()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        var corner = cut.Find(".hm-corner");
        Assert.Equal("STATUS", corner.TextContent);
    }

    [Fact]
    public void Heatmap_RendersFourCategoryRows()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        Assert.Equal(4, rowHeaders.Count);
    }

    [Fact]
    public void Heatmap_CategoryRowsInCorrectOrder()
    {
        var cut = RenderComponent<Heatmap>(p => p
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
    public void Heatmap_CategoryRowsHaveCorrectCssPrefixes()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        Assert.NotNull(cut.Find(".ship-hdr"));
        Assert.NotNull(cut.Find(".prog-hdr"));
        Assert.NotNull(cut.Find(".carry-hdr"));
        Assert.NotNull(cut.Find(".block-hdr"));
    }

    [Fact]
    public void Heatmap_WithData_RendersItemsInCells()
    {
        var heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["january"] = new() { "Shipped Item 1" }
            },
            InProgress = new Dictionary<string, List<string>>(),
            Carryover = new Dictionary<string, List<string>>(),
            Blockers = new Dictionary<string, List<string>>()
        };

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, heatmap)
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("Shipped Item 1", cut.Markup);
    }

    [Fact]
    public void Heatmap_NonCurrentMonthHeader_NoNowLabel()
    {
        var months = new List<string> { "January", "February" };
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "February"));

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.DoesNotContain("Now", headers[0].TextContent);
    }
}