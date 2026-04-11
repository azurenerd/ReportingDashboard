using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class DashboardPageTests : TestContext
{
    private static DashboardData CreateTestData() => new()
    {
        Title = "Test Dashboard",
        Subtitle = "Team A - April 2026",
        BacklogLink = "https://ado.example.com",
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
                    Name = "Platform",
                    Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-01", Label = "Mar 1", Type = "poc" }
                    }
                }
            }
        },
        Heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["Jan"] = new() { "Feature A" }
            },
            InProgress = new Dictionary<string, List<string>>(),
            Carryover = new Dictionary<string, List<string>>(),
            Blockers = new Dictionary<string, List<string>>()
        }
    };

    private Mock<DashboardDataService> CreateMockService(DashboardData? data, bool isError, string? errorMessage)
    {
        var mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<DashboardDataService>>();
        var mock = new Mock<DashboardDataService>(mockEnv.Object, mockLogger.Object);
        mock.SetupGet(s => s.Data).Returns(data);
        mock.SetupGet(s => s.IsError).Returns(isError);
        mock.SetupGet(s => s.ErrorMessage).Returns(errorMessage);
        return mock;
    }

    [Fact]
    public void Dashboard_WithValidData_ShouldRenderHeader()
    {
        var mock = CreateMockService(CreateTestData(), false, null);
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find(".hdr").Should().NotBeNull();
        cut.Markup.Should().Contain("Test Dashboard");
    }

    [Fact]
    public void Dashboard_WithValidData_ShouldRenderTimeline()
    {
        var mock = CreateMockService(CreateTestData(), false, null);
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find(".tl-area").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_WithValidData_ShouldRenderHeatmap()
    {
        var mock = CreateMockService(CreateTestData(), false, null);
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find(".hm-wrap").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_WithError_ShouldRenderErrorPanel()
    {
        var mock = CreateMockService(null, true, "File not found");
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Markup.Should().Contain("File not found");
    }

    [Fact]
    public void Dashboard_WithError_ShouldNotRenderDashboardContent()
    {
        var mock = CreateMockService(null, true, "Parse error");
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.FindAll(".hdr").Should().BeEmpty();
        cut.FindAll(".tl-area").Should().BeEmpty();
        cut.FindAll(".hm-wrap").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_WithNullData_ShouldRenderNothing()
    {
        var mock = CreateMockService(null, false, null);
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.FindAll(".hdr").Should().BeEmpty();
        cut.FindAll(".error-panel").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_ShouldPassCorrectTitleToHeader()
    {
        var data = CreateTestData();
        data.Title = "Specific Title";
        var mock = CreateMockService(data, false, null);
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find("h1").TextContent.Should().Contain("Specific Title");
    }

    [Fact]
    public void Dashboard_ShouldPassCorrectSubtitleToHeader()
    {
        var data = CreateTestData();
        data.Subtitle = "Custom Subtitle";
        var mock = CreateMockService(data, false, null);
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find(".sub").TextContent.Should().Be("Custom Subtitle");
    }

    [Fact]
    public void Dashboard_ShouldPassBacklogLinkToHeader()
    {
        var data = CreateTestData();
        data.BacklogLink = "https://custom.link.com";
        var mock = CreateMockService(data, false, null);
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find("a").GetAttribute("href").Should().Be("https://custom.link.com");
    }

    [Fact]
    public void Dashboard_ErrorWithEmptyMessage_ShouldStillShowErrorPanel()
    {
        var mock = CreateMockService(null, true, "");
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
    }
}