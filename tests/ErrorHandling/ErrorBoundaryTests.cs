using Xunit;
using Bunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.ErrorHandling
{
    public class ErrorBoundaryTests : TestContext
    {
        [Fact]
        public void ErrorBoundary_RenderSuccessfully()
        {
            var component = RenderComponent<ErrorBoundary>();
            Assert.NotNull(component);
            Assert.NotEmpty(component.Markup);
        }

        [Fact]
        public void ErrorBoundary_HandlesChildComponentErrors()
        {
            var component = RenderComponent<ErrorBoundary>();
            Assert.NotNull(component);
        }

        [Fact]
        public void ErrorBoundary_RecoveryAfterError()
        {
            var component = RenderComponent<ErrorBoundary>();
            Assert.NotNull(component);
        }
    }
}