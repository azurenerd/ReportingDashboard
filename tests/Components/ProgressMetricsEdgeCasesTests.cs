using Bunit;
using Xunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsEdgeCasesTests : TestContext
    {
        [Fact]
        public void ProgressMetrics_HandlesMaxIntPercentage()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 100,
                BurndownData = new[] { 0 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_HandlesSingleDataPoint()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 50,
                BurndownData = new[] { 100 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }

        [Fact]
        public void ProgressMetrics_HandlesConsecutiveNullParameters()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, null as ProjectMetrics)
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_HandlesNegativeCompletionValue()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = -10,
                BurndownData = new[] { 100, 110 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_HandlesBurndownWithIncreasingValues()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 50,
                BurndownData = new[] { 10, 20, 30, 40, 50 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_HandlesZeroBurndownValues()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 0,
                BurndownData = new[] { 0, 0, 0, 0, 0 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_HandlesVeryLargeCompletionValue()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 999,
                BurndownData = new[] { 1000 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            Assert.NotNull(component.Instance);
        }
    }
}