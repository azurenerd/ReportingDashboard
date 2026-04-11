using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class DashboardComponentIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public DashboardComponentIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashCompInteg_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    private DashboardDataService CreateLoadedService()
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, TestDataHelper.CreateValidDataJsonString());
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        return svc;
    }

    private DashboardDataService CreateErrorService()
    {
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json")).GetAwaiter().GetResult();
        return svc;
    }

    #region Dashboard Page ↔ Child Component Integration

    [Fact]
    public void DashboardPage_WithValidData_RendersAllSections()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // Dashboard.razor wraps sections in divs with these classes
        Assert.Contains("hdr", cut.Markup);
        Assert.Contains("tl-area", cut.Markup);
        Assert.Contains("hm-wrap", cut.Markup);
    }

    [Fact]
    public void DashboardPage_WithValidData_HeaderShowsCorrectTitle()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var h1 = cut.Find("h1");

        Assert.Contains("Integration Test Dashboard", h1.TextContent);
    }

    [Fact]
    public void DashboardPage_WithValidData_HeaderShowsBacklogLink()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var link = cut.Find("a[href='https://dev.azure.com/test/backlog']");

        Assert.NotNull(link);
        Assert.Contains("ADO Backlog", link.TextContent);
    }

    [Fact]
    public void DashboardPage_WithValidData_TimelineShowsTrackNames()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("M1", cut.Markup);
        Assert.Contains("Core Platform", cut.Markup);
        Assert.Contains("M2", cut.Markup);
        Assert.Contains("Data Pipeline", cut.Markup);
    }

    [Fact]
    public void DashboardPage_WithValidData_TimelineSvgContainsMilestones()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Feb 15 PoC", cut.Markup);
        Assert.Contains("May 1 GA", cut.Markup);
        Assert.Contains("Mar 1 Check", cut.Markup);
    }

    [Fact]
    public void DashboardPage_WithValidData_TimelineSvgContainsNowMarker()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("NOW", cut.Markup);
        Assert.Contains("#EA4335", cut.Markup);
    }

    [Fact]
    public void DashboardPage_WithValidData_HeatmapShowsMonthHeaders()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var colHeaders = cut.FindAll(".hm-col-hdr");

        Assert.Equal(4, colHeaders.Count);
        Assert.Contains("January", colHeaders[0].TextContent);
        Assert.Contains("February", colHeaders[1].TextContent);
        Assert.Contains("March", colHeaders[2].TextContent);
        Assert.Contains("April", colHeaders[3].TextContent);
    }

    [Fact]
    public void DashboardPage_WithValidData_HeatmapCurrentMonthHighlighted()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        // Root Heatmap uses apr-hdr class for current month
        var currentHeader = cut.Find(".apr-hdr");

        Assert.Contains("April", currentHeader.TextContent);
        Assert.Contains("Now", currentHeader.TextContent);
    }

    [Fact]
    public void DashboardPage_WithValidData_HeatmapHasFourCategoryRows()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var rowHeaders = cut.FindAll(".hm-row-hdr");

        Assert.Equal(4, rowHeaders.Count);
        // Root Heatmap passes category labels with Unicode prefixes
        Assert.Contains("SHIPPED", rowHeaders[0].TextContent);
        Assert.Contains("IN PROGRESS", rowHeaders[1].TextContent);
        Assert.Contains("CARRYOVER", rowHeaders[2].TextContent);
        Assert.Contains("BLOCKERS", rowHeaders[3].TextContent);
    }

    [Fact]
    public void DashboardPage_WithValidData_HeatmapRendersTitle()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("MONTHLY EXECUTION HEATMAP", cut.Markup);
    }

    #endregion

    #region Error State Integration

    [Fact]
    public void DashboardPage_WithError_ShowsErrorPanel()
    {
        var svc = CreateErrorService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void DashboardPage_WithError_DoesNotRenderDashboardSections()
    {
        var svc = CreateErrorService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // Error state should not render any dashboard sections
        Assert.DoesNotContain("hm-title", cut.Markup);
        Assert.DoesNotContain("tl-svg-box", cut.Markup);
    }

    [Fact]
    public void DashboardPage_WithError_ShowsHelpText()
    {
        var svc = CreateErrorService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Check data.json for errors and restart the application", cut.Markup);
    }

    [Fact]
    public void DashboardPage_WithValidationError_ShowsSpecificError()
    {
        var invalidJson = TestDataHelper.SerializeToJson(new
        {
            title = "",
            subtitle = "Valid",
            months = new[] { "Jan" },
            timeline = new { tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } }
        });
        var path = Path.Combine(_tempDir, "invalid.json");
        File.WriteAllText(path, invalidJson);
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();

        Services.AddSingleton(svc);
        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
    }

    #endregion

    #region Heatmap ↔ HeatmapRow ↔ HeatmapCell Integration

    [Fact]
    public void Heatmap_WithPopulatedData_CellsRenderItems()
    {
        var heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["january"] = new() { "Alpha Release" },
                ["february"] = new() { "Beta Release", "Hotfix 1" }
            },
            InProgress = new Dictionary<string, List<string>>
            {
                ["february"] = new() { "Feature X" }
            },
            Carryover = new(),
            Blockers = new()
        };

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, heatmap)
            .Add(x => x.Months, new List<string> { "January", "February" })
            .Add(x => x.CurrentMonth, "February"));

        Assert.Contains("Alpha Release", cut.Markup);
        Assert.Contains("Beta Release", cut.Markup);
        Assert.Contains("Hotfix 1", cut.Markup);
        Assert.Contains("Feature X", cut.Markup);
    }

    [Fact]
    public void Heatmap_EmptyMonthCells_ShowDash()
    {
        var heatmap = new HeatmapData
        {
            Shipped = new(),
            InProgress = new(),
            Carryover = new(),
            Blockers = new()
        };

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, heatmap)
            .Add(x => x.Months, new List<string> { "January", "February" })
            .Add(x => x.CurrentMonth, "January"));

        // Empty cells render a dash character
        var cells = cut.FindAll(".hm-cell");
        Assert.Equal(8, cells.Count); // 4 categories × 2 months
        // Each empty cell contains a dash
        foreach (var cell in cells)
        {
            Assert.Contains("-", cell.InnerHtml);
        }
    }

    [Fact]
    public void Heatmap_CurrentMonthCells_HaveAprClass()
    {
        var heatmap = new HeatmapData
        {
            Shipped = new(),
            InProgress = new(),
            Carryover = new(),
            Blockers = new()
        };

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, heatmap)
            .Add(x => x.Months, new List<string> { "January", "February" })
            .Add(x => x.CurrentMonth, "February"));

        // Root HeatmapCell uses "apr" class for current month
        var aprCells = cut.FindAll(".hm-cell.apr");
        Assert.Equal(4, aprCells.Count); // 4 category rows
    }

    [Fact]
    public void Heatmap_CssPrefixesFlowToChildren()
    {
        var heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>> { ["january"] = new() { "Item" } },
            InProgress = new Dictionary<string, List<string>> { ["january"] = new() { "Item" } },
            Carryover = new Dictionary<string, List<string>> { ["january"] = new() { "Item" } },
            Blockers = new Dictionary<string, List<string>> { ["january"] = new() { "Item" } }
        };

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, heatmap)
            .Add(x => x.Months, new List<string> { "January" })
            .Add(x => x.CurrentMonth, ""));

        Assert.NotNull(cut.Find(".ship-cell"));
        Assert.NotNull(cut.Find(".prog-cell"));
        Assert.NotNull(cut.Find(".carry-cell"));
        Assert.NotNull(cut.Find(".block-cell"));
    }

    #endregion

    #region Header ↔ Data Integration

    [Fact]
    public void Header_WithFullData_RendersAllSections()
    {
        var data = new DashboardData
        {
            Title = "Full Header Test",
            Subtitle = "Team Z - May 2026",
            BacklogLink = "https://ado.example.com/full",
            CurrentMonth = "May",
            Months = new List<string> { "May" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-07-01",
                NowDate = "2026-05-15",
                Tracks = new List<TimelineTrack>()
            },
            Heatmap = new HeatmapData()
        };

        var cut = RenderComponent<Header>(p => p.Add(x => x.Data, data));

        Assert.Contains("Full Header Test", cut.Find("h1").TextContent);
        Assert.Contains("Team Z - May 2026", cut.Find(".sub").TextContent);
        Assert.Equal("https://ado.example.com/full", cut.Find("a").GetAttribute("href"));
        // NowLabel includes month and year from NowDate
        Assert.Contains("Now (May", cut.Markup);
        Assert.Contains("PoC Milestone", cut.Markup);
        Assert.Contains("Production Release", cut.Markup);
        Assert.Contains("Checkpoint", cut.Markup);
    }

    [Fact]
    public void Header_WithoutBacklogLink_OmitsLink()
    {
        var data = new DashboardData
        {
            Title = "No Link",
            Subtitle = "Sub",
            BacklogLink = "",
            CurrentMonth = "Jan",
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-07-01",
                NowDate = "2026-01-15",
                Tracks = new List<TimelineTrack>()
            },
            Heatmap = new HeatmapData()
        };

        var cut = RenderComponent<Header>(p => p.Add(x => x.Data, data));

        Assert.Empty(cut.FindAll("a"));
    }

    #endregion

    #region Timeline ↔ Data Integration

    [Fact]
    public void Timeline_WithMultipleTracks_RendersAllTracksAndMilestones()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Platform", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-02-15", Type = "poc", Label = "PoC Done" },
                        new() { Date = "2026-06-01", Type = "production", Label = "GA Release" }
                    }
                },
                new()
                {
                    Name = "M2", Label = "Pipeline", Color = "#34A853",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-03-15", Type = "checkpoint", Label = "Check 1" }
                    }
                },
                new()
                {
                    Name = "M3", Label = "Security", Color = "#EA4335",
                    Milestones = new List<Milestone>()
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // Track names and labels rendered
        Assert.Contains("M1", cut.Markup);
        Assert.Contains("Platform", cut.Markup);
        Assert.Contains("M2", cut.Markup);
        Assert.Contains("Pipeline", cut.Markup);
        Assert.Contains("M3", cut.Markup);
        Assert.Contains("Security", cut.Markup);

        // SVG contains milestone labels
        Assert.Contains("PoC Done", cut.Markup);
        Assert.Contains("GA Release", cut.Markup);
        Assert.Contains("Check 1", cut.Markup);

        // SVG contains the correct colors
        Assert.Contains("#4285F4", cut.Markup);
        Assert.Contains("#34A853", cut.Markup);
        Assert.Contains("#EA4335", cut.Markup);

        // Contains all milestone types
        Assert.Contains("#F4B400", cut.Markup); // poc diamond
        Assert.Contains("circle", cut.Markup);  // checkpoint
    }

    [Fact]
    public void Timeline_MonthLabels_CorrespondToDateRange()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-03-01",
            EndDate = "2026-06-01",
            NowDate = "2026-04-15",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "T1", Label = "Test", Color = "#000", Milestones = new() }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        Assert.Contains("Mar", cut.Markup);
        Assert.Contains("Apr", cut.Markup);
        Assert.Contains("May", cut.Markup);
    }

    #endregion

    #region Full Data Flow: JSON String → Service → Dashboard Page → All Components

    [Fact]
    public void FullPipeline_JsonToRenderedDashboard_AllDataVisible()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // Header
        Assert.Contains("Integration Test Dashboard", cut.Markup);
        Assert.Contains("QA Team - April 2026", cut.Markup);
        Assert.Contains("ADO Backlog", cut.Markup);

        // Legend
        Assert.Contains("PoC Milestone", cut.Markup);
        Assert.Contains("Production Release", cut.Markup);
        Assert.Contains("Checkpoint", cut.Markup);

        // Timeline tracks
        Assert.Contains("Core Platform", cut.Markup);
        Assert.Contains("Data Pipeline", cut.Markup);

        // Timeline SVG milestones
        Assert.Contains("Feb 15 PoC", cut.Markup);
        Assert.Contains("May 1 GA", cut.Markup);
        Assert.Contains("NOW", cut.Markup);

        // Heatmap structure
        Assert.Contains("MONTHLY EXECUTION HEATMAP", cut.Markup);
        Assert.Contains("SHIPPED", cut.Markup);
        Assert.Contains("IN PROGRESS", cut.Markup);
        Assert.Contains("CARRYOVER", cut.Markup);
        Assert.Contains("BLOCKERS", cut.Markup);
    }

    #endregion

    protected new void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
            _disposed = true;
        }
        base.Dispose(disposing);
    }

    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}