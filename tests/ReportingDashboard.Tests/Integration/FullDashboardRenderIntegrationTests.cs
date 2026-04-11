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
/// End-to-end integration tests that render the full Dashboard page with
/// a complete data.json and verify all three sections (Header, Timeline, Heatmap)
/// render together with the correct data flowing through all child components.
/// </summary>
[Trait("Category", "Integration")]
public class FullDashboardRenderIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public FullDashboardRenderIntegrationTests()
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
            title = "Privacy Automation Roadmap",
            subtitle = "Trusted Platform – April 2026",
            backlogLink = "https://dev.azure.com/org/project/_backlogs",
            currentMonth = "Apr",
            months = new[] { "Jan", "Feb", "Mar", "Apr" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-06-30",
                nowDate = "2026-04-10",
                tracks = new[]
                {
                    new
                    {
                        name = "M1",
                        label = "Chatbot & MS Role",
                        color = "#0078D4",
                        milestones = new[]
                        {
                            new { date = "2026-01-12", type = "checkpoint", label = "Jan 12" },
                            new { date = "2026-03-26", type = "poc", label = "Mar 26 PoC" },
                            new { date = "2026-05-01", type = "production", label = "May 1 GA" }
                        }
                    },
                    new
                    {
                        name = "M2",
                        label = "Privacy Policy Engine",
                        color = "#00897B",
                        milestones = new[]
                        {
                            new { date = "2026-02-05", type = "checkpoint", label = "Feb 5" },
                            new { date = "2026-04-15", type = "poc", label = "Apr 15 PoC" }
                        }
                    },
                    new
                    {
                        name = "M3",
                        label = "Audit & Compliance",
                        color = "#546E7A",
                        milestones = new[]
                        {
                            new { date = "2026-01-20", type = "checkpoint", label = "Jan 20" },
                            new { date = "2026-05-10", type = "poc", label = "May 10 PoC" }
                        }
                    }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>
                {
                    ["jan"] = new[] { "Auth Module", "CI Pipeline" },
                    ["feb"] = new[] { "Search Feature" },
                    ["mar"] = new[] { "Dashboard v1" }
                },
                inProgress = new Dictionary<string, string[]>
                {
                    ["mar"] = new[] { "Analytics Engine" },
                    ["apr"] = new[] { "Export API" }
                },
                carryover = new Dictionary<string, string[]>
                {
                    ["apr"] = new[] { "Legacy Migration" }
                },
                blockers = new Dictionary<string, string[]>
                {
                    ["apr"] = new[] { "License Review" }
                }
            }
        }, JsonOpts);

        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        return svc;
    }

    #region All Three Sections Present

    [Fact]
    public void FullDashboard_RendersHeaderTimelineAndHeatmap()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // Header
        Assert.Contains("Privacy Automation Roadmap", cut.Markup);
        Assert.Contains("Trusted Platform", cut.Markup);

        // Timeline
        Assert.Contains("M1", cut.Markup);
        Assert.Contains("Chatbot", cut.Markup);
        Assert.Contains("svg", cut.Markup);

        // Heatmap
        Assert.Contains("MONTHLY EXECUTION HEATMAP", cut.Markup);
        Assert.Contains("Auth Module", cut.Markup);
    }

    #endregion

    #region Header Data Flows Correctly

    [Fact]
    public void FullDashboard_HeaderShowsTitle()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var h1 = cut.Find("h1");

        Assert.Contains("Privacy Automation Roadmap", h1.TextContent);
    }

    [Fact]
    public void FullDashboard_HeaderShowsBacklogLink()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var link = cut.Find("a[href='https://dev.azure.com/org/project/_backlogs']");

        Assert.NotNull(link);
        Assert.Contains("ADO Backlog", link.TextContent);
    }

    [Fact]
    public void FullDashboard_HeaderShowsLegendItems()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("PoC Milestone", cut.Markup);
        Assert.Contains("Production Release", cut.Markup);
        Assert.Contains("Checkpoint", cut.Markup);
        Assert.Contains("Now (", cut.Markup);
    }

    #endregion

    #region Timeline Data Flows Correctly

    [Fact]
    public void FullDashboard_TimelineShows3Tracks()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("M1", cut.Markup);
        Assert.Contains("Chatbot", cut.Markup);
        Assert.Contains("#0078D4", cut.Markup);

        Assert.Contains("M2", cut.Markup);
        Assert.Contains("Privacy Policy Engine", cut.Markup);
        Assert.Contains("#00897B", cut.Markup);

        Assert.Contains("M3", cut.Markup);
        Assert.Contains("Audit", cut.Markup);
        Assert.Contains("#546E7A", cut.Markup);
    }

    [Fact]
    public void FullDashboard_TimelineShowsNowMarker()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("NOW", cut.Markup);
        Assert.Contains("stroke-dasharray=\"5,3\"", cut.Markup);
    }

    [Fact]
    public void FullDashboard_TimelineShowsMilestoneLabels()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Mar 26 PoC", cut.Markup);
        Assert.Contains("May 1 GA", cut.Markup);
        Assert.Contains("Apr 15 PoC", cut.Markup);
    }

    [Fact]
    public void FullDashboard_TimelineShowsAllMilestoneTypes()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // PoC diamonds (gold)
        Assert.Contains("#F4B400", cut.Markup);
        // Production diamonds (green)
        Assert.Contains("#34A853", cut.Markup);
        // Checkpoint circles
        Assert.Contains("circle", cut.Markup);
    }

    #endregion

    #region Heatmap Data Flows Correctly

    [Fact]
    public void FullDashboard_HeatmapShows4Months()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.Equal(4, headers.Count);
    }

    [Fact]
    public void FullDashboard_HeatmapCurrentMonthHighlighted()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var headers = cut.FindAll(".hm-col-hdr");
        var aprHeader = headers[3]; // Apr is 4th month
        Assert.Contains("apr-hdr", aprHeader.GetAttribute("class") ?? "");
        Assert.Contains("Now", aprHeader.TextContent);
    }

    [Fact]
    public void FullDashboard_HeatmapRendersAllCategoryItems()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // Shipped
        Assert.Contains("Auth Module", cut.Markup);
        Assert.Contains("CI Pipeline", cut.Markup);
        Assert.Contains("Search Feature", cut.Markup);
        Assert.Contains("Dashboard v1", cut.Markup);

        // InProgress
        Assert.Contains("Analytics Engine", cut.Markup);
        Assert.Contains("Export API", cut.Markup);

        // Carryover
        Assert.Contains("Legacy Migration", cut.Markup);

        // Blockers
        Assert.Contains("License Review", cut.Markup);
    }

    #endregion

    #region No Error State When Data Is Valid

    [Fact]
    public void FullDashboard_NoErrorPanel_WhenDataValid()
    {
        var svc = CreateFullService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("error-panel", cut.Markup);
        Assert.DoesNotContain("Dashboard data could not be loaded", cut.Markup);
    }

    #endregion
}