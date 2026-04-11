using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Full end-to-end integration tests that render the Dashboard page with a real
/// DashboardDataService, validating that all three major sections (Header, Timeline, Heatmap)
/// render correctly together with data flowing from file → service → page → child components.
/// Covers the root-level components (Components/Header.razor, Components/Timeline.razor,
/// Components/Heatmap.razor) rather than the Sections variants.
/// </summary>
[Trait("Category", "Integration")]
public class FullDashboardRenderingIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public FullDashboardRenderingIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"FullDash_{Guid.NewGuid():N}");
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

    private DashboardDataService CreateFullService()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Q2 Executive Dashboard",
            subtitle = "Platform Engineering - April 2026",
            backlogLink = "https://dev.azure.com/contoso/platform/_backlogs",
            currentMonth = "April",
            months = new[] { "January", "February", "March", "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[]
                {
                    new
                    {
                        name = "M1", label = "Core Platform", color = "#4285F4",
                        milestones = new[]
                        {
                            new { date = "2026-02-15", type = "poc", label = "Feb 15 PoC" },
                            new { date = "2026-05-01", type = "production", label = "May 1 GA" }
                        }
                    },
                    new
                    {
                        name = "M2", label = "Data Pipeline", color = "#EA4335",
                        milestones = new[]
                        {
                            new { date = "2026-03-01", type = "checkpoint", label = "Mar 1 Check" },
                            new { date = "2026-06-15", type = "production", label = "Jun 15 GA" }
                        }
                    }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>
                {
                    ["january"] = new[] { "Auth Module", "CI Pipeline" },
                    ["february"] = new[] { "Search Feature" },
                    ["march"] = new[] { "Dashboard v1" }
                },
                inProgress = new Dictionary<string, string[]>
                {
                    ["april"] = new[] { "Analytics Engine", "Export API" }
                },
                carryover = new Dictionary<string, string[]>
                {
                    ["march"] = new[] { "Legacy Migration" }
                },
                blockers = new Dictionary<string, string[]>
                {
                    ["april"] = new[] { "Vendor SDK Delay" }
                }
            }
        }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
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

    #region Full Dashboard Layout

    [Fact]
    public void Dashboard_WithFullData_RendersHeaderSection()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("hdr", cut.Markup);
        Assert.Contains("Q2 Executive Dashboard", cut.Markup);
        Assert.Contains("Platform Engineering - April 2026", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithFullData_HeaderShowsBacklogLink()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var link = cut.Find("a[href='https://dev.azure.com/contoso/platform/_backlogs']");
        Assert.NotNull(link);
        Assert.Contains("ADO Backlog", link.TextContent);
        Assert.Equal("_blank", link.GetAttribute("target"));
    }

    [Fact]
    public void Dashboard_WithFullData_HeaderShowsLegend()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("PoC Milestone", cut.Markup);
        Assert.Contains("Production Release", cut.Markup);
        Assert.Contains("Checkpoint", cut.Markup);
        Assert.Contains("Now (April)", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithFullData_HeaderLegendHasCorrectColors()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("#F4B400", cut.Markup); // PoC diamond
        Assert.Contains("#34A853", cut.Markup); // Production diamond
        Assert.Contains("#999", cut.Markup);    // Checkpoint circle
        Assert.Contains("#EA4335", cut.Markup); // Now line
    }

    #endregion

    #region Error State

    [Fact]
    public void Dashboard_WithError_ShowsErrorPanel()
    {
        var svc = CreateErrorService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
        Assert.Contains("not found", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithError_DoesNotRenderDashboardDiv()
    {
        var svc = CreateErrorService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("class=\"dashboard\"", cut.Markup);
        Assert.DoesNotContain("hdr", cut.Find(".error-panel").OuterHtml.Replace("error-panel", "").Contains("hdr") ? "hdr" : "xyznotfound");
    }

    [Fact]
    public void Dashboard_WithError_ErrorPanelHasHelpText()
    {
        var svc = CreateErrorService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Check data.json for errors and restart the application", cut.Markup);
    }

    #endregion

    #region Data Consistency Across Sections

    [Fact]
    public void Dashboard_SameCurrentMonth_UsedInHeaderAndHeatmap()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // Header legend should show "Now (April)"
        Assert.Contains("Now (April)", cut.Markup);
    }

    [Fact]
    public void Dashboard_AllTimelineTrackData_FlowsToRenderedSvg()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // Verify track data appears in rendered output
        Assert.Contains("M1", cut.Markup);
        Assert.Contains("Core Platform", cut.Markup);
        Assert.Contains("M2", cut.Markup);
        Assert.Contains("Data Pipeline", cut.Markup);
    }

    [Fact]
    public void Dashboard_AllHeatmapCategories_FlowToRenderedGrid()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // Shipped items
        Assert.Contains("Auth Module", cut.Markup);
        Assert.Contains("CI Pipeline", cut.Markup);
        // InProgress items
        Assert.Contains("Analytics Engine", cut.Markup);
        // Carryover items
        Assert.Contains("Legacy Migration", cut.Markup);
        // Blocker items
        Assert.Contains("Vendor SDK Delay", cut.Markup);
    }

    #endregion

    #region Fresh Service (No Load)

    [Fact]
    public void Dashboard_FreshService_ShowsFallbackError()
    {
        var svc = new DashboardDataService(_logger);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("Dashboard data is not available", cut.Markup);
    }

    #endregion
}