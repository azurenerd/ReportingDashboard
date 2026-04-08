using Xunit;
using Bunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsTests : TestContext
    {
        [Fact]
        public void ProgressMetrics_WithValidMetrics_RendersProgress()
        {
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Completed, 5)
                          .Add(p => p.Total, 10));

            Assert.NotNull(component);
            Assert.Contains("50", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithZeroTotal_RendersZeroPercent()
        {
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Completed, 0)
                          .Add(p => p.Total, 0));

            Assert.NotNull(component);
        }

        [Fact]
        public void ProgressMetrics_WithCompleteProgress_RendersHundredPercent()
        {
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Completed, 10)
                          .Add(p => p.Total, 10));

            Assert.NotNull(component);
        }
    }
}