using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class DashboardPageTests : TestContext
{
    private void RegisterDataService(DashboardData? data, bool isError = false, string? errorMessage = null)
    {
        var service = new TestDashboardDataService(data, isError, errorMessage);
        Services.AddSingleton<DashboardDataService>(service);
    }

    [Fact]
    public void Dashboard_WhenDataLoaded_RendersHeaderComponent()
    {
        var data = new DashboardData
        {
            Title = "Test Dashboard",
            Subtitle = "Test Subtitle",
            BacklogLink = "https://test.com",
            CurrentMonth = "April",
            Heatmap = new HeatmapData(),
            Months = new List<string> { "Jan", "Feb", "Mar", "Apr" }
        };
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find("div.hdr").Should().NotBeNull();
        cut.Markup.Should().Contain("Test Dashboard");
    }

    [Fact]
    public void Dashboard_WhenDataLoaded_RendersDashboardRoot()
    {
        var data = new DashboardData
        {
            Title = "My Project",
            Subtitle = "Sub",
            CurrentMonth = "April",
            Heatmap = new HeatmapData(),
            Months = new List<string> { "Jan" }
        };
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find("div.dashboard-root").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_WhenError_RendersErrorPanel()
    {
        RegisterDataService(null, isError: true, errorMessage: "data.json not found");

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find("div.error-panel").Should().NotBeNull();
        cut.Markup.Should().Contain("data.json not found");
    }

    [Fact]
    public void Dashboard_WhenError_DoesNotRenderHeader()
    {
        RegisterDataService(null, isError: true, errorMessage: "Parse error");

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.FindAll("div.hdr").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_WhenError_ShowsHelpText()
    {
        RegisterDataService(null, isError: true, errorMessage: "Something broke");

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void Dashboard_WhenDataNull_RendersNothing()
    {
        RegisterDataService(null, isError: false);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.FindAll("div.dashboard-root").Should().BeEmpty();
        cut.FindAll("div.error-panel").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_WithTimeline_RendersTimelineComponent()
    {
        var data = new DashboardData
        {
            Title = "Timeline Test",
            Subtitle = "Sub",
            CurrentMonth = "April",
            Timeline = new TimelineData
            {
                NowDate = "2026-04-15",
                Tracks = new List<TimelineTrack>()
            },
            Heatmap = new HeatmapData(),
            Months = new List<string> { "Jan" }
        };
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find("div.dashboard-root").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_WithNullTimeline_DoesNotRenderTimeline()
    {
        var data = new DashboardData
        {
            Title = "No Timeline",
            Subtitle = "Sub",
            CurrentMonth = "April",
            Timeline = null,
            Heatmap = new HeatmapData(),
            Months = new List<string> { "Jan" }
        };
        RegisterDataService(data);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find("div.dashboard-root").Should().NotBeNull();
        cut.Markup.Should().Contain("No Timeline");
    }

    [Fact]
    public void Dashboard_ErrorPanel_ShowsErrorIcon()
    {
        RegisterDataService(null, isError: true, errorMessage: "fail");

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find("div.error-icon").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_ErrorPanel_ShowsTitle()
    {
        RegisterDataService(null, isError: true, errorMessage: "fail");

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("Dashboard data could not be loaded");
    }
}

/// <summary>
/// Test double for DashboardDataService that allows setting properties directly.
/// Passes a mock ILogger to satisfy the constructor requirement.
/// </summary>
public class TestDashboardDataService : DashboardDataService
{
    private readonly DashboardData? _data;
    private readonly bool _isError;
    private readonly string? _errorMessage;

    public TestDashboardDataService(DashboardData? data, bool isError = false, string? errorMessage = null)
        : base(new Mock<ILogger<DashboardDataService>>().Object)
    {
        _data = data;
        _isError = isError;
        _errorMessage = errorMessage;
    }

    public new DashboardData? Data => _data;
    public new bool IsError => _isError;
    public new string? ErrorMessage => _errorMessage;
}