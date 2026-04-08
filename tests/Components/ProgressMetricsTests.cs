using Bunit;
using Xunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsTests : TestContext
    {
        [Fact]
        public void ProgressMetrics_RendersSuccessfully()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 75,
                BurndownData = new[] { 100, 80, 60, 40, 20 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_DisplaysCompletionPercentage()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 85,
                BurndownData = Array.Empty<int>()
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            var markup = component.Markup;
            Assert.Contains("85", markup);
        }

        [Fact]
        public void ProgressMetrics_RendersBurndownChart()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 50,
                BurndownData = new[] { 100, 75, 50, 25 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            var elements = component.FindAll("[data-burndown]");
            Assert.NotNull(elements);
        }

        [Fact]
        public void ProgressMetrics_HandlesNullBurndownData()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 60,
                BurndownData = null
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_HandlesEmptyBurndownData()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 0,
                BurndownData = Array.Empty<int>()
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }
    }
}