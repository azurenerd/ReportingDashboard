using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class DashboardPageIntegrationTests : TestContext
{
    private static DashboardData CreateFullTestData() => new()
    {
        Title = "Executive Reporting Dashboard",
        Subtitle = "Cloud Platform Team · Azure Workstream · April 2026",
        BacklogLink = "https://dev.azure.com/org/project/_backlogs",
        CurrentMonth = "Apr",
        Months = new List<string> { "Jan", "Feb", "Mar", "Apr" },
        Timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1",
                    Name = "Core Platform",
                    Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-02-15", Label = "Feb 15", Type = "poc" },
                        new() { Date = "2026-04-01", Label = "Apr 1", Type = "production" },
                        new() { Date = "2026-03-01", Label = "Mar 1", Type = "checkpoint" }
                    }
                },
                new()
                {
                    Id = "M2",
                    Name = "API Gateway",
                    Color = "#34A853",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-05-30", Label = "May 30", Type = "production" }
                    }
                }
            }
        },
        Heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["Jan"] = new() { "Auth service v2", "Logging pipeline" },
                ["Feb"] = new() { "Config management" },
                ["Mar"] = new() { "Rate limiting" },
                ["Apr"] = new() { "Dashboard MVP" }
            },
            InProgress = new Dictionary<string, List<string>>
            {
                ["Apr"] = new() { "Perf testing", "Load balancer" }
            },
            Carryover = new Dictionary<string, List<string>>
            {
                ["Mar"] = new() { "Legacy migration" },
                ["Apr"] = new() { "Legacy migration" }
            },
            Blockers = new Dictionary<string, List<string>>
            {
                ["Apr"] = new() { "License approval" }
            }
        }
    };

    private Mock<DashboardDataService> RegisterMockService(DashboardData? data, bool isError, string? errorMessage)
    {
        var mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<DashboardDataService>>();
        var mock = new Mock<DashboardDataService>(mockEnv.Object, mockLogger.Object);
        mock.SetupGet(s => s.Data).Returns(data);
        mock.SetupGet(s => s.IsError).Returns(isError);
        mock.SetupGet(s => s.ErrorMessage).Returns(errorMessage);
        Services.AddSingleton(mock.Object);
        return mock;
    }

    [Fact]
    public void Dashboard_FullDataFlow_ShouldRenderAllSections()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Header section present
        cut.Find(".hdr").Should().NotBeNull();
        // Timeline section present
        cut.Find(".tl-area").Should().NotBeNull();
        // Heatmap section present
        cut.Find(".hm-wrap").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_FullDataFlow_HeaderDisplaysTitleAndSubtitle()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find("h1").TextContent.Should().Contain("Executive Reporting Dashboard");
        cut.Find(".sub").TextContent.Should().Contain("Cloud Platform Team");
    }

    [Fact]
    public void Dashboard_FullDataFlow_HeaderDisplaysBacklogLink()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var link = cut.Find("a[href]");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/org/project/_backlogs");
        link.GetAttribute("target").Should().Be("_blank");
        link.TextContent.Should().Contain("ADO Backlog");
    }

    [Fact]
    public void Dashboard_FullDataFlow_LegendDisplaysAllFourItems()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var legendItems = cut.FindAll(".legend-item");
        legendItems.Should().HaveCount(4);

        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Production Release");
        cut.Markup.Should().Contain("Checkpoint");
        cut.Markup.Should().Contain("Now (Apr 2026)");
    }

    [Fact]
    public void Dashboard_FullDataFlow_TimelineRendersAllTracks()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var trackLabels = cut.FindAll(".tl-label");
        trackLabels.Should().HaveCount(2);

        var trackIds = cut.FindAll(".tl-id");
        trackIds[0].TextContent.Should().Be("M1");
        trackIds[1].TextContent.Should().Be("M2");

        var trackNames = cut.FindAll(".tl-name");
        trackNames[0].TextContent.Should().Be("Core Platform");
        trackNames[1].TextContent.Should().Be("API Gateway");
    }

    [Fact]
    public void Dashboard_FullDataFlow_TimelineRendersSvgWithMilestones()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var svg = cut.Find("svg");
        svg.Should().NotBeNull();
        svg.GetAttribute("width").Should().Be("1560");

        // PoC milestone (gold diamond)
        cut.Markup.Should().Contain("#F4B400");
        // Production release (green diamond)
        cut.Markup.Should().Contain("#34A853");
        // Checkpoint circle
        cut.FindAll("circle").Should().NotBeEmpty();
        // NOW line
        cut.Markup.Should().Contain("NOW");
        cut.Markup.Should().Contain("#EA4335");
        cut.Markup.Should().Contain("stroke-dasharray=\"5,3\"");
    }

    [Fact]
    public void Dashboard_FullDataFlow_TimelineMilestoneLabelsRendered()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("Feb 15");
        cut.Markup.Should().Contain("Apr 1");
        cut.Markup.Should().Contain("Mar 1");
        cut.Markup.Should().Contain("May 30");
    }

    [Fact]
    public void Dashboard_FullDataFlow_HeatmapRendersTitle()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find(".hm-title").TextContent.Should().Contain("MONTHLY EXECUTION HEATMAP");
    }

    [Fact]
    public void Dashboard_FullDataFlow_HeatmapRendersMonthHeaders()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var colHeaders = cut.FindAll(".hm-col-hdr");
        colHeaders.Should().HaveCount(4);

        // Current month should be highlighted
        cut.Find(".cur-month-hdr").TextContent.Should().Contain("Apr");
    }

    [Fact]
    public void Dashboard_FullDataFlow_HeatmapRendersFourCategoryRows()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        rowHeaders.Should().HaveCount(4);

        cut.Find(".shipped-hdr").Should().NotBeNull();
        cut.Find(".prog-hdr").Should().NotBeNull();
        cut.Find(".carry-hdr").Should().NotBeNull();
        cut.Find(".block-hdr").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_FullDataFlow_HeatmapRendersWorkItems()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var items = cut.FindAll(".it");
        items.Should().NotBeEmpty();

        // Verify specific items appear
        cut.Markup.Should().Contain("Auth service v2");
        cut.Markup.Should().Contain("Dashboard MVP");
        cut.Markup.Should().Contain("Perf testing");
        cut.Markup.Should().Contain("Legacy migration");
        cut.Markup.Should().Contain("License approval");
    }

    [Fact]
    public void Dashboard_FullDataFlow_HeatmapCurrentMonthCellsHighlighted()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Current month cells should have -cur class
        cut.FindAll(".shipped-cur").Should().NotBeEmpty();
        cut.FindAll(".prog-cur").Should().NotBeEmpty();
    }

    [Fact]
    public void Dashboard_FullDataFlow_HeatmapEmptyCellsShowDash()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.FindAll(".empty-cell").Should().NotBeEmpty();
    }

    [Fact]
    public void Dashboard_FullDataFlow_GridTemplateMatchesMonthCount()
    {
        RegisterMockService(CreateFullTestData(), false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var grid = cut.Find(".hm-grid");
        grid.GetAttribute("style").Should().Contain("repeat(4, 1fr)");
    }

    [Fact]
    public void Dashboard_ErrorState_ShouldRenderErrorPanelOnly()
    {
        RegisterMockService(null, true, "Dashboard data file not found: /path/data.json");

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Find("h2").TextContent.Should().Contain("Dashboard data could not be loaded");
        cut.Markup.Should().Contain("Dashboard data file not found");
        cut.Find(".error-hint").TextContent.Should().Contain("Check data.json");

        // No dashboard content
        cut.FindAll(".hdr").Should().BeEmpty();
        cut.FindAll(".tl-area").Should().BeEmpty();
        cut.FindAll(".hm-wrap").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_ErrorState_ParseError_ShouldShowDetailedMessage()
    {
        RegisterMockService(null, true, "Failed to parse data.json: unexpected token at line 5");

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("Failed to parse data.json");
        cut.Markup.Should().Contain("unexpected token");
    }

    [Fact]
    public void Dashboard_NullData_NoError_ShouldRenderBlank()
    {
        RegisterMockService(null, false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.FindAll(".hdr").Should().BeEmpty();
        cut.FindAll(".error-panel").Should().BeEmpty();
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_DataWithNoBacklogLink_ShouldNotRenderLink()
    {
        var data = CreateFullTestData();
        data.BacklogLink = "";
        RegisterMockService(data, false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.FindAll("a").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_DataWithNoTracks_ShouldRenderTimelineAreaEmpty()
    {
        var data = CreateFullTestData();
        data.Timeline.Tracks = new List<TimelineTrack>();
        RegisterMockService(data, false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find(".tl-area").Should().NotBeNull();
        cut.FindAll(".tl-label").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_DataWithEmptyHeatmap_ShouldRenderStructureWithDashes()
    {
        var data = CreateFullTestData();
        data.Heatmap = new HeatmapData();
        RegisterMockService(data, false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find(".hm-wrap").Should().NotBeNull();
        cut.FindAll(".hm-row-hdr").Should().HaveCount(4);
        // All cells should show dash since heatmap is empty
        cut.FindAll(".empty-cell").Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Dashboard_SingleMonth_ShouldRenderMinimalGrid()
    {
        var data = CreateFullTestData();
        data.Months = new List<string> { "Apr" };
        data.CurrentMonth = "Apr";
        RegisterMockService(data, false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var grid = cut.Find(".hm-grid");
        grid.GetAttribute("style").Should().Contain("repeat(1, 1fr)");
        cut.FindAll(".hm-col-hdr").Should().HaveCount(1);
    }

    [Fact]
    public void Dashboard_SixMonths_ShouldRenderWideGrid()
    {
        var data = CreateFullTestData();
        data.Months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
        RegisterMockService(data, false, null);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var grid = cut.Find(".hm-grid");
        grid.GetAttribute("style").Should().Contain("repeat(6, 1fr)");
        cut.FindAll(".hm-col-hdr").Should().HaveCount(6);
    }
}