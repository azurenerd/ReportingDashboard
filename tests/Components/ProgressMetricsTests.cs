using Bunit;
using Xunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsTests : TestContext
    {
        [Fact]
        public void ProgressMetrics_DisplaysCompletionPercentage()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 68)
                .Add(p => p.TotalTasks, 19)
                .Add(p => p.CompletedTasks, 17)
                .Add(p => p.BurndownRate, 2.5m));

            Assert.Contains("68%", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_DisplaysTotalTasks()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 68)
                .Add(p => p.TotalTasks, 19)
                .Add(p => p.CompletedTasks, 17)
                .Add(p => p.BurndownRate, 2.5m));

            Assert.Contains("19", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_DisplaysCompletedTasks()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 68)
                .Add(p => p.TotalTasks, 19)
                .Add(p => p.CompletedTasks, 17)
                .Add(p => p.BurndownRate, 2.5m));

            Assert.Contains("17", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_DisplaysBurndownRate()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 68)
                .Add(p => p.TotalTasks, 19)
                .Add(p => p.CompletedTasks, 17)
                .Add(p => p.BurndownRate, 2.50m));

            Assert.Contains("2.50", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_RendersMetricCards()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 68)
                .Add(p => p.TotalTasks, 19)
                .Add(p => p.CompletedTasks, 17)
                .Add(p => p.BurndownRate, 2.5m));

            Assert.Contains("Completion", component.Markup);
            Assert.Contains("Total Tasks", component.Markup);
            Assert.Contains("Completed", component.Markup);
            Assert.Contains("Burndown", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_ResponsiveGrid()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 68)
                .Add(p => p.TotalTasks, 19)
                .Add(p => p.CompletedTasks, 17)
                .Add(p => p.BurndownRate, 2.5m));

            Assert.Contains("col-12", component.Markup);
            Assert.Contains("col-md-6", component.Markup);
            Assert.Contains("col-lg-3", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_MetricsRangeValidation()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 100)
                .Add(p => p.TotalTasks, 100)
                .Add(p => p.CompletedTasks, 100)
                .Add(p => p.BurndownRate, 5.0m));

            Assert.Contains("100%", component.Markup);
            Assert.Contains("100", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_ZeroMetricsDisplayCorrectly()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 0)
                .Add(p => p.TotalTasks, 0)
                .Add(p => p.CompletedTasks, 0)
                .Add(p => p.BurndownRate, 0m));

            Assert.Contains("0%", component.Markup);
            Assert.Contains("0.00", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_NoAnimationsPresent()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 68)
                .Add(p => p.TotalTasks, 19)
                .Add(p => p.CompletedTasks, 17)
                .Add(p => p.BurndownRate, 2.5m));

            Assert.DoesNotContain("animation", component.Markup.ToLower());
            Assert.DoesNotContain("transition", component.Markup.ToLower());
        }
    }
}