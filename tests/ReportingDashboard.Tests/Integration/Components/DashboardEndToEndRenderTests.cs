using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class DashboardEndToEndRenderTests : TestContext, IDisposable
{
    private readonly string _tempDir;

    public DashboardEndToEndRenderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"E2ERender_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    void IDisposable.Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
        base.Dispose();
    }

    private async Task<DashboardDataService> LoadServiceFromJson(string json)
    {
        var logger = new Mock<ILogger<DashboardDataService>>();
        var service = new DashboardDataService(logger.Object);
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
        await service.LoadAsync(path);
        return service;
    }

    private static string FullSampleJson => """
    {
        "title": "E2E Test Dashboard",
        "subtitle": "End-to-End Team - April 2026",
        "backlogLink": "https://dev.azure.com/e2e/project",
        "currentMonth": "Apr",
        "months": ["Jan", "Feb", "Mar", "Apr"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "name": "M1",
                    "label": "Auth Service",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-02-01", "type": "checkpoint", "label": "Spec Review" },
                        { "date": "2026-03-15", "type": "poc", "label": "PoC Done" },
                        { "date": "2026-05-01", "type": "production", "label": "GA" }
                    ]
                }
            ]
        },
        "heatmap": {
            "shipped": { "jan": ["Login Flow"], "feb": ["Token Refresh"] },
            "inProgress": { "mar": ["MFA Integration"] },
            "carryover": { "apr": ["SSO Migration"] },
            "blockers": { "apr": ["IdP Vendor Delay"] }
        }
    }
    """;

    [Fact]
    public async Task FullRender_ValidData_ContainsDashboardRoot()
    {
        var service = await LoadServiceFromJson(FullSampleJson);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".dashboard-root").Should().NotBeNull();
    }

    [Fact]
    public async Task FullRender_ValidData_ContainsTimelineWithSvg()
    {
        var service = await LoadServiceFromJson(FullSampleJson);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".tl-area").Should().NotBeNull();
        cut.Find(".tl-svg-box").Should().NotBeNull();
        cut.Find("svg").Should().NotBeNull();
    }

    [Fact]
    public async Task FullRender_ValidData_ContainsAllMilestoneTypes()
    {
        var service = await LoadServiceFromJson(FullSampleJson);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        // Checkpoint = circle
        cut.FindAll("circle").Should().NotBeEmpty();
        // PoC = gold polygon
        cut.FindAll("polygon[fill='#F4B400']").Should().NotBeEmpty();
        // Production = green polygon
        cut.FindAll("polygon[fill='#34A853']").Should().NotBeEmpty();
    }

    [Fact]
    public async Task FullRender_ValidData_ContainsNowMarker()
    {
        var service = await LoadServiceFromJson(FullSampleJson);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("NOW");
        cut.Markup.Should().Contain("#EA4335");
        cut.Markup.Should().Contain("stroke-dasharray");
    }

    [Fact]
    public async Task FullRender_ValidData_TimelineShowsTrackInfo()
    {
        var service = await LoadServiceFromJson(FullSampleJson);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("M1");
        cut.Markup.Should().Contain("Auth Service");
        cut.Markup.Should().Contain("#0078D4");
    }

    [Fact]
    public async Task FullRender_ErrorState_OnlyShowsErrorPanel()
    {
        var logger = new Mock<ILogger<DashboardDataService>>();
        var service = new DashboardDataService(logger.Object);
        await service.LoadAsync(Path.Combine(_tempDir, "nope.json"));
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
        cut.FindAll(".dashboard-root").Should().BeEmpty();
        cut.FindAll(".tl-area").Should().BeEmpty();
        cut.FindAll("svg").Should().BeEmpty();
    }

    [Fact]
    public async Task FullRender_ErrorState_ContainsAllErrorPanelElements()
    {
        var logger = new Mock<ILogger<DashboardDataService>>();
        var service = new DashboardDataService(logger.Object);
        await service.LoadAsync(Path.Combine(_tempDir, "nope.json"));
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-icon").Should().NotBeNull();
        cut.Find(".error-title").TextContent.Should().Contain("Dashboard data could not be loaded");
        cut.Find(".error-details").TextContent.Should().NotBeEmpty();
        cut.Find(".error-help").TextContent.Should().Contain("Check data.json");
    }

    [Fact]
    public async Task FullRender_MilestoneTooltips_ContainLabels()
    {
        var service = await LoadServiceFromJson(FullSampleJson);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        var titles = cut.FindAll("title");
        var tooltipTexts = titles.Select(t => t.TextContent).ToList();
        tooltipTexts.Should().Contain("Spec Review");
        tooltipTexts.Should().Contain("PoC Done");
        tooltipTexts.Should().Contain("GA");
    }

    [Fact]
    public async Task FullRender_DropShadowFilter_IsPresent()
    {
        var service = await LoadServiceFromJson(FullSampleJson);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find("defs").Should().NotBeNull();
        cut.Find("filter[id='sh']").Should().NotBeNull();
        cut.Markup.Should().Contain("feDropShadow");
    }
}