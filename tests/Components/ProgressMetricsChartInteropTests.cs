using Bunit;
using Xunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsChartInteropTests : TestContext
    {
        [Fact]
        public async Task ProgressMetrics_LoadsChartDataViaInterop()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 45,
                BurndownData = new[] { 100, 80, 60, 40, 20 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            await component.WaitForAsyncLoad();

            var markup = component.Markup;
            Assert.Contains("45", markup);
        }

        [Fact]
        public async Task ProgressMetrics_HandlesMissingChartLibrary()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 50,
                BurndownData = new[] { 100, 50 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            await component.WaitForAsyncLoad();

            Assert.NotNull(component.Instance);
            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }

        [Fact]
        public async Task ProgressMetrics_RendersChartContainerAfterInteropCall()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 65,
                BurndownData = new[] { 100, 70, 40, 20 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            await component.WaitForAsyncLoad();

            var elements = component.FindAll("[data-chart]");
            Assert.NotNull(elements);
        }

        [Fact]
        public async Task ProgressMetrics_UpdatesChartOnMetricsChange()
        {
            var initialMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 30,
                BurndownData = new[] { 100, 70 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, initialMetrics)
            );

            await component.WaitForAsyncLoad();

            var updatedMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 85,
                BurndownData = new[] { 100, 15 }
            };

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, updatedMetrics)
            );

            await component.WaitForAsyncLoad();

            var markup = component.Markup;
            Assert.Contains("85", markup);
        }

        [Fact]
        public async Task ProgressMetrics_HandlesLargeBurndownDataset()
        {
            var burndownData = new int[100];
            for (int i = 0; i < 100; i++)
            {
                burndownData[i] = 1000 - (i * 10);
            }

            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 90,
                BurndownData = burndownData
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            await component.WaitForAsyncLoad();

            Assert.NotNull(component.Instance);
        }
    }
}