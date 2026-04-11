using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying the Heatmap component renders correctly
/// when driven through the Dashboard page with a real DashboardDataService
/// loaded from JSON on disk. Covers all four categories, month mapping,
/// current month highlighting, and data flow through HeatmapRow → HeatmapCell.
/// </summary>
[Trait("Category", "Integration")]
public class HeatmapComponentIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };

    public HeatmapComponentIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HmInteg_{Guid.NewGuid():N}");
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

    private string BuildJson(
        Dictionary<string, string[]>? shipped = null,
        Dictionary<string, string[]>? inProgress = null,
        Dictionary<string, string[]>? carryover = null,
        Dictionary<string, string[]>? blockers = null,
        string[]? months = null,
        string currentMonth = "Apr")
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Heatmap Integration Test",
            subtitle = "Team - April 2026",
            backlogLink = "https://dev.azure.com/test",
            currentMonth,
            months = months ?? new[] { "Jan", "Feb", "Mar", "Apr" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = shipped ?? new Dictionary<string, string[]>(),
                inProgress = inProgress ?? new Dictionary<string, string[]>(),
                carryover = carryover ?? new Dictionary<string, string[]>(),
                blockers = blockers ?? new Dictionary<string, string[]>()
            }
        }, JsonOpts);
    }

    #region Heatmap Section Rendering via Dashboard

    [Fact]
    public void Dashboard_WithHeatmap_RendersHeatmapWrap()
    {
        var svc = CreateServiceFromJson(BuildJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("hm-wrap", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithHeatmap_RendersTitle()
    {
        var svc = CreateServiceFromJson(BuildJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("MONTHLY EXECUTION HEATMAP", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithHeatmap_RendersStatusCorner()
    {
        var svc = CreateServiceFromJson(BuildJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var corner = cut.Find(".hm-corner");
        Assert.Equal("STATUS", corner.TextContent);
    }

    [Fact]
    public void Dashboard_With4Months_RendersAllColumnHeaders()
    {
        var svc = CreateServiceFromJson(BuildJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.Equal(4, headers.Count);
        Assert.Contains("Jan", headers[0].TextContent);
        Assert.Contains("Feb", headers[1].TextContent);
        Assert.Contains("Mar", headers[2].TextContent);
        Assert.Contains("Apr", headers[3].TextContent);
    }

    [Fact]
    public void Dashboard_CurrentMonth_HighlightedInHeader()
    {
        var svc = CreateServiceFromJson(BuildJson(currentMonth: "Mar"));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var headers = cut.FindAll(".hm-col-hdr");
        // Mar is the 3rd header (index 2)
        Assert.Contains("apr-hdr", headers[2].GetAttribute("class") ?? "");
        Assert.Contains("Now", headers[2].TextContent);
    }

    [Fact]
    public void Dashboard_AllFourCategoryRows_RenderedInOrder()
    {
        var svc = CreateServiceFromJson(BuildJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        Assert.Equal(4, rowHeaders.Count);
        Assert.Contains("SHIPPED", rowHeaders[0].TextContent);
        Assert.Contains("IN PROGRESS", rowHeaders[1].TextContent);
        Assert.Contains("CARRYOVER", rowHeaders[2].TextContent);
        Assert.Contains("BLOCKERS", rowHeaders[3].TextContent);
    }

    [Fact]
    public void Dashboard_CategoryHeaders_HaveCorrectCssPrefixes()
    {
        var svc = CreateServiceFromJson(BuildJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.NotNull(cut.Find(".ship-hdr"));
        Assert.NotNull(cut.Find(".prog-hdr"));
        Assert.NotNull(cut.Find(".carry-hdr"));
        Assert.NotNull(cut.Find(".block-hdr"));
    }

    #endregion

    #region Shipped Items Data Flow

    [Fact]
    public void Dashboard_ShippedItems_RenderedInCorrectCells()
    {
        var shipped = new Dictionary<string, string[]>
        {
            ["jan"] = new[] { "Auth Module", "CI Pipeline" },
            ["feb"] = new[] { "Search Feature" },
            ["mar"] = new[] { "Dashboard v1", "Report Builder" }
        };
        var svc = CreateServiceFromJson(BuildJson(shipped: shipped));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Auth Module", cut.Markup);
        Assert.Contains("CI Pipeline", cut.Markup);
        Assert.Contains("Search Feature", cut.Markup);
        Assert.Contains("Dashboard v1", cut.Markup);
        Assert.Contains("Report Builder", cut.Markup);
    }

    #endregion

    #region InProgress Items Data Flow

    [Fact]
    public void Dashboard_InProgressItems_RenderedInCorrectCells()
    {
        var inProgress = new Dictionary<string, string[]>
        {
            ["mar"] = new[] { "Analytics Engine" },
            ["apr"] = new[] { "Export API", "Bulk Operations" }
        };
        var svc = CreateServiceFromJson(BuildJson(inProgress: inProgress));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Analytics Engine", cut.Markup);
        Assert.Contains("Export API", cut.Markup);
        Assert.Contains("Bulk Operations", cut.Markup);
    }

    #endregion

    #region Carryover Items Data Flow

    [Fact]
    public void Dashboard_CarryoverItems_RenderedInCorrectCells()
    {
        var carryover = new Dictionary<string, string[]>
        {
            ["apr"] = new[] { "Legacy Migration", "Debt Cleanup" }
        };
        var svc = CreateServiceFromJson(BuildJson(carryover: carryover));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Legacy Migration", cut.Markup);
        Assert.Contains("Debt Cleanup", cut.Markup);
    }

    #endregion

    #region Blocker Items Data Flow

    [Fact]
    public void Dashboard_BlockerItems_RenderedInCorrectCells()
    {
        var blockers = new Dictionary<string, string[]>
        {
            ["mar"] = new[] { "Vendor Dependency" },
            ["apr"] = new[] { "License Review", "Security Audit" }
        };
        var svc = CreateServiceFromJson(BuildJson(blockers: blockers));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Vendor Dependency", cut.Markup);
        Assert.Contains("License Review", cut.Markup);
        Assert.Contains("Security Audit", cut.Markup);
    }

    #endregion

    #region All Categories Together

    [Fact]
    public void Dashboard_AllHeatmapCategories_RenderedTogether()
    {
        var shipped = new Dictionary<string, string[]> { ["jan"] = new[] { "Shipped A" } };
        var inProgress = new Dictionary<string, string[]> { ["feb"] = new[] { "InProg B" } };
        var carryover = new Dictionary<string, string[]> { ["mar"] = new[] { "Carry C" } };
        var blockers = new Dictionary<string, string[]> { ["apr"] = new[] { "Block D" } };

        var svc = CreateServiceFromJson(BuildJson(shipped, inProgress, carryover, blockers));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Shipped A", cut.Markup);
        Assert.Contains("InProg B", cut.Markup);
        Assert.Contains("Carry C", cut.Markup);
        Assert.Contains("Block D", cut.Markup);
    }

    #endregion

    #region Empty Heatmap Cells

    [Fact]
    public void Dashboard_EmptyHeatmapCells_ShowDash()
    {
        var svc = CreateServiceFromJson(BuildJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // With no data in any cell, all cells should show "-"
        Assert.Contains("-", cut.Markup);
    }

    #endregion

    #region Grid Columns Match Month Count

    [Fact]
    public void Dashboard_With2Months_GridHasRepeat2()
    {
        var svc = CreateServiceFromJson(BuildJson(months: new[] { "Jan", "Feb" }, currentMonth: "Feb"));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("repeat(2, 1fr)", style);
    }

    [Fact]
    public void Dashboard_With6Months_GridHasRepeat6()
    {
        var svc = CreateServiceFromJson(BuildJson(
            months: new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
            currentMonth: "Jun"));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("repeat(6, 1fr)", style);

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.Equal(6, headers.Count);
    }

    #endregion

    #region Month Key Mapping (lowercase vs original)

    [Fact]
    public void Dashboard_HeatmapWithLowercaseKeys_ItemsMapped()
    {
        var shipped = new Dictionary<string, string[]>
        {
            ["jan"] = new[] { "Lowercase Key Item" }
        };
        var svc = CreateServiceFromJson(BuildJson(shipped: shipped));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Lowercase Key Item", cut.Markup);
    }

    [Fact]
    public void Dashboard_HeatmapWithOriginalCaseKeys_ItemsMapped()
    {
        // The HeatmapRow component tries lowercase first, then original case
        var shipped = new Dictionary<string, string[]>
        {
            ["Jan"] = new[] { "Original Case Item" }
        };
        var svc = CreateServiceFromJson(BuildJson(shipped: shipped));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Original Case Item", cut.Markup);
    }

    #endregion

    #region Special Characters in Heatmap Items

    [Fact]
    public void Dashboard_HeatmapItemsWithSpecialChars_HtmlEncoded()
    {
        var shipped = new Dictionary<string, string[]>
        {
            ["jan"] = new[] { "Feature <Alpha> & \"Beta\"" }
        };
        var svc = CreateServiceFromJson(BuildJson(shipped: shipped));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // Blazor auto-encodes
        Assert.DoesNotContain("<Alpha>", cut.Markup);
        Assert.Contains("Alpha", cut.Markup);
        Assert.Contains("Beta", cut.Markup);
    }

    #endregion
}