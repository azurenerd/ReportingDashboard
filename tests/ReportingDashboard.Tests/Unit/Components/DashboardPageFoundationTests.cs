using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Components;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class DashboardPageFoundationTests : TestContext
{
    private readonly Mock<DashboardDataService> _mockDataService;

    public DashboardPageFoundationTests()
    {
        _mockDataService = CreateMockDataService();
        Services.AddSingleton(_mockDataService.Object);
    }

    private static Mock<DashboardDataService> CreateMockDataService()
    {
        var mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<DashboardDataService>>();
        var mock = new Mock<DashboardDataService>(mockEnv.Object, mockLogger.Object);
        return mock;
    }

    private static DashboardData CreateValidData() => new()
    {
        Title = "Test Project Dashboard",
        Subtitle = "Team A – April 2026",
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
                    Id = "M1",
                    Name = "Chatbot",
                    Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-02-15", Label = "Feb 15", Type = "poc" }
                    }
                }
            }
        },
        Heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>> { ["jan"] = new() { "Item A" } },
            InProgress = new Dictionary<string, List<string>>(),
            Carryover = new Dictionary<string, List<string>>(),
            Blockers = new Dictionary<string, List<string>>()
        }
    };

    [Fact]
    public void Dashboard_WhenIsError_RendersErrorPanel()
    {
        _mockDataService.Setup(s => s.IsError).Returns(true);
        _mockDataService.Setup(s => s.ErrorMessage).Returns("File not found");
        _mockDataService.Setup(s => s.Data).Returns((DashboardData?)null);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Markup.Should().Contain("File not found");
    }

    [Fact]
    public void Dashboard_WhenIsError_DoesNotRenderDashboardSections()
    {
        _mockDataService.Setup(s => s.IsError).Returns(true);
        _mockDataService.Setup(s => s.ErrorMessage).Returns("Error");
        _mockDataService.Setup(s => s.Data).Returns((DashboardData?)null);

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".hdr").Should().BeEmpty();
        cut.FindAll(".tl-area").Should().BeEmpty();
        cut.FindAll(".hm-wrap").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_WhenDataValid_RendersHeaderSection()
    {
        _mockDataService.Setup(s => s.IsError).Returns(false);
        _mockDataService.Setup(s => s.Data).Returns(CreateValidData());

        var cut = RenderComponent<Dashboard>();

        cut.Find(".hdr").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_WhenDataValid_RendersTimelineSection()
    {
        _mockDataService.Setup(s => s.IsError).Returns(false);
        _mockDataService.Setup(s => s.Data).Returns(CreateValidData());

        var cut = RenderComponent<Dashboard>();

        cut.Find(".tl-area").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_WhenDataValid_RendersHeatmapSection()
    {
        _mockDataService.Setup(s => s.IsError).Returns(false);
        _mockDataService.Setup(s => s.Data).Returns(CreateValidData());

        var cut = RenderComponent<Dashboard>();

        cut.Find(".hm-wrap").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_WhenDataValid_DoesNotRenderErrorPanel()
    {
        _mockDataService.Setup(s => s.IsError).Returns(false);
        _mockDataService.Setup(s => s.Data).Returns(CreateValidData());

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".error-panel").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_WhenDataNull_RendersNothing()
    {
        _mockDataService.Setup(s => s.IsError).Returns(false);
        _mockDataService.Setup(s => s.Data).Returns((DashboardData?)null);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_WhenIsError_WithNullMessage_RendersErrorPanelWithoutMessage()
    {
        _mockDataService.Setup(s => s.IsError).Returns(true);
        _mockDataService.Setup(s => s.ErrorMessage).Returns((string?)null);
        _mockDataService.Setup(s => s.Data).Returns((DashboardData?)null);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_WhenDataValid_RendersAllThreeSections()
    {
        _mockDataService.Setup(s => s.IsError).Returns(false);
        _mockDataService.Setup(s => s.Data).Returns(CreateValidData());

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".hdr").Should().HaveCount(1);
        cut.FindAll(".tl-area").Should().HaveCount(1);
        cut.FindAll(".hm-wrap").Should().HaveCount(1);
    }
}