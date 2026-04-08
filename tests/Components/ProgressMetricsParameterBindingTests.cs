using Bunit;
using Xunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsParameterBindingTests : TestContext
    {
        [Fact]
        public async Task ProgressMetrics_UpdatesWhenMetricsParameterChanges()
        {
            var initialMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 25,
                BurndownData = new[] { 100, 75 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, initialMetrics)
            );

            var initialText = component.Markup;
            Assert.Contains("25", initialText);

            var updatedMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 75,
                BurndownData = new[] { 100, 25 }
            };

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, updatedMetrics)
            );

            var updatedText = component.Markup;
            Assert.Contains("75", updatedText);
        }

        [Fact]
        public async Task ProgressMetrics_HandlesNullMetrics()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, null as ProjectMetrics)
            );

            Assert.NotNull(component.Instance);

            var metrics = new ProjectMetrics 
            { 
                CompletionPercentage = 40,
                BurndownData = Array.Empty<int>()
            };

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            var rendered = component.GetComponentInstance<ProgressMetrics>();
            Assert.NotNull(rendered);
        }

        [Fact]
        public async Task ProgressMetrics_HandlesEmptyBurndownData()
        {
            var metrics = new ProjectMetrics 
            { 
                CompletionPercentage = 50,
                BurndownData = Array.Empty<int>()
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            var markup = component.Markup;
            Assert.Contains("50", markup);

            var updatedMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 100,
                BurndownData = new[] { 100, 0 }
            };

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, updatedMetrics)
            );

            var updatedMarkup = component.Markup;
            Assert.Contains("100", updatedMarkup);
        }

        [Fact]
        public async Task ProgressMetrics_HandlesHighCompletionPercentage()
        {
            var metrics = new ProjectMetrics 
            { 
                CompletionPercentage = 99,
                BurndownData = new[] { 100, 1 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            var markup = component.Markup;
            Assert.Contains("99", markup);
        }

        [Fact]
        public async Task ProgressMetrics_HandlesZeroCompletion()
        {
            var metrics = new ProjectMetrics 
            { 
                CompletionPercentage = 0,
                BurndownData = new[] { 100 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }
    }
}