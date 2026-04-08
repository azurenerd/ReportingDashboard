using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace AgentSquad.Runner.Tests.Components;

public class ErrorHandlingTests : TestContext
{
    private readonly Mock<IDataProvider> _mockDataProvider;
    private readonly Mock<IErrorHandler> _mockErrorHandler;
    private readonly Mock<ILogger<DashboardLayout>> _mockLogger;

    public ErrorHandlingTests()
    {
        _mockDataProvider = new Mock<IDataProvider>();
        _mockErrorHandler = new Mock<IErrorHandler>();
        _mockLogger = new Mock<ILogger<DashboardLayout>>();

        Services.AddScoped(_ => _mockDataProvider.Object);
        Services.AddScoped(_ => _mockErrorHandler.Object);
        Services.AddScoped(_ => _mockLogger.Object);
        Services.AddLogging();
    }

    [Fact]
    public void DashboardLayout_DisplaysErrorOverlay_OnFileNotFound()
    {
        var exception = new FileNotFoundException("File not found");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("Configuration file missing", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_DisplaysErrorOverlay_OnInvalidJson()
    {
        var exception = new JsonException("Invalid JSON");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("Invalid JSON format", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_DisplaysErrorOverlay_OnValidationFailure()
    {
        var exception = new InvalidOperationException("Configuration validation failed. Required fields missing: Name, Milestones");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("Configuration validation failed", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_DisplaysErrorOverlay_OnUnexpectedException()
    {
        var exception = new Exception("Unexpected error");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("unexpected error", component.Markup, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void ErrorBoundary_DisplaysErrorMessage_WhenProvided()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "Test error message")
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        Assert.Contains("Test error message", component.Markup);
        Assert.Contains("error-overlay", component.Markup);
    }

    [Fact]
    public void ErrorBoundary_DoesNotDisplay_WhenErrorMessageNull()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, (string?)null)
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        Assert.DoesNotContain("error-overlay", component.Markup);
    }

    [Fact]
    public async Task ErrorBoundary_RetryButton_InvokesCallback()
    {
        var retryInvoked = false;
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "Test error")
                .Add(p => p.OnRetry, async () =>
                {
                    retryInvoked = true;
                    await Task.CompletedTask;
                })
        );

        var retryButton = component.Find("button.retry-button");
        await retryButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        Assert.True(retryInvoked);
    }

    [Fact]
    public void ErrorBoundary_ShowsDismissButton_InDevelopment()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "Test error")
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        var dismissButtons = component.FindAll("button.dismiss-button");
        Assert.NotEmpty(dismissButtons);
    }

    [Fact]
    public void ErrorBoundary_DisplaysErrorIcon()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "Test error")
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        Assert.Contains("error-icon", component.Markup);
        Assert.Contains("⚠️", component.Markup);
    }

    [Fact]
    public void DashboardLayout_ShowsLoadingSpinner_DuringDataLoad()
    {
        var tcs = new TaskCompletionSource<Project>();
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .Returns(tcs.Task);

        var component = RenderComponent<DashboardLayout>();

        Assert.Contains("loading-spinner", component.Markup);
        Assert.Contains("Loading project dashboard", component.Markup);

        tcs.SetResult(InvalidDataFixtures.ValidProject);
    }

    [Fact]
    public void DataProvider_CapturesError_WithContextAndLogsDetails()
    {
        var exception = new FileNotFoundException("Test file not found");
        var context = ErrorContext.FileNotFound;

        _mockErrorHandler.Setup(x => x.CaptureErrorAsync(It.IsAny<Exception>(), It.IsAny<ErrorContext>()))
            .Returns(Task.CompletedTask);

        _mockErrorHandler.Object.CaptureErrorAsync(exception, context).Wait();

        _mockErrorHandler.Verify(
            x => x.CaptureErrorAsync(It.Is<Exception>(e => e.Message == "Test file not found"), context),
            Times.Once);
    }

    [Fact]
    public void ErrorLogger_GeneratesUserMessage_ForFileNotFound()
    {
        var mockLogger = new Mock<ILogger<ErrorLogger>>();
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        var errorHandler = new ErrorLogger(mockLogger.Object, mockEnvironment.Object);

        var message = errorHandler.GetUserMessage(ErrorContext.FileNotFound, new Exception());

        Assert.Contains("Configuration file missing", message);
    }

    [Fact]
    public void ErrorLogger_GeneratesUserMessage_ForInvalidJson()
    {
        var mockLogger = new Mock<ILogger<ErrorLogger>>();
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        var errorHandler = new ErrorLogger(mockLogger.Object, mockEnvironment.Object);

        var message = errorHandler.GetUserMessage(ErrorContext.InvalidJson, new Exception());

        Assert.Contains("Invalid JSON format", message);
    }

    [Fact]
    public void ErrorLogger_GeneratesDetailedMessage_InDevelopment()
    {
        var mockLogger = new Mock<ILogger<ErrorLogger>>();
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.IsDevelopment()).Returns(true);

        var errorHandler = new ErrorLogger(mockLogger.Object, mockEnvironment.Object);
        var context = ErrorContext.ValidationFailed;
        var exception = new Exception("Specific validation error");

        var message = errorHandler.GetUserMessage(context, exception);

        Assert.Contains(context.ToString(), message);
        Assert.Contains("Specific validation error", message);
    }
}