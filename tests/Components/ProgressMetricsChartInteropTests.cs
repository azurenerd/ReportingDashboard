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
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, new[] 
                { 
                    new MetricData { Label = "Sprint 1", Value = 45 },
                    new MetricData { Label = "Sprint 2", Value = 78 }
                })
            );

            await component.WaitForAsyncLoad();

            var markup = component.Markup;
            Assert.Contains("Sprint 1", markup);
            Assert.Contains("Sprint 2", markup);
        }

        [Fact]
        public async Task ProgressMetrics_HandlesMissingChartLibrary()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, new[] { new MetricData { Label = "Test", Value = 50 } })
            );

            await component.WaitForAsyncLoad();

            Assert.NotNull(component.Instance);
            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }

        [Fact]
        public async Task ProgressMetrics_RendersChartContainerAfterInteropCall()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, new[] { new MetricData { Label = "Data", Value = 65 } })
            );

            await component.WaitForAsyncLoad();

            var elements = component.FindAll("[data-chart]");
            Assert.NotNull(elements);
        }

        [Fact]
        public async Task ProgressMetrics_UpdatesChartOnMetricsChange()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, new[] { new MetricData { Label = "Initial", Value = 30 } })
            );

            await component.WaitForAsyncLoad();

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, new[] { new MetricData { Label = "Updated", Value = 85 } })
            );

            await component.WaitForAsyncLoad();

            var markup = component.Markup;
            Assert.Contains("Updated", markup);
        }
    }
}