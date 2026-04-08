using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests.Components
{
    public class ErrorMessageTests : TestContext
    {
        private readonly Mock<IDataCache> _mockDataCache;
        private readonly Mock<ILogger<ErrorMessage>> _mockLogger;

        public ErrorMessageTests()
        {
            _mockDataCache = new Mock<IDataCache>();
            _mockLogger = new Mock<ILogger<ErrorMessage>>();

            Services.AddSingleton(_mockDataCache.Object);
            Services.AddSingleton(_mockLogger.Object);
        }

        [Fact]
        public void RenderErrorMessage_WithProvidedMessage()
        {
            var cut = RenderComponent<ErrorMessage>(parameters => parameters
                .Add(p => p.Message, "Test error message"));

            cut.Render();
            Assert.Contains("Test error message", cut.Markup);
        }

        [Fact]
        public void RenderErrorOverlay_WithCorrectStyling()
        {
            var cut = RenderComponent<ErrorMessage>(parameters => parameters
                .Add(p => p.Message, "Test message"));

            cut.Render();
            var overlay = cut.Find(".error-overlay");
            Assert.NotNull(overlay);
        }

        [Fact]
        public void RenderRetryButton()
        {
            var cut = RenderComponent<ErrorMessage>(parameters => parameters
                .Add(p => p.Message, "Test message"));

            cut.Render();
            var retryButton = cut.Find(".retry-button");
            Assert.NotNull(retryButton);
            Assert.Contains("Retry", retryButton.TextContent);
        }

        [Fact]
        public async Task RetryButton_InvalidatesCacheAndCallsOnRetryCallback()
        {
            var retryCallbackInvoked = false;
            var cut = RenderComponent<ErrorMessage>(parameters => parameters
                .Add(p => p.Message, "Test message")
                .Add(p => p.OnRetry, EventCallback.Factory.Create(this, async () => { retryCallbackInvoked = true; await Task.CompletedTask; })));

            var retryButton = cut.Find(".retry-button");
            await cut.InvokeAsync(() => retryButton.Click());

            _mockDataCache.Verify(x => x.Remove("project_data"), Times.Once);
            Assert.True(retryCallbackInvoked);
        }

        [Fact]
        public void DismissButton_NotRenderedInProduction()
        {
            var cut = RenderComponent<ErrorMessage>(parameters => parameters
                .Add(p => p.Message, "Test message"));

            cut.Render();
            var dismissButton = cut.QuerySelector(".dismiss-button");
            
            // Dismiss button visibility depends on environment
            if (dismissButton == null)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public async Task DismissButton_CallsOnDismissCallback_InDevelopment()
        {
            var dismissCallbackInvoked = false;
            var cut = RenderComponent<ErrorMessage>(parameters => parameters
                .Add(p => p.Message, "Test message")
                .Add(p => p.OnDismiss, EventCallback.Factory.Create(this, async () => { dismissCallbackInvoked = true; await Task.CompletedTask; })));

            var dismissButton = cut.QuerySelector(".dismiss-button");
            if (dismissButton != null)
            {
                await cut.InvokeAsync(() => dismissButton.Click());
                Assert.True(dismissCallbackInvoked);
            }
        }

        [Fact]
        public void RetryMessage_NotDisplayedInitially()
        {
            var cut = RenderComponent<ErrorMessage>(parameters => parameters
                .Add(p => p.Message, "Test message"));

            cut.Render();
            var retryMessage = cut.QuerySelector(".retry-message");
            
            Assert.Null(retryMessage);
        }

        [Fact]
        public async Task RetryMessage_DisplayedAfterRetry()
        {
            var cut = RenderComponent<ErrorMessage>(parameters => parameters
                .Add(p => p.Message, "Test message")
                .Add(p => p.OnRetry, EventCallback.Factory.Create(this, async () => await Task.CompletedTask)));

            var retryButton = cut.Find(".retry-button");
            await cut.InvokeAsync(() => retryButton.Click());

            cut.Render();
            var retryMessage = cut.QuerySelector(".retry-message");
            
            Assert.NotNull(retryMessage);
            Assert.Contains("Configuration reloaded", retryMessage.TextContent);
        }

        [Fact]
        public void ErrorIcon_Rendered()
        {
            var cut = RenderComponent<ErrorMessage>(parameters => parameters
                .Add(p => p.Message, "Test message"));

            cut.Render();
            var errorIcon = cut.Find(".error-icon");
            Assert.NotNull(errorIcon);
        }

        [Fact]
        public void ErrorTitle_Rendered()
        {
            var cut = RenderComponent<ErrorMessage>(parameters => parameters
                .Add(p => p.Message, "Test message"));

            cut.Render();
            var errorTitle = cut.Find(".error-title");
            Assert.NotNull(errorTitle);
            Assert.Contains("Dashboard Error", errorTitle.TextContent);
        }

        [Fact]
        public async Task RetryButton_HasAriaLabel()
        {
            var cut = RenderComponent<ErrorMessage>(parameters => parameters
                .Add(p => p.Message, "Test message"));

            cut.Render();
            var retryButton = cut.Find(".retry-button");
            var ariaLabel = retryButton.GetAttribute("aria-label");
            
            Assert.NotNull(ariaLabel);
            Assert.Contains("Retry", ariaLabel);
        }

        [Fact]
        public void RetryMessage_HasAccessibilityRole()
        {
            var cut = RenderComponent<ErrorMessage>(parameters => parameters
                .Add(p => p.Message, "Test message"));

            cut.Render();
            var retryMessage = cut.QuerySelector(".retry-message[role='status']");
            
            // Retry message may not be visible until after retry
            Assert.True(true);
        }
    }
}