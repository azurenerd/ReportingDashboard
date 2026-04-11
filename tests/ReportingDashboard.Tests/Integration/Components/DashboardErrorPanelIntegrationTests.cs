using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Components;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class DashboardErrorPanelIntegrationTests : TestContext
{
    private static Mock<DashboardDataService> CreateMockService(
        bool isError,
        string? errorMessage,
        DashboardData? data)
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockLogger = new Mock<ILogger<DashboardDataService>>();
        var mock = new Mock<DashboardDataService>(mockEnv.Object, mockLogger.Object);
        mock.Setup(s => s.IsError).Returns(isError);
        mock.Setup(s => s.ErrorMessage).Returns(errorMessage);
        mock.Setup(s => s.Data).Returns(data);
        return mock;
    }

    [Fact]
    public void Dashboard_WhenServiceHasError_RendersErrorPanelWithMessage()
    {
        var mock = CreateMockService(true, "Dashboard data file not found: /path/data.json.", null);
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Find("h2").TextContent.Should().Be("Dashboard data could not be loaded");
        cut.Markup.Should().Contain("Dashboard data file not found");
        cut.Find(".error-hint").TextContent.Should().Contain("Check data.json for errors");
    }

    [Fact]
    public void Dashboard_WhenServiceHasJsonParseError_RendersSpecificErrorMessage()
    {
        var mock = CreateMockService(true, "Failed to parse data.json: '$' is an invalid start of a value.", null);
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Markup.Should().Contain("Failed to parse data.json");
        cut.Markup.Should().Contain("invalid start of a value");
    }

    [Fact]
    public void Dashboard_WhenServiceHasNullErrorMessage_RendersErrorPanelWithoutMessageParagraph()
    {
        var mock = CreateMockService(true, null, null);
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
        var paragraphs = cut.FindAll("p");
        paragraphs.Should().HaveCount(1);
        paragraphs[0].ClassList.Should().Contain("error-hint");
    }

    [Fact]
    public void Dashboard_WhenServiceHasError_DoesNotRenderDashboardSections()
    {
        var mock = CreateMockService(true, "Some error", null);
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".hdr").Should().BeEmpty();
        cut.FindAll(".tl-area").Should().BeEmpty();
        cut.FindAll(".hm-wrap").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_WhenServiceHasData_DoesNotRenderErrorPanel()
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            BacklogLink = "https://test.com",
            CurrentMonth = "Apr",
            Months = new List<string> { "Apr" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-06-30",
                NowDate = "2026-04-10",
                Tracks = new List<TimelineTrack>
                {
                    new() { Id = "M1", Name = "Track", Color = "#000" }
                }
            },
            Heatmap = new HeatmapData()
        };
        var mock = CreateMockService(false, null, data);
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".error-panel").Should().BeEmpty();
        cut.Find(".hdr").Should().NotBeNull();
        cut.Find(".tl-area").Should().NotBeNull();
        cut.Find(".hm-wrap").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_WhenDataIsNull_RendersEmptyMarkup()
    {
        var mock = CreateMockService(false, null, null);
        Services.AddSingleton(mock.Object);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Trim().Should().BeEmpty();
    }
}