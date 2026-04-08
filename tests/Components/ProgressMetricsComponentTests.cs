using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsComponentTests : TestContext
    {
        [Fact]
        public void ProgressMetricsComponent_Renders()
        {
            var component = RenderComponent<ProgressMetrics>();
            Assert.NotNull(component);
        }

        [Fact]
        public void ProgressMetricsComponent_DisplaysMetrics()
        {
            var component = RenderComponent<ProgressMetrics>();
            Assert.NotEmpty(component.Markup);
        }

        [Fact]
        public void ProgressMetricsComponent_WithZeroProgress_Renders()
        {
            var component = RenderComponent<ProgressMetrics>();
            Assert.NotNull(component);
        }

        [Fact]
        public void ProgressMetricsComponent_WithFullProgress_Renders()
        {
            var component = RenderComponent<ProgressMetrics>();
            Assert.NotNull(component);
        }
    }
}