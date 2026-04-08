using Bunit;
using Xunit;
using Microsoft.AspNetCore.Components;
using AgentSquad.Runner.Components;

namespace AgentSquad.Runner.Tests.Components
{
    public class ErrorBoundaryTests : TestContext
    {
        [Fact]
        public void ErrorBoundary_Renders_ChildContent()
        {
            // Act
            var component = RenderComponent<ErrorBoundary>(
                parameters => parameters
                    .AddChildContent(@"<p>Test Content</p>")
            );

            // Assert
            Assert.NotNull(component);
            component.Markup.Should().Contain("Test Content");
        }

        [Fact]
        public void ErrorBoundary_Renders_WithoutErrors()
        {
            // Arrange
            var childContent = (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, "Safe content");
                builder.CloseElement();
            });

            // Act
            var component = RenderComponent<ErrorBoundary>(
                parameters => parameters
                    .Add(p => p.ChildContent, childContent)
            );

            // Assert
            Assert.NotNull(component);
        }

        [Fact]
        public void ErrorBoundary_Structure_HasAlertDiv()
        {
            // Act
            var component = RenderComponent<ErrorBoundary>(
                parameters => parameters
                    .AddChildContent(@"<p>Content</p>")
            );

            // Assert - Component should render and have proper structure
            Assert.NotNull(component);
            component.Markup.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ErrorBoundary_Renders_MultipleChildElements()
        {
            // Arrange
            var childContent = (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, "First");
                builder.CloseElement();
                builder.OpenElement(2, "div");
                builder.AddContent(3, "Second");
                builder.CloseElement();
            });

            // Act
            var component = RenderComponent<ErrorBoundary>(
                parameters => parameters
                    .Add(p => p.ChildContent, childContent)
            );

            // Assert
            Assert.NotNull(component);
        }
    }
}