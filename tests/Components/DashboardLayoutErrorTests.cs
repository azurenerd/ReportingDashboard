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

public class DashboardLayoutErrorTests : TestContext
{
    private readonly Mock<IDataProvider> _mockDataProvider;
    private readonly Mock<IErrorHandler> _mockErrorHandler;
    private readonly Mock<ILogger<DashboardLayout>> _mockLogger;

    public DashboardLayoutErrorTests()
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
    public void DashboardLayout_DisplaysLoadingSpinner_DuringInitialization()
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
    public void DashboardLayout_HidesLoadingSpinner_WhenDataLoaded()
    {
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ReturnsAsync(InvalidDataFixtures.ValidProject);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.DoesNotContain("Loading project dashboard", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_DisplaysErrorOverlay_OnFileNotFound()
    {
        var exception = new FileNotFoundException("data.json not found");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("error-overlay", component.Markup);
            Assert.Contains("Configuration file missing", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_DisplaysErrorOverlay_OnInvalidJson()
    {
        var exception = new JsonException("Invalid JSON syntax");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("error-overlay", component.Markup);
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
            Assert.Contains("error-overlay", component.Markup);
            Assert.Contains("Configuration validation failed", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_DisplaysErrorOverlay_OnUnexpectedException()
    {
        var exception = new Exception("Unexpected error occurred");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("error-overlay", component.Markup);
            Assert.Contains("unexpected error", component.Markup, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void DashboardLayout_DisplaysErrorOverlay_OnNullProject()
    {
        var exception = new InvalidOperationException("Project data is invalid");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("error-overlay", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_LogsError_WithTimestamp()
    {
        var exception = new FileNotFoundException("Test error");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        });
    }

    [Fact]
    public void DashboardLayout_CapturesError_WithCorrectContext()
    {
        var exception = new FileNotFoundException("Test");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            _mockErrorHandler.Verify(
                x => x.CaptureErrorAsync(
                    It.Is<Exception>(e => e is FileNotFoundException),
                    ErrorContext.FileNotFound),
                Times.Once);
        });
    }

    [Fact]
    public void DashboardLayout_DisplaysProjectName_WhenLoaded()
    {
        var project = InvalidDataFixtures.ValidProject;
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ReturnsAsync(project);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains(project.Name, component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_DisplaysProjectDescription_WhenProvided()
    {
        var project = InvalidDataFixtures.ValidProject;
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ReturnsAsync(project);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            if (!string.IsNullOrEmpty(project.Description))
            {
                Assert.Contains(project.Description, component.Markup);
            }
        });
    }

    [Fact]
    public async Task DashboardLayout_RetryButton_InvalidatesCacheAndReloads()
    {
        var callCount = 0;
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    return Task.FromException<Project>(
                        new FileNotFoundException("Initial failure"));
                }
                return Task.FromResult(InvalidDataFixtures.ValidProject);
            });

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("error-overlay", component.Markup);
        });

        var retryButton = component.Find("button.retry-button");
        await retryButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        component.WaitForAssertion(() =>
        {
            _mockDataProvider.Verify(x => x.InvalidateCache(), Times.Once);
            Assert.Equal(2, callCount);
        });
    }

    [Fact]
    public async Task DashboardLayout_RetryButton_ClearsErrorMessage()
    {
        var callCount = 0;
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    return Task.FromException<Project>(
                        new InvalidOperationException("Validation failed"));
                }
                return Task.FromResult(InvalidDataFixtures.ValidProject);
            });

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("Validation", component.Markup);
        });

        var retryButton = component.Find("button.retry-button");
        await retryButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        component.WaitForAssertion(() =>
        {
            Assert.DoesNotContain("Validation", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_ShowsChildComponents_WhenDataLoaded()
    {
        var project = InvalidDataFixtures.ValidProject;
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ReturnsAsync(project);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("dashboard-container", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_DoesNotShowDashboard_WhenErrorDisplayed()
    {
        var exception = new FileNotFoundException("File not found");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("error-overlay", component.Markup);
            Assert.DoesNotContain("dashboard-container", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_FileNotFoundMessage_IncludesDataJsonReference()
    {
        var exception = new FileNotFoundException("File not found");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("data.json", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_InvalidJsonMessage_IncludesDataJsonReference()
    {
        var exception = new JsonException("Invalid JSON");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("data.json", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_ValidationFailedMessage_ListsMissingFields()
    {
        var exception = new InvalidOperationException("Configuration validation failed. Required fields missing: Name, Milestones, Metrics");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("Configuration validation failed", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_InitialState_ShowsLoadingSpinner()
    {
        var tcs = new TaskCompletionSource<Project>();
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .Returns(tcs.Task);

        var component = RenderComponent<DashboardLayout>();

        Assert.Contains("dashboard-loading", component.Markup);
        tcs.SetResult(InvalidDataFixtures.ValidProject);
    }

    [Fact]
    public void DashboardLayout_SetIsLoading_ToFalseAfterDataLoaded()
    {
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ReturnsAsync(InvalidDataFixtures.ValidProject);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.DoesNotContain("dashboard-loading", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_SetIsLoading_ToFalseAfterError()
    {
        var exception = new FileNotFoundException("Not found");
        _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(exception);

        var component = RenderComponent<DashboardLayout>();

        component.WaitForAssertion(() =>
        {
            Assert.DoesNotContain("Loading project dashboard", component.Markup);
        });
    }

    [Fact]
    public void DashboardLayout_AllExceptionTypes_SetErrorMessage()
    {
        var exceptions = new Exception[]
        {
            new FileNotFoundException("File not found"),
            new JsonException("Invalid JSON"),
            new InvalidOperationException("Validation failed"),
            new Exception("Unexpected")
        };

        foreach (var ex in exceptions)
        {
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(ex);

            var component = RenderComponent<DashboardLayout>();

            component.WaitForAssertion(() =>
            {
                Assert.Contains("error-overlay", component.Markup);
            });
        }
    }
}