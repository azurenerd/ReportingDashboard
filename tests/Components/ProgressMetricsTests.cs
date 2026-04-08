using Xunit;
using Bunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsTests : TestContext
    {
        [Fact]
        public void ProgressMetrics_WithValidMetrics_RendersPercentage()
        {
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Completed, 5)
                          .Add(p => p.Total, 10));

            Assert.Contains("50", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithZeroTotal_RendersZeroPercent()
        {
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Completed, 0)
                          .Add(p => p.Total, 0));

            Assert.Contains("0", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithCompleteProgress_RendersHundredPercent()
        {
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Completed, 10)
                          .Add(p => p.Total, 10));

            Assert.Contains("100", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_CalculatesPercentageCorrectly()
        {
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Completed, 3)
                          .Add(p => p.Total, 4));

            Assert.Contains("75", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithPartialProgress_RendersCorrectValue()
        {
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Completed, 7)
                          .Add(p => p.Total, 20));

            Assert.Contains("35", component.Markup);
        }
    }
}