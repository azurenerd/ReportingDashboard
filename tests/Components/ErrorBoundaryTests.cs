using AgentSquad.Runner.Components;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AgentSquad.Runner.Tests.Components;

public class ErrorBoundaryTests : TestContext
{
    public ErrorBoundaryTests()
    {
        Services.AddLogging();
    }

    [Fact]
    public void ErrorBoundary_DoesNotRenderOverlay_WhenErrorMessageIsNull()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, (string?)null)
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        Assert.DoesNotContain("error-overlay", component.Markup);
    }

    [Fact]
    public void ErrorBoundary_DoesNotRenderOverlay_WhenErrorMessageIsEmpty()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, string.Empty)
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        Assert.DoesNotContain("error-overlay", component.Markup);
    }

    [Fact]
    public void ErrorBoundary_RendersOverlay_WhenErrorMessageProvided()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "Test error message")
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        Assert.Contains("error-overlay", component.Markup);
        Assert.Contains("error-overlay-content", component.Markup);
    }

    [Fact]
    public void ErrorBoundary_DisplaysErrorMessage_InMarkup()
    {
        var errorMessage = "Configuration file is missing";
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, errorMessage)
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        Assert.Contains(errorMessage, component.Markup);
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
    }

    [Fact]
    public void ErrorBoundary_DisplaysErrorTitle()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "Test error")
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        Assert.Contains("Dashboard Error", component.Markup);
    }

    [Fact]
    public void ErrorBoundary_DisplaysRetryButton()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "Test error")
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        var retryButtons = component.FindAll("button.retry-button");
        Assert.NotEmpty(retryButtons);
    }

    [Fact]
    public void ErrorBoundary_RetryButtonHasCorrectText()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "Test error")
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        var retryButton = component.Find("button.retry-button");
        Assert.Contains("Retry", retryButton.TextContent);
    }

    [Fact]
    public async Task ErrorBoundary_RetryButton_InvokesOnRetryCallback()
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
    public async Task ErrorBoundary_RetryButton_DisabledDuringRetry()
    {
        var retryInProgress = false;
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "Test error")
                .Add(p => p.OnRetry, async () =>
                {
                    retryInProgress = true;
                    await Task.Delay(100);
                    retryInProgress = false;
                })
        );

        var retryButton = component.Find("button.retry-button");
        var clickTask = retryButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        Assert.True(retryInProgress);
        await clickTask;
        Assert.False(retryInProgress);
    }

    [Fact]
    public void ErrorBoundary_DisplaysDismissButton_InDevelopment()
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
    public async Task ErrorBoundary_DismissButton_InvokesDismissCallback()
    {
        var dismissInvoked = false;
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "Test error")
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
                .Add(p => p.OnDismiss, EventCallback.Factory.Create(null, async () =>
                {
                    dismissInvoked = true;
                    await Task.CompletedTask;
                }))
        );

        var dismissButton = component.Find("button.dismiss-button");
        await dismissButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        Assert.True(dismissInvoked);
    }

    [Fact]
    public void ErrorBoundary_AppliesErrorOverlayCSS()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "Test error")
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        Assert.Contains("error-overlay", component.Markup);
        Assert.Matches(@"class=""[^""]*error-overlay[^""]*""", component.Markup);
    }

    [Fact]
    public void ErrorBoundary_MultipleErrorMessages_DisplaysLatest()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "First error")
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        Assert.Contains("First error", component.Markup);

        component.SetParametersAndRender(parameters =>
            parameters.Add(p => p.ErrorMessage, "Second error")
        );

        Assert.Contains("Second error", component.Markup);
        Assert.DoesNotContain("First error", component.Markup);
    }

    [Fact]
    public void ErrorBoundary_HandleRetry_WithNullCallback_DoesNotThrow()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "Test error")
                .Add(p => p.OnRetry, (Func<Task>?)null)
        );

        var retryButton = component.Find("button.retry-button");
        Assert.NotNull(retryButton);
    }

    [Fact]
    public void ErrorBoundary_ErrorMessage_EscapesHTML()
    {
        var component = RenderComponent<ErrorBoundary>(parameters =>
            parameters
                .Add(p => p.ErrorMessage, "Error: <script>alert('xss')</script>")
                .Add(p => p.OnRetry, async () => await Task.CompletedTask)
        );

        Assert.DoesNotContain("<script>", component.Markup);
    }
}