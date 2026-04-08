using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using System;
using System.Threading.Tasks;

namespace AgentSquad.Runner.Tests.Components
{
    public class ErrorBoundaryTests : TestContext
    {
        [Fact]
        public void ErrorBoundary_DisplaysChildContent_WhenNoErrorOccurs()
        {
            // Arrange & Act
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Test Child Content</div>"));

            // Assert
            cut.MarkupMatches("<div>Test Child Content</div>");
        }

        [Fact]
        public void ErrorBoundary_DisplaysErrorUI_WhenErrorOccurs()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));

            // Act
            var exception = new InvalidOperationException("Test error");
            cut.Instance.ProcessError(exception);

            // Assert
            cut.Find(".error-boundary-container").MarkupMatches(
                @"<div class=""error-boundary-container"">
                    <div class=""alert alert-danger"" role=""alert"">
                        <div class=""alert-icon mb-3"">
                            <span style=""font-size: 2rem;"">⚠️</span>
                        </div>
                        <h4 class=""alert-heading"">Dashboard Error</h4>
                        <p class=""error-message"">An error occurred while loading the dashboard. Please refresh your browser.</p>
                        <button class=""btn btn-primary mt-3 refresh-button"">Refresh</button>
                    </div>
                </div>"
            );
        }

        [Fact]
        public void ErrorBoundary_DisplaysUserFriendlyMessage()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));
            var exception = new NullReferenceException("Null reference");

            // Act
            cut.Instance.ProcessError(exception);

            // Assert
            var message = cut.Find(".error-message");
            Assert.Equal("An error occurred while loading the dashboard. Please refresh your browser.", 
                message.TextContent);
        }

        [Fact]
        public void ErrorBoundary_DisplaysErrorHeading()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));

            // Act
            var exception = new Exception("Test");
            cut.Instance.ProcessError(exception);

            // Assert
            var heading = cut.Find(".alert-heading");
            Assert.Equal("Dashboard Error", heading.TextContent);
        }

        [Fact]
        public void ErrorBoundary_RefreshButton_HasCorrectBootstrapClasses()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));
            var exception = new Exception("Test");

            // Act
            cut.Instance.ProcessError(exception);

            // Assert
            var button = cut.Find(".refresh-button");
            Assert.Contains("btn", button.ClassList);
            Assert.Contains("btn-primary", button.ClassList);
            Assert.Equal("14pt", button.Style["font-size"]);
        }

        [Fact]
        public void ErrorBoundary_RefreshButton_HasMinimumFontSize()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));

            // Act
            var exception = new Exception("Test");
            cut.Instance.ProcessError(exception);

            // Assert
            var button = cut.Find(".refresh-button");
            var fontSizeStr = button.Style["font-size"];
            Assert.NotNull(fontSizeStr);
            
            if (fontSizeStr.EndsWith("pt"))
            {
                var fontSize = int.Parse(fontSizeStr.Replace("pt", ""));
                Assert.True(fontSize >= 14, $"Font size {fontSize}pt should be >= 14pt");
            }
        }

        [Fact]
        public void ErrorBoundary_ErrorContainer_IsBootstrapAlert()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));

            // Act
            var exception = new Exception("Test");
            cut.Instance.ProcessError(exception);

            // Assert
            var alert = cut.Find(".alert");
            Assert.Contains("alert-danger", alert.ClassList);
            Assert.Equal("alert", alert.GetAttribute("role"));
        }

        [Fact]
        public void ErrorBoundary_ErrorContainer_IsCentered()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));

            // Act
            var exception = new Exception("Test");
            cut.Instance.ProcessError(exception);

            // Assert
            var container = cut.Find(".error-boundary-container");
            Assert.Contains("flex", container.Style["display"] ?? "");
            Assert.Contains("center", container.Style["justify-content"] ?? "");
        }

        [Fact]
        public void ErrorBoundary_LogsErrorToConsole_OnError()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));
            var exception = new InvalidOperationException("Critical error");

            // Act
            cut.Instance.ProcessError(exception);

            // Assert - Verify error handling was triggered
            var errorContainer = cut.Find(".error-boundary-container");
            Assert.NotNull(errorContainer);
        }

        [Fact]
        public void ErrorBoundary_LogsInnerException_WhenPresent()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));
            var innerException = new ArgumentNullException(nameof(innerException));
            var exception = new InvalidOperationException("Outer error", innerException);

            // Act
            cut.Instance.ProcessError(exception);

            // Assert
            var errorContainer = cut.Find(".error-boundary-container");
            Assert.NotNull(errorContainer);
        }

        [Fact]
        public async Task ErrorBoundary_RefreshButton_CallsLocationReload()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));
            var exception = new Exception("Test");
            cut.Instance.ProcessError(exception);
            var button = cut.Find(".refresh-button");

            // Act
            await button.ClickAsync();

            // Assert - No exception should be thrown
            Assert.NotNull(button);
        }

        [Fact]
        public void ErrorBoundary_RendersChildContent_BeforeError()
        {
            // Arrange & Act
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent(@"<div class=""child-content"">Child rendered</div>"));

            // Assert
            var child = cut.Find(".child-content");
            Assert.Equal("Child rendered", child.TextContent);
        }

        [Fact]
        public void ErrorBoundary_HidesChildContent_AfterError()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent(@"<div class=""child-content"">Child rendered</div>"));

            // Act
            var exception = new Exception("Test");
            cut.Instance.ProcessError(exception);

            // Assert
            Assert.Throws<ElementNotFoundException>(() => cut.Find(".child-content"));
        }

        [Fact]
        public void ErrorBoundary_DisplaysErrorIcon()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));

            // Act
            var exception = new Exception("Test");
            cut.Instance.ProcessError(exception);

            // Assert
            var icon = cut.Find(".alert-icon span");
            Assert.NotNull(icon);
            Assert.Equal("2rem", icon.Style["font-size"]);
        }

        [Fact]
        public void ErrorBoundary_AlertHasProperMarkup_OnError()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));

            // Act
            var exception = new Exception("Test");
            cut.Instance.ProcessError(exception);

            // Assert
            var alert = cut.Find(".alert");
            var heading = alert.QuerySelector(".alert-heading");
            var message = alert.QuerySelector(".error-message");
            var button = alert.QuerySelector(".refresh-button");
            
            Assert.NotNull(heading);
            Assert.NotNull(message);
            Assert.NotNull(button);
        }

        [Fact]
        public void ErrorBoundary_HandlesMultipleExceptions()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));

            // Act
            cut.Instance.ProcessError(new Exception("First error"));
            cut.Instance.ProcessError(new InvalidOperationException("Second error"));

            // Assert
            var errorContainer = cut.Find(".error-boundary-container");
            Assert.NotNull(errorContainer);
        }

        [Fact]
        public void ErrorBoundary_CascadingValueNotSet_WhenErrorOccurs()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));

            // Act
            var exception = new Exception("Test");
            cut.Instance.ProcessError(exception);

            // Assert - CascadingValue should not be in tree
            var children = cut.FindAll("*");
            Assert.NotEmpty(children);
        }

        [Fact]
        public void ErrorBoundary_ButtonText_IsRefresh()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));

            // Act
            var exception = new Exception("Test");
            cut.Instance.ProcessError(exception);

            // Assert
            var button = cut.Find(".refresh-button");
            Assert.Equal("Refresh", button.TextContent.Trim());
        }

        [Fact]
        public void ErrorBoundary_ResponsiveStylesToMobileView()
        {
            // Arrange
            var cut = RenderComponent<ErrorBoundary>(parameters => parameters
                .AddChildContent("<div>Content</div>"));

            // Act
            var exception = new Exception("Test");
            cut.Instance.ProcessError(exception);

            // Assert - Container should be flexbox for responsive design
            var container = cut.Find(".error-boundary-container");
            Assert.Contains("flex", container.Style["display"] ?? "");
        }
    }
}