using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using ReportingDashboard.Tests.Unit.Components;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Integration tests verifying that Dashboard.razor correctly wires the Header component
/// with DashboardDataService and that Header renders all expected sections when composed
/// within the full page context.
/// </summary>
[Trait("Category", "Integration")]
public class HeaderDashboardIntegrationTests : TestContext
{
    private static DashboardData CreateFullDashboardData(
        string title = "Executive Reporting Dashboard",
        string subtitle = "Engineering · Core Platform · April 2026",
        string backlogLink = "https://dev.azure.com/org/project/_backlogs",
        string currentMonth = "April",
        List<string>? months = null,
        TimelineData? timeline = null,
        HeatmapData? heatmap = null)
    {
        return new DashboardData
        {
            Title = title,
            Subtitle = subtitle,
            BacklogLink = backlogLink,
            CurrentMonth = currentMonth,
            Months = months ?? new List<string> { "January", "February", "March", "April" },
            Timeline = timeline,
            Heatmap = heatmap ?? new HeatmapData()
        };
    }

    private void RegisterDataService(DashboardData? data, bool isError = false, string? errorMessage = null)
    {
        var service = new TestDashboardDataService(data, isError, errorMessage);
        Services.AddSingleton<DashboardDataService>(service);
    }

    // ── Dashboard + Header composition ──

    [Fact]
    public void Dashboard_WithValidData_RendersHeaderInsideDashboardRoot()
    {
        var data = CreateFullDashboardData();
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var root = cut.Find("div.dashboard-root");
        root.Should().NotBeNull();
        var hdr = root.QuerySelector("div.hdr");
        hdr.Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_HeaderReceivesTitleFromDataService()
    {
        var data = CreateFullDashboardData(title: "My Project Alpha");
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var h1 = cut.Find("div.hdr h1");
        h1.TextContent.Should().Contain("My Project Alpha");
    }

    [Fact]
    public void Dashboard_HeaderReceivesSubtitleFromDataService()
    {
        var data = CreateFullDashboardData(subtitle: "Team Bravo · Q2 2026");
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var sub = cut.Find("div.hdr div.sub");
        sub.TextContent.Should().Be("Team Bravo · Q2 2026");
    }

    [Fact]
    public void Dashboard_HeaderReceivesBacklogLinkFromDataService()
    {
        var data = CreateFullDashboardData(backlogLink: "https://dev.azure.com/test");
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var link = cut.Find("div.hdr a");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/test");
        link.GetAttribute("target").Should().Be("_blank");
    }

    [Fact]
    public void Dashboard_HeaderRendersAllFourLegendSymbols()
    {
        var data = CreateFullDashboardData();
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Production Release");
        cut.Markup.Should().Contain("Checkpoint");
        cut.Markup.Should().Contain("Now (April)");
    }

    [Fact]
    public void Dashboard_HeaderLegendNowLabel_UsesTimelineNowDate_WhenPresent()
    {
        var data = CreateFullDashboardData(
            currentMonth: "March",
            timeline: new TimelineData
            {
                NowDate = "2026-06-15",
                Tracks = new List<TimelineTrack>()
            });
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("Now (June 2026)");
    }

    [Fact]
    public void Dashboard_HeaderLegendNowLabel_FallsBackToCurrentMonth_WhenNoTimeline()
    {
        var data = CreateFullDashboardData(currentMonth: "September", timeline: null);
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("Now (September)");
    }

    // ── Error state: Header should NOT render ──

    [Fact]
    public void Dashboard_WhenError_DoesNotRenderHeader()
    {
        RegisterDataService(null, isError: true, errorMessage: "data.json not found");

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.FindAll("div.hdr").Should().BeEmpty();
        cut.Find("div.error-panel").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_WhenError_ShowsErrorMessageInPanel()
    {
        RegisterDataService(null, isError: true, errorMessage: "Failed to parse data.json: unexpected token");

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("Failed to parse data.json: unexpected token");
        cut.Markup.Should().Contain("Dashboard data could not be loaded");
    }

    [Fact]
    public void Dashboard_WhenNullData_RendersNeither_HeaderNorError()
    {
        RegisterDataService(null, isError: false);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.FindAll("div.hdr").Should().BeEmpty();
        cut.FindAll("div.error-panel").Should().BeEmpty();
        cut.FindAll("div.dashboard-root").Should().BeEmpty();
    }

    // ── Header + Timeline co-existence ──

    [Fact]
    public void Dashboard_WithTimelineData_RendersHeaderBeforeTimeline()
    {
        var data = CreateFullDashboardData(
            timeline: new TimelineData
            {
                NowDate = "2026-04-11",
                Tracks = new List<TimelineTrack>()
            });
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var root = cut.Find("div.dashboard-root");
        var children = root.Children;
        children.Length.Should().BeGreaterOrEqualTo(2);
        children[0].ClassName.Should().Contain("hdr");
    }

    [Fact]
    public void Dashboard_WithoutTimelineData_RendersHeaderAndHeatmapOnly()
    {
        var data = CreateFullDashboardData(timeline: null);
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find("div.hdr").Should().NotBeNull();
        cut.Find("div.dashboard-root").Should().NotBeNull();
    }

    // ── Header with empty/edge-case data from service ──

    [Fact]
    public void Dashboard_HeaderWithEmptyBacklogLink_DoesNotRenderLink()
    {
        var data = CreateFullDashboardData(backlogLink: "");
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.FindAll("div.hdr a").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_HeaderWithEmptyTitle_RendersH1WithoutError()
    {
        var data = CreateFullDashboardData(title: "");
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var h1 = cut.Find("div.hdr h1");
        h1.Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_HeaderWithSpecialCharactersInTitle_RendersCorrectly()
    {
        var data = CreateFullDashboardData(title: "Project <Alpha> & \"Beta\" · Γ");
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var h1 = cut.Find("div.hdr h1");
        h1.TextContent.Should().Contain("Project");
    }

    [Fact]
    public void Dashboard_HeaderWithLongSubtitle_RendersWithoutTruncation()
    {
        var longSubtitle = string.Join(" · ", Enumerable.Range(1, 20).Select(i => $"Department{i}"));
        var data = CreateFullDashboardData(subtitle: longSubtitle);
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var sub = cut.Find("div.hdr div.sub");
        sub.TextContent.Should().Contain("Department1");
        sub.TextContent.Should().Contain("Department20");
    }

    // ── Legend colors integration ──

    [Fact]
    public void Dashboard_HeaderLegend_PoCDiamondHasGoldBackground()
    {
        var data = CreateFullDashboardData();
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("background:#F4B400");
    }

    [Fact]
    public void Dashboard_HeaderLegend_ProductionDiamondHasGreenBackground()
    {
        var data = CreateFullDashboardData();
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("background:#34A853");
    }

    [Fact]
    public void Dashboard_HeaderLegend_CheckpointHasGrayCircle()
    {
        var data = CreateFullDashboardData();
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("background:#999");
        cut.Markup.Should().Contain("border-radius:50%");
    }

    [Fact]
    public void Dashboard_HeaderLegend_NowBarHasRedBackground()
    {
        var data = CreateFullDashboardData();
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("background:#EA4335");
    }

    // ── Full data flow: service → page → header ──

    [Fact]
    public void Dashboard_AllHeaderFieldsFlowFromServiceToComponent()
    {
        var data = CreateFullDashboardData(
            title: "Integration Test Title",
            subtitle: "Integration · Test · May 2026",
            backlogLink: "https://dev.azure.com/integ/test",
            currentMonth: "May");
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find("div.hdr h1").TextContent.Should().Contain("Integration Test Title");
        cut.Find("div.hdr div.sub").TextContent.Should().Be("Integration · Test · May 2026");
        var link = cut.Find("div.hdr a");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/integ/test");
        cut.Markup.Should().Contain("Now (May)");
    }

    [Fact]
    public void Dashboard_HeaderStructure_HasTwoMainChildren()
    {
        var data = CreateFullDashboardData();
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var hdr = cut.Find("div.hdr");
        hdr.Children.Length.Should().Be(2, "Header should have left content div and right legend div");
    }

    [Fact]
    public void Dashboard_HeaderLinkHasSecurityAttributes()
    {
        var data = CreateFullDashboardData();
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var link = cut.Find("div.hdr a");
        link.GetAttribute("rel").Should().Contain("noopener");
        link.GetAttribute("rel").Should().Contain("noreferrer");
    }
}