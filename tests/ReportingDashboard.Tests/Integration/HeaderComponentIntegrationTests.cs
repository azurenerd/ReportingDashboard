using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for Header.razor (Components/Header.razor - inline-styled version from PR #533)
/// rendered through the Dashboard page with a real DashboardDataService loaded from disk.
/// Complements existing DashboardComponentIntegrationTests by focusing on header-specific
/// data binding, legend rendering, and parameter flow from service → Dashboard → Header.
/// </summary>
[Trait("Category", "Integration")]
public class HeaderComponentIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public HeaderComponentIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HdrInteg_{Guid.NewGuid():N}");
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

    private DashboardDataService CreateServiceWithData(
        string title = "Integration Header Test",
        string subtitle = "Team Alpha - April 2026",
        string backlogLink = "https://dev.azure.com/org/project/_backlogs",
        string currentMonth = "April")
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            title,
            subtitle,
            backlogLink,
            currentMonth,
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
                        name = "M1",
                        label = "Core Platform",
                        color = "#4285F4",
                        milestones = new[]
                        {
                            new { date = "2026-02-15", type = "poc", label = "Feb 15 PoC" }
                        }
                    }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]> { ["jan"] = new[] { "Feature A" } },
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        return CreateServiceFromJson(json);
    }

    #region Header Title Data Flow: Service → Dashboard → Header

    [Fact]
    public void Dashboard_HeaderTitle_MatchesServiceData()
    {
        var svc = CreateServiceWithData(title: "Project Phoenix Dashboard");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var h1 = cut.Find("h1");

        Assert.Contains("Project Phoenix Dashboard", h1.TextContent);
    }

    [Fact]
    public void Dashboard_HeaderTitle_WithLongTitle_RendersWithoutError()
    {
        var longTitle = string.Join(" ", Enumerable.Range(1, 20).Select(i => $"Word{i}"));
        var svc = CreateServiceWithData(title: longTitle);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var h1 = cut.Find("h1");

        Assert.Contains(longTitle, h1.TextContent);
    }

    [Fact]
    public void Dashboard_HeaderTitle_WithSpecialCharacters_EncodesCorrectly()
    {
        var svc = CreateServiceWithData(title: "Project <Alpha> & \"Beta\"");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Project", cut.Markup);
        Assert.Contains("Alpha", cut.Markup);
        Assert.Contains("Beta", cut.Markup);
        // HTML encoding should handle < > & "
        Assert.DoesNotContain("<Alpha>", cut.Markup);
    }

    #endregion

    #region Subtitle Data Flow

    [Fact]
    public void Dashboard_HeaderSubtitle_MatchesServiceData()
    {
        var svc = CreateServiceWithData(subtitle: "DevOps Team - March 2026");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var sub = cut.Find(".sub");

        Assert.Equal("DevOps Team - March 2026", sub.TextContent);
    }

    [Fact]
    public void Dashboard_HeaderSubtitle_WithUnicode_RendersCorrectly()
    {
        var svc = CreateServiceWithData(subtitle: "Équipe développement — Avril 2026");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var sub = cut.Find(".sub");

        Assert.Contains("Équipe développement", sub.TextContent);
    }

    #endregion

    #region Backlog Link Data Flow

    [Fact]
    public void Dashboard_BacklogLink_HrefMatchesServiceData()
    {
        var url = "https://dev.azure.com/myorg/myproject/_backlogs";
        var svc = CreateServiceWithData(backlogLink: url);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var link = cut.Find("a[target='_blank']");

        Assert.Equal(url, link.GetAttribute("href"));
    }

    [Fact]
    public void Dashboard_BacklogLink_OpensInNewTab()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var link = cut.Find("a[target='_blank']");

        Assert.Equal("_blank", link.GetAttribute("target"));
    }

    [Fact]
    public void Dashboard_BacklogLink_HasSecurityRel()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var link = cut.Find("a[target='_blank']");
        var rel = link.GetAttribute("rel") ?? "";

        Assert.Contains("noopener", rel);
    }

    [Fact]
    public void Dashboard_BacklogLink_ContainsADOBacklogText()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var link = cut.Find("a[target='_blank']");

        Assert.Contains("ADO Backlog", link.TextContent);
    }

    [Fact]
    public void Dashboard_BacklogLink_WithQueryParams_PreservesFullUrl()
    {
        var url = "https://dev.azure.com/org/project/_backlogs?query=active&filter=sprint";
        var svc = CreateServiceWithData(backlogLink: url);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var link = cut.Find("a[target='_blank']");

        Assert.Equal(url, link.GetAttribute("href"));
    }

    #endregion

    #region Legend Integration (rendered via inline styles in Components/Header.razor)

    [Fact]
    public void Dashboard_Legend_ContainsAllFourLabels()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var markup = cut.Markup;

        Assert.Contains("PoC Milestone", markup);
        Assert.Contains("Production Release", markup);
        Assert.Contains("Checkpoint", markup);
        Assert.Contains("Now (", markup);
    }

    [Fact]
    public void Dashboard_LegendNow_ShowsCurrentMonthFromService()
    {
        var svc = CreateServiceWithData(currentMonth: "June");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (June)", cut.Markup);
    }

    [Fact]
    public void Dashboard_LegendNow_UpdatesWhenMonthChanges()
    {
        var svc1 = CreateServiceWithData(currentMonth: "January");
        Services.AddSingleton(svc1);

        var cut = RenderComponent<Dashboard>();
        Assert.Contains("Now (January)", cut.Markup);
    }

    [Fact]
    public void Dashboard_Legend_HasCorrectSymbolColors()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var markup = cut.Markup;

        // Gold diamond for PoC
        Assert.Contains("#F4B400", markup);
        // Green diamond for Production
        Assert.Contains("#34A853", markup);
        // Gray circle for Checkpoint
        Assert.Contains("#999", markup);
        // Red bar for NOW
        Assert.Contains("#EA4335", markup);
    }

    [Fact]
    public void Dashboard_Legend_DiamondSymbols_HaveRotation()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("rotate(45deg)", cut.Markup);
    }

    [Fact]
    public void Dashboard_Legend_CheckpointCircle_HasBorderRadius()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("border-radius:50%", cut.Markup);
    }

    [Fact]
    public void Dashboard_Legend_Container_Has22pxGap()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("gap:22px", cut.Markup);
    }

    [Fact]
    public void Dashboard_Legend_Container_Has12pxFont()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("font-size:12px", cut.Markup);
    }

    #endregion

    #region Header Structure Integration

    [Fact]
    public void Dashboard_Header_HasHdrClass()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var hdr = cut.Find(".hdr");
        Assert.NotNull(hdr);
    }

    [Fact]
    public void Dashboard_Header_HasSubtitleDiv()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var sub = cut.Find(".sub");
        Assert.NotNull(sub);
    }

    [Fact]
    public void Dashboard_Header_HasH1WithTitle()
    {
        var svc = CreateServiceWithData(title: "My Dashboard");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var h1 = cut.Find("h1");
        Assert.Contains("My Dashboard", h1.TextContent);
    }

    #endregion

    #region Error State - Header Not Rendered

    [Fact]
    public void Dashboard_WhenServiceError_NoHeaderRendered()
    {
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json")).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("class=\"hdr\"", cut.Markup);
    }

    [Fact]
    public void Dashboard_WhenServiceError_ShowsErrorPanel()
    {
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "missing.json")).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("not found", cut.Markup);
    }

    #endregion

    #region Full Data Pipeline: JSON File → Service → Dashboard → Header

    [Fact]
    public void FullPipeline_JsonToRenderedHeader_AllFieldsPresent()
    {
        var json = """
        {
            "title": "E2E Pipeline Test",
            "subtitle": "Integration Team - May 2026",
            "backlogLink": "https://dev.azure.com/e2e/test",
            "currentMonth": "May",
            "months": ["March", "April", "May", "June"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-05-15",
                "tracks": [
                    {
                        "name": "M1",
                        "label": "Platform",
                        "color": "#4285F4",
                        "milestones": [
                            { "date": "2026-03-01", "type": "poc", "label": "PoC" }
                        ]
                    }
                ]
            },
            "heatmap": {
                "shipped": { "mar": ["Item X"] },
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;

        var svc = CreateServiceFromJson(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // Title
        Assert.Contains("E2E Pipeline Test", cut.Find("h1").TextContent);

        // Subtitle
        Assert.Equal("Integration Team - May 2026", cut.Find(".sub").TextContent);

        // Backlog link
        var link = cut.Find("a[target='_blank']");
        Assert.Equal("https://dev.azure.com/e2e/test", link.GetAttribute("href"));

        // Legend with current month
        Assert.Contains("Now (May)", cut.Markup);

        // All four legend labels
        Assert.Contains("PoC Milestone", cut.Markup);
        Assert.Contains("Production Release", cut.Markup);
        Assert.Contains("Checkpoint", cut.Markup);
    }

    [Fact]
    public void FullPipeline_DifferentMonths_LegendReflectsCorrectMonth()
    {
        var months = new[] { "January", "February", "September", "December" };

        foreach (var month in months)
        {
            using var ctx = new Bunit.TestContext();
            var svc = CreateServiceWithData(currentMonth: month);
            ctx.Services.AddSingleton(svc);

            var cut = ctx.RenderComponent<Dashboard>();

            Assert.Contains($"Now ({month})", cut.Markup);
        }
    }

    #endregion

    #region Service State Transitions with Header

    [Fact]
    public void Dashboard_AfterServiceRecovery_HeaderRendersCorrectly()
    {
        // First load - error
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "missing.json")).GetAwaiter().GetResult();
        Assert.True(svc.IsError);

        // Second load - valid data
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Recovered Dashboard",
            subtitle = "After Recovery - April 2026",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = new[] { new { date = "2026-03-01", type = "poc", label = "P" } } } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        var path = Path.Combine(_tempDir, $"recovered_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        svc.LoadAsync(path).GetAwaiter().GetResult();

        Assert.False(svc.IsError);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Recovered Dashboard", cut.Find("h1").TextContent);
        Assert.Contains("After Recovery - April 2026", cut.Find(".sub").TextContent);
    }

    #endregion
}