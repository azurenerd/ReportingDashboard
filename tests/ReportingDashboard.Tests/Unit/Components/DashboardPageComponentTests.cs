using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Components;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class DashboardPageComponentTests : TestContext
{
    private readonly Mock<ILogger<DashboardDataService>> _loggerMock;

    public DashboardPageComponentTests()
    {
        _loggerMock = new Mock<ILogger<DashboardDataService>>();
    }

    private DashboardDataService CreateServiceWithData(DashboardData data)
    {
        var service = new DashboardDataService(_loggerMock.Object);
        // Use reflection to set properties since they have private setters
        typeof(DashboardDataService).GetProperty("Data")!.SetValue(service, data);
        typeof(DashboardDataService).GetProperty("IsError")!.SetValue(service, false);
        typeof(DashboardDataService).GetProperty("ErrorMessage")!.SetValue(service, null);
        return service;
    }

    private DashboardDataService CreateServiceWithError(string errorMessage)
    {
        var service = new DashboardDataService(_loggerMock.Object);
        typeof(DashboardDataService).GetProperty("IsError")!.SetValue(service, true);
        typeof(DashboardDataService).GetProperty("ErrorMessage")!.SetValue(service, errorMessage);
        typeof(DashboardDataService).GetProperty("Data")!.SetValue(service, null);
        return service;
    }

    private static DashboardData CreateValidData() => new()
    {
        Title = "Test Dashboard",
        Subtitle = "Test Subtitle - April 2026",
        BacklogLink = "https://dev.azure.com/test",
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
                    Name = "M1",
                    Label = "Feature A",
                    Color = "#0078D4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-03-01", Type = "poc", Label = "PoC" }
                    }
                }
            }
        },
        Heatmap = new HeatmapData()
    };

    [Fact]
    public void WhenError_RendersErrorPanel()
    {
        var service = CreateServiceWithError("data.json not found");
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Find(".error-title").TextContent.Should().Contain("Dashboard data could not be loaded");
    }

    [Fact]
    public void WhenError_ShowsErrorMessage()
    {
        var service = CreateServiceWithError("File missing at /path/data.json");
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-details").TextContent.Should().Contain("File missing at /path/data.json");
    }

    [Fact]
    public void WhenError_ShowsHelpText()
    {
        var service = CreateServiceWithError("Some error");
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-help").TextContent.Should().Contain("Check data.json for errors");
    }

    [Fact]
    public void WhenValidData_RendersDashboardRoot()
    {
        var service = CreateServiceWithData(CreateValidData());
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".dashboard-root").Should().NotBeNull();
    }

    [Fact]
    public void WhenValidData_DoesNotRenderErrorPanel()
    {
        var service = CreateServiceWithData(CreateValidData());
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".error-panel").Should().BeEmpty();
    }

    [Fact]
    public void WhenValidData_WithTimeline_RendersTimelineArea()
    {
        var service = CreateServiceWithData(CreateValidData());
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".tl-area").Should().NotBeNull();
    }

    [Fact]
    public void WhenValidData_NullTimeline_DoesNotRenderTimeline()
    {
        var data = CreateValidData();
        data.Timeline = null;
        var service = CreateServiceWithData(data);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".tl-area").Should().BeEmpty();
    }

    [Fact]
    public void WhenNullData_DoesNotRenderDashboard()
    {
        var service = new DashboardDataService(_loggerMock.Object);
        // Default state: Data is null, IsError is false
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".dashboard-root").Should().BeEmpty();
        cut.FindAll(".error-panel").Should().BeEmpty();
    }
}