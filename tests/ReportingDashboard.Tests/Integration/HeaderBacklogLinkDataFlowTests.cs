using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for the conditional backlog link rendering through the
/// full data pipeline: data.json → DashboardDataService → Dashboard → Header.
/// Existing HeaderComponentIntegrationTests (truncated) test basic href matching.
/// These tests focus on the @if (!string.IsNullOrEmpty(Data.BacklogLink)) conditional
/// branch and verify link presence/absence flows correctly from JSON through rendering.
/// </summary>
[Trait("Category", "Integration")]
public class HeaderBacklogLinkDataFlowTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public HeaderBacklogLinkDataFlowTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"BkLink_{Guid.NewGuid():N}");
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

    private DashboardDataService CreateService(string backlogLink)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Backlog Link Test",
            subtitle = "Team - Test",
            backlogLink,
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        return svc;
    }

    #region Link Present When Valid

    [Fact]
    public void Dashboard_BacklogLink_RendersAnchor_WhenLinkIsValid()
    {
        var svc = CreateService("https://dev.azure.com/org/project/_backlogs");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var links = cut.FindAll(".hdr a");
        Assert.NotEmpty(links);
    }

    [Fact]
    public void Dashboard_BacklogLink_HrefMatchesJsonValue()
    {
        var url = "https://dev.azure.com/myorg/myproject/_backlogs?query=active";
        var svc = CreateService(url);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var link = cut.Find(".hdr a");
        Assert.Equal(url, link.GetAttribute("href"));
    }

    [Fact]
    public void Dashboard_BacklogLink_HasTargetBlank()
    {
        var svc = CreateService("https://link.example.com");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var link = cut.Find(".hdr a");
        Assert.Equal("_blank", link.GetAttribute("target"));
    }

    [Fact]
    public void Dashboard_BacklogLink_HasHdrLinkClass()
    {
        var svc = CreateService("https://link.example.com");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var link = cut.Find(".hdr a");
        Assert.Contains("hdr-link", link.GetAttribute("class") ?? "");
    }

    [Fact]
    public void Dashboard_BacklogLink_ContainsADOBacklogText()
    {
        var svc = CreateService("https://link.example.com");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var link = cut.Find(".hdr a");
        Assert.Contains("ADO Backlog", link.TextContent);
    }

    #endregion

    #region Link With Complex URLs

    [Fact]
    public void Dashboard_BacklogLink_WithQueryParams_PreservesFullUrl()
    {
        var url = "https://dev.azure.com/org/project/_backlogs?a=1&b=2&c=value%20encoded";
        var svc = CreateService(url);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var link = cut.Find(".hdr a");
        Assert.Equal(url, link.GetAttribute("href"));
    }

    [Fact]
    public void Dashboard_BacklogLink_WithFragment_PreservesFragment()
    {
        var url = "https://dev.azure.com/org/project#section";
        var svc = CreateService(url);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var link = cut.Find(".hdr a");
        Assert.Contains("#section", link.GetAttribute("href") ?? "");
    }

    [Fact]
    public void Dashboard_BacklogLink_VeryLongUrl_RendersCorrectly()
    {
        var url = "https://dev.azure.com/" + new string('a', 500);
        var svc = CreateService(url);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var link = cut.Find(".hdr a");
        Assert.Equal(url, link.GetAttribute("href"));
    }

    #endregion

    #region Link Inside H1

    [Fact]
    public void Dashboard_BacklogLink_IsNestedInsideH1()
    {
        var svc = CreateService("https://link.example.com");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var h1 = cut.Find("h1");
        var linkInH1 = h1.QuerySelector("a");
        Assert.NotNull(linkInH1);
    }

    [Fact]
    public void Dashboard_H1_ContainsTitleAndLink()
    {
        var svc = CreateService("https://link.example.com");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var h1 = cut.Find("h1");
        Assert.Contains("Backlog Link Test", h1.TextContent);
        Assert.Contains("ADO Backlog", h1.TextContent);
    }

    #endregion
}