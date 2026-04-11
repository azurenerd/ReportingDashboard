using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for the root Components/Heatmap.razor, Components/HeatmapRow.razor,
/// and Components/HeatmapCell.razor rendered with real DashboardDataService data loaded from disk.
/// Tests the full heatmap rendering pipeline: service → data → Heatmap → HeatmapRow → HeatmapCell.
/// </summary>
[Trait("Category", "Integration")]
public class HeatmapComponentIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public HeatmapComponentIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HmComp_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public new void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            base.Dispose();
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
    }

    private DashboardDataService CreateServiceFromJson(string json)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        return svc;
    }

    private DashboardDataService CreateServiceWithHeatmapData()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Heatmap Integration Test",
            subtitle = "Team X",
            backlogLink = "https://link",
            currentMonth = "March",
            months = new[] { "January", "February", "March", "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-03-15",
                tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>
                {
                    ["january"] = new[] { "Auth Module", "CI Pipeline" },
                    ["february"] = new[] { "Search Feature" },
                    ["march"] = new[] { "Dashboard v1", "Export Tool" }
                },
                inProgress = new Dictionary<string, string[]>
                {
                    ["march"] = new[] { "Analytics Engine" },
                    ["april"] = new[] { "Mobile App", "API Gateway" }
                },
                carryover = new Dictionary<string, string[]>
                {
                    ["february"] = new[] { "Legacy Migration" }
                },
                blockers = new Dictionary<string, string[]>
                {
                    ["march"] = new[] { "Vendor SDK Delay", "Security Audit Pending" }
                }
            }
        }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        return CreateServiceFromJson(json);
    }

    #region Heatmap Structure From Service Data

    [Fact]
    public void Heatmap_RendersWrappperWithServiceData()
    {
        var svc = CreateServiceWithHeatmapData();
        var data = svc.Data!;

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, data.Heatmap)
            .Add(x => x.Months, data.Months)
            .Add(x => x.CurrentMonth, data.CurrentMonth));

        Assert.NotNull(cut.Find(".hm-wrap"));
        Assert.NotNull(cut.Find(".hm-grid"));
        Assert.NotNull(cut.Find(".hm-title"));
    }

    [Fact]
    public void Heatmap_RendersAllMonthHeaders()
    {
        var svc = CreateServiceWithHeatmapData();
        var data = svc.Data!;

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, data.Heatmap)
            .Add(x => x.Months, data.Months)
            .Add(x => x.CurrentMonth, data.CurrentMonth));

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.Equal(4, headers.Count);
        Assert.Contains("January", headers[0].TextContent);
        Assert.Contains("February", headers[1].TextContent);
        Assert.Contains("March", headers[2].TextContent);
        Assert.Contains("April", headers[3].TextContent);
    }

    [Fact]
    public void Heatmap_CurrentMonthHighlighted()
    {
        var svc = CreateServiceWithHeatmapData();
        var data = svc.Data!;

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, data.Heatmap)
            .Add(x => x.Months, data.Months)
            .Add(x => x.CurrentMonth, data.CurrentMonth));

        // March is currentMonth, should have apr-hdr class
        var headers = cut.FindAll(".hm-col-hdr");
        Assert.DoesNotContain("apr-hdr", headers[0].GetAttribute("class") ?? "");
        Assert.DoesNotContain("apr-hdr", headers[1].GetAttribute("class") ?? "");
        Assert.Contains("apr-hdr", headers[2].GetAttribute("class") ?? "");
        Assert.DoesNotContain("apr-hdr", headers[3].GetAttribute("class") ?? "");
    }

    [Fact]
    public void Heatmap_GridTemplate_MatchesMonthCount()
    {
        var svc = CreateServiceWithHeatmapData();
        var data = svc.Data!;

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, data.Heatmap)
            .Add(x => x.Months, data.Months)
            .Add(x => x.CurrentMonth, data.CurrentMonth));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("repeat(4, 1fr)", style);
    }

    #endregion

    #region Category Row Rendering From Service Data

    [Fact]
    public void Heatmap_ShippedRow_RendersItemsFromService()
    {
        var svc = CreateServiceWithHeatmapData();
        var data = svc.Data!;

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, data.Heatmap)
            .Add(x => x.Months, data.Months)
            .Add(x => x.CurrentMonth, data.CurrentMonth));

        Assert.Contains("Auth Module", cut.Markup);
        Assert.Contains("CI Pipeline", cut.Markup);
        Assert.Contains("Search Feature", cut.Markup);
        Assert.Contains("Dashboard v1", cut.Markup);
        Assert.Contains("Export Tool", cut.Markup);
    }

    [Fact]
    public void Heatmap_InProgressRow_RendersItemsFromService()
    {
        var svc = CreateServiceWithHeatmapData();
        var data = svc.Data!;

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, data.Heatmap)
            .Add(x => x.Months, data.Months)
            .Add(x => x.CurrentMonth, data.CurrentMonth));

        Assert.Contains("Analytics Engine", cut.Markup);
        Assert.Contains("Mobile App", cut.Markup);
        Assert.Contains("API Gateway", cut.Markup);
    }

    [Fact]
    public void Heatmap_CarryoverRow_RendersItemsFromService()
    {
        var svc = CreateServiceWithHeatmapData();
        var data = svc.Data!;

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, data.Heatmap)
            .Add(x => x.Months, data.Months)
            .Add(x => x.CurrentMonth, data.CurrentMonth));

        Assert.Contains("Legacy Migration", cut.Markup);
    }

    [Fact]
    public void Heatmap_BlockersRow_RendersItemsFromService()
    {
        var svc = CreateServiceWithHeatmapData();
        var data = svc.Data!;

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, data.Heatmap)
            .Add(x => x.Months, data.Months)
            .Add(x => x.CurrentMonth, data.CurrentMonth));

        Assert.Contains("Vendor SDK Delay", cut.Markup);
        Assert.Contains("Security Audit Pending", cut.Markup);
    }

    [Fact]
    public void Heatmap_RendersFourCategoryRows()
    {
        var svc = CreateServiceWithHeatmapData();
        var data = svc.Data!;

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, data.Heatmap)
            .Add(x => x.Months, data.Months)
            .Add(x => x.CurrentMonth, data.CurrentMonth));

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        Assert.Equal(4, rowHeaders.Count);
    }

    [Fact]
    public void Heatmap_CategoryRowsHaveCorrectCssPrefixes()
    {
        var svc = CreateServiceWithHeatmapData();
        var data = svc.Data!;

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, data.Heatmap)
            .Add(x => x.Months, data.Months)
            .Add(x => x.CurrentMonth, data.CurrentMonth));

        Assert.NotNull(cut.Find(".ship-hdr"));
        Assert.NotNull(cut.Find(".prog-hdr"));
        Assert.NotNull(cut.Find(".carry-hdr"));
        Assert.NotNull(cut.Find(".block-hdr"));
    }

    #endregion

    #region Empty Cells and Dashes

    [Fact]
    public void Heatmap_MonthsWithNoItems_ShowDash()
    {
        var svc = CreateServiceWithHeatmapData();
        var data = svc.Data!;

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, data.Heatmap)
            .Add(x => x.Months, data.Months)
            .Add(x => x.CurrentMonth, data.CurrentMonth));

        // Carryover only has data for "february", other months should show dash
        Assert.Contains("hm-empty", cut.Markup);
    }

    #endregion

    #region Current Month Cell Highlighting

    [Fact]
    public void Heatmap_CurrentMonthCells_HaveAprClass()
    {
        var svc = CreateServiceWithHeatmapData();
        var data = svc.Data!;

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapData, data.Heatmap)
            .Add(x => x.Months, data.Months)
            .Add(x => x.CurrentMonth, data.CurrentMonth));

        // There should be cells with "apr" class for March (currentMonth)
        var allCells = cut.FindAll(".hm-cell");
        var aprCells = allCells.Where(c => (c.GetAttribute("class") ?? "").Contains(" apr")).ToList();
        // 4 categories × 1 current month = 4 cells with apr class
        Assert.Equal(4, aprCells.Count);
    }

    #endregion
}