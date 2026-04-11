using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class DashboardPageErrorPanelTests : TestContext
{
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly Mock<ILogger<DashboardDataService>> _mockLogger;

    public DashboardPageErrorPanelTests()
    {
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(e => e.WebRootPath).Returns("fakepath");
        _mockLogger = new Mock<ILogger<DashboardDataService>>();
    }

    private DashboardDataService CreateErrorService(string errorMessage)
    {
        var service = new DashboardDataService(_mockEnv.Object, _mockLogger.Object);
        // Use reflection to set error state since LoadAsync requires file I/O
        var isErrorProp = typeof(DashboardDataService).GetProperty("IsError");
        var errorMsgProp = typeof(DashboardDataService).GetProperty("ErrorMessage");

        if (isErrorProp?.CanWrite == true)
        {
            isErrorProp.SetValue(service, true);
        }
        if (errorMsgProp?.CanWrite == true)
        {
            errorMsgProp.SetValue(service, errorMessage);
        }

        return service;
    }

    private DashboardDataService CreateSuccessService(DashboardData data)
    {
        var service = new DashboardDataService(_mockEnv.Object, _mockLogger.Object);
        var dataProp = typeof(DashboardDataService).GetProperty("Data");
        var isErrorProp = typeof(DashboardDataService).GetProperty("IsError");

        if (dataProp?.CanWrite == true)
        {
            dataProp.SetValue(service, data);
        }
        if (isErrorProp?.CanWrite == true)
        {
            isErrorProp.SetValue(service, false);
        }

        return service;
    }

    [Fact]
    public void Dashboard_WhenServiceHasError_RendersErrorPanel()
    {
        // Arrange
        var service = CreateErrorService("Test error message");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        cut.Find(".error-panel").Should().NotBeNull();
        cut.Markup.Should().Contain("Test error message");
    }

    [Fact]
    public void Dashboard_WhenServiceHasError_DoesNotRenderDashboardContent()
    {
        // Arrange
        var service = CreateErrorService("Error occurred");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        cut.FindAll(".hdr").Should().BeEmpty("dashboard header should not render in error state");
        cut.FindAll(".tl-area").Should().BeEmpty("timeline should not render in error state");
        cut.FindAll(".hm-wrap").Should().BeEmpty("heatmap should not render in error state");
    }

    [Fact]
    public void Dashboard_WhenServiceHasError_PassesErrorMessageToErrorPanel()
    {
        // Arrange
        var expectedMessage = "data.json not found at /path/to/file";
        var service = CreateErrorService(expectedMessage);
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        cut.Markup.Should().Contain(expectedMessage);
        cut.Find("h2").TextContent.Should().Be("Dashboard data could not be loaded");
    }

    [Fact]
    public void Dashboard_WhenServiceHasError_ShowsHintText()
    {
        // Arrange
        var service = CreateErrorService("some error");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        cut.Find(".error-hint").TextContent.Should().Contain("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void Dashboard_WhenServiceHasError_ShowsWarningIcon()
    {
        // Arrange
        var service = CreateErrorService("some error");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        var icon = cut.Find(".error-icon");
        icon.TextContent.Should().Contain("⚠");
    }
}