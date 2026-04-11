using Bunit;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests that verify the full Heatmap component tree renders correctly
/// when composed together (Heatmap → HeatmapRow → HeatmapCell) with realistic data.
/// </summary>
[Trait("Category", "Integration")]
public class HeatmapComponentIntegrationTests : TestContext
{
    private static HeatmapData CreateRealisticHeatmap() => new()
    {
        Shipped = new Dictionary<string, List<string>>
        {
            ["jan"] = new() { "Auth module", "API gateway" },
            ["feb"] = new() { "Dashboard v1" },
            ["mar"] = new() { "Report export", "Batch processing" },
            ["apr"] = new() { "Mobile app" }
        },
        InProgress = new Dictionary<string, List<string>>
        {
            ["apr"] = new() { "Search indexing", "Cache layer" }
        },
        Carryover = new Dictionary<string, List<string>>
        {
            ["mar"] = new() { "Legacy migration" }
        },
        Blockers = new Dictionary<string, List<string>>
        {
            ["apr"] = new() { "License renewal" }
        }
    };

    [Fact]
    public void FullHeatmap_WithRealisticData_RendersCorrectStructure()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr" };
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateRealisticHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr"));

        // Structure: 1 corner + 4 column headers
        Assert.NotNull(cut.Find(".hm-corner"));
        Assert.Equal(4, cut.FindAll(".hm-col-hdr").Count);

        // 4 row headers
        Assert.Equal(4, cut.FindAll(".hm-row-hdr").Count);

        // 4 rows × 4 months = 16 data cells
        Assert.Equal(16, cut.FindAll(".hm-cell").Count);
    }

    [Fact]
    public void FullHeatmap_ShippedItemsAppearInCorrectCells()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateRealisticHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr" })
            .Add(x => x.CurrentMonth, "Apr"));

        // Shipped Jan items
        Assert.Contains("Auth module", cut.Markup);
        Assert.Contains("API gateway", cut.Markup);
        // Shipped Feb
        Assert.Contains("Dashboard v1", cut.Markup);
        // Shipped Mar
        Assert.Contains("Report export", cut.Markup);
        Assert.Contains("Batch processing", cut.Markup);
        // Shipped Apr
        Assert.Contains("Mobile app", cut.Markup);
    }

    [Fact]
    public void FullHeatmap_InProgressItemsRendered()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateRealisticHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr" })
            .Add(x => x.CurrentMonth, "Apr"));

        Assert.Contains("Search indexing", cut.Markup);
        Assert.Contains("Cache layer", cut.Markup);
    }

    [Fact]
    public void FullHeatmap_BlockerItemsRendered()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateRealisticHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr" })
            .Add(x => x.CurrentMonth, "Apr"));

        Assert.Contains("License renewal", cut.Markup);
    }

    [Fact]
    public void FullHeatmap_CurrentMonthApr_HighlightApplied()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateRealisticHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr" })
            .Add(x => x.CurrentMonth, "Apr"));

        // Column header for Apr should have apr-hdr
        var aprHeader = cut.Find(".apr-hdr");
        Assert.Contains("Apr", aprHeader.TextContent);

        // Each row should have exactly one cell with apr class (the Apr column)
        var aprCells = cut.FindAll(".apr");
        Assert.Equal(4, aprCells.Count);
    }

    [Fact]
    public void FullHeatmap_EmptyCells_ShowDashes()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateRealisticHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr" })
            .Add(x => x.CurrentMonth, "Apr"));

        // InProgress has data only for Apr, so Jan/Feb/Mar should be dashes
        // Carryover has data only for Mar, so Jan/Feb/Apr should be dashes
        // Blockers has data only for Apr, so Jan/Feb/Mar should be dashes
        var dashes = cut.FindAll(".hm-empty");
        Assert.True(dashes.Count > 0, "Expected some empty cell dashes");
    }

    [Fact]
    public void FullHeatmap_AllCssPrefixCellsPresent()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateRealisticHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        Assert.NotNull(cut.Find(".ship-cell"));
        Assert.NotNull(cut.Find(".prog-cell"));
        Assert.NotNull(cut.Find(".carry-cell"));
        Assert.NotNull(cut.Find(".block-cell"));
    }

    [Fact]
    public void FullHeatmap_ItemsHaveItClass()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateRealisticHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr" })
            .Add(x => x.CurrentMonth, "Apr"));

        var itDivs = cut.FindAll(".it");
        // Total items: Shipped(2+1+2+1) + InProgress(2) + Carryover(1) + Blockers(1) = 10
        Assert.Equal(10, itDivs.Count);
    }

    [Fact]
    public void FullHeatmap_NonCurrentMonthCells_NoAprClass()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateRealisticHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr" })
            .Add(x => x.CurrentMonth, "Apr"));

        // Get all cells that are NOT current month (12 of 16)
        var allCells = cut.FindAll(".hm-cell");
        var nonAprCells = allCells.Where(c =>
            !(c.GetAttribute("class") ?? "").Contains(" apr")).ToList();
        Assert.Equal(12, nonAprCells.Count);
    }

    [Fact]
    public void FullHeatmap_WithNoCurrentMonth_NoHighlightAnywhere()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateRealisticHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr" })
            .Add(x => x.CurrentMonth, ""));

        var aprHeaders = cut.FindAll(".apr-hdr");
        Assert.Empty(aprHeaders);

        var aprCells = cut.FindAll(".apr");
        Assert.Empty(aprCells);
    }
}