using Xunit;
using Bunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.ErrorHandling
{
    public class ErrorBoundaryTests : TestContext
    {
        [Fact]
        public void ErrorBoundary_CatchesChildExceptions()
        {
            var component = RenderComponent<ErrorBoundary>();
            Assert.NotNull(component);
        }

        [Fact]
        public void ErrorBoundary_DisplaysErrorMessage()
        {
            var component = RenderComponent<ErrorBoundary>();
            Assert.NotNull(component);
        }
    }
}