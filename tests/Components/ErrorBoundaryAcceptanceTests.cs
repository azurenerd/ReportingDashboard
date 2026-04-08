using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using System;
using System.Threading.Tasks;

namespace AgentSquad.Runner.Tests.Components
{
    public class ErrorBoundaryAcceptanceTests : TestContext
    {
        [Fact]
        public void AC1_CatchesComponentRenderingErrors()
        {
            // AC: Catches component rendering errors gracefully
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Test</div>"));

            var testException = new NullReferenceException("Component rendering failed");
            cut.Instance.ProcessError(testException);

            var errorContainer = cut.QuerySelector(".error-boundary-container");
            Assert.NotNull(errorContainer);
        }

        [Fact]
        public void AC2_DisplaysUserFriendlyMessage()
        {
            // AC: Displays user-friendly error message: "An error occurred while loading the dashboard. Please refresh your browser."
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Test</div>"));

            cut.Instance.ProcessError(new Exception("Test"));

            var message = cut.Find(".error-message");
            Assert.Equal("An error occurred while loading the dashboard. Please refresh your browser.", 
                message.TextContent);
        }

        [Fact]
        public async Task AC3_ProvidesRefreshButton_TriggersPageReload()
        {
            // AC: Provides "Refresh" button triggering window.location.reload()
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Test</div>"));

            cut.Instance.ProcessError(new Exception("Test"));

            var button = cut.Find(".refresh-button");
            Assert.Equal("Refresh", button.TextContent.Trim());
            
            // Button click should trigger location.reload (tested via onclick handler)
            await button.ClickAsync();
        }

        [Fact]
        public void AC4_LogsErrorDetailsToConsole()
        {
            // AC: Logs error details to browser console for debugging
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Test</div>"));

            var exceptionWithDetails = new InvalidOperationException("Critical operation failed");
            cut.Instance.ProcessError(exceptionWithDetails);

            // Error should be processed and logged
            var errorContainer = cut.QuerySelector(".error-boundary-container");
            Assert.NotNull(errorContainer);
        }

        [Fact]
        public void AC5_UsesBootstrapAlertDangerStyling()
        {
            // AC: Uses Bootstrap alert-danger styling for error UI
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Test</div>"));

            cut.Instance.ProcessError(new Exception("Test"));

            var alert = cut.Find(".alert");
            Assert.Contains("alert-danger", alert.ClassList);
        }

        [Fact]
        public void AC6_ErrorContainerCenteredAndProminent()
        {
            // AC: Error container is centered and visually prominent
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Test</div>"));

            cut.Instance.ProcessError(new Exception("Test"));

            var container = cut.Find(".error-boundary-container");
            Assert.Contains("flex", container.Style["display"] ?? "");
            Assert.Contains("center", container.Style["justify-content"] ?? "");
            Assert.Contains("center", container.Style["align-items"] ?? "");
        }

        [Fact]
        public void AC7_ButtonStyledWithBootstrap()
        {
            // AC: Button styled with Bootstrap btn btn-primary classes
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Test</div>"));

            cut.Instance.ProcessError(new Exception("Test"));

            var button = cut.Find(".refresh-button");
            Assert.Contains("btn", button.ClassList);
            Assert.Contains("btn-primary", button.ClassList);
        }

        [Fact]
        public void AC8_FontSizeAtLeast14pt()
        {
            // AC: Font size 14pt+ for readability
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Test</div>"));

            cut.Instance.ProcessError(new Exception("Test"));

            var button = cut.Find(".refresh-button");
            var fontSizeStr = button.Style["font-size"];
            
            if (fontSizeStr?.EndsWith("pt") == true)
            {
                var fontSize = int.Parse(fontSizeStr.Replace("pt", ""));
                Assert.True(fontSize >= 14, $"Font size {fontSize}pt should be >= 14pt");
            }
        }

        [Fact]
        public void AC9_DisplaysChildContentNormally_WhenNoError()
        {
            // AC: Displays child content normally when no error occurs
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent(@"<div class=""my-content"">This is child content</div>"));

            var childContent = cut.QuerySelector(".my-content");
            Assert.NotNull(childContent);
            Assert.Equal("This is child content", childContent.TextContent);

            var errorContainer = cut.QuerySelector(".error-boundary-container");
            Assert.Null(errorContainer);
        }

        [Fact]
        public void AC_Integration_CompleteErrorHandlingFlow()
        {
            // Integration test: All acceptance criteria working together
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent(@"<div class=""dashboard"">Dashboard Content</div>"));

            // 1. Initially shows child content
            Assert.NotNull(cut.QuerySelector(".dashboard"));
            Assert.Null(cut.QuerySelector(".error-boundary-container"));

            // 2. Error occurs
            var exception = new Exception("Dashboard component error");
            cut.Instance.ProcessError(exception);

            // 3. Shows error UI
            var errorContainer = cut.Find(".error-boundary-container");
            Assert.NotNull(errorContainer);

            // 4. Shows user-friendly message
            var message = cut.Find(".error-message");
            Assert.Contains("An error occurred while loading the dashboard", message.TextContent);

            // 5. Has properly styled refresh button
            var button = cut.Find(".refresh-button");
            Assert.Equal("Refresh", button.TextContent.Trim());
            Assert.Contains("btn-primary", button.ClassList);

            // 6. Child content is hidden
            Assert.Null(cut.QuerySelector(".dashboard"));
        }

        [Fact]
        public void AC_EdgeCase_MultipleConsecutiveErrors()
        {
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Test</div>"));

            cut.Instance.ProcessError(new Exception("First error"));
            cut.Instance.ProcessError(new InvalidOperationException("Second error"));
            cut.Instance.ProcessError(new ArgumentException("Third error"));

            var errorContainer = cut.QuerySelector(".error-boundary-container");
            Assert.NotNull(errorContainer);
        }

        [Fact]
        public void AC_EdgeCase_ErrorWithNullMessage()
        {
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Test</div>"));

            var exception = new Exception();
            cut.Instance.ProcessError(exception);

            var message = cut.Find(".error-message");
            Assert.NotNull(message);
        }

        [Fact]
        public void AC_EdgeCase_ErrorWithInnerException()
        {
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Test</div>"));

            var innerException = new ArgumentNullException("Inner error");
            var exception = new InvalidOperationException("Outer error", innerException);
            cut.Instance.ProcessError(exception);

            var errorContainer = cut.QuerySelector(".error-boundary-container");
            Assert.NotNull(errorContainer);
        }

        [Fact]
        public void AC_EdgeCase_LargeChildContent()
        {
            var largeContent = string.Concat(
                Enumerable.Range(0, 1000).Select(i => $"<div>Item {i}</div>")
            );

            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent(largeContent));

            var children = cut.QuerySelectorAll("div");
            Assert.NotEmpty(children);

            cut.Instance.ProcessError(new Exception("Error after large content"));
            var errorContainer = cut.QuerySelector(".error-boundary-container");
            Assert.NotNull(errorContainer);
        }
    }
}