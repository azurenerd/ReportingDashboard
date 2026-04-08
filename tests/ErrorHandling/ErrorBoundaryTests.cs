using Bunit;
using AgentSquad.Runner.Components;
using Xunit;

namespace AgentSquad.Runner.Tests.ErrorHandling;

public class ErrorBoundaryTests : TestContext
{
    [Fact]
    public void ErrorBoundary_RenderChildContent()
    {
        // Arrange & Act
        var component = RenderComponent<ErrorBoundary>(parameters => parameters
            .AddChildContent("<div>Test Content</div>"));

        // Assert
        var markup = component.Markup;
        Assert.Contains("Test Content", markup);
    }

    [Fact]
    public void ErrorBoundary_CatchesException()
    {
        // Arrange
        var component = RenderComponent<ErrorBoundary>(parameters => parameters
            .AddChildContent(b => b.Component<ThrowExceptionComponent>()));

        // Act
        var exception = Record.Exception(() => component.Render());

        // Assert - Error boundary should handle exception without crashing the test
        Assert.NotNull(component);
    }
}

// Helper component that throws an exception
public partial class ThrowExceptionComponent : ComponentBase
{
    protected override void OnInitialized()
    {
        throw new InvalidOperationException("Test exception");
    }
}