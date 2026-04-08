using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsTests : TestContext
    {
        [Fact]
        public void ProgressMetrics_RenderProgressBar()
        {
            var metrics = new ProjectMetrics { Total = 100, Completed = 75 };

            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Metrics, metrics)
            );

            var html = component.Markup;
            Assert.Contains("progress", html);
        }

        [Fact]
        public void ProgressMetrics_DisplaysCorrectPercentage()
        {
            var metrics = new ProjectMetrics { Total = 100, Completed = 75 };

            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Metrics, metrics)
            );

            Assert.Contains("75", component.Markup);
        }
    }
}