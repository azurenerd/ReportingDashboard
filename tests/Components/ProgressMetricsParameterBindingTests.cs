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
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, new[] { new MetricData { Label = "Initial", Value = 25 } })
            );

            var initialText = component.Find("div").TextContent;
            Assert.Contains("Initial", initialText);

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, new[] { new MetricData { Label = "Updated", Value = 75 } })
            );

            var updatedText = component.Find("div").TextContent;
            Assert.Contains("Updated", updatedText);
        }

        [Fact]
        public async Task ProgressMetrics_HandlesNullMetrics()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, null)
            );

            Assert.NotNull(component.Instance);

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, Array.Empty<MetricData>())
            );

            var rendered = component.GetComponentInstance<ProgressMetrics>();
            Assert.NotNull(rendered);
        }

        [Fact]
        public async Task ProgressMetrics_RespondsToTitleParameterChange()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Title, "Original Title")
            );

            var markup = component.Markup;
            Assert.Contains("Original Title", markup);

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Title, "New Title")
            );

            var updatedMarkup = component.Markup;
            Assert.Contains("New Title", updatedMarkup);
        }

        [Fact]
        public async Task ProgressMetrics_UpdatesMultipleParametersSimultaneously()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Title, "Title1")
                .Add(p => p.Metrics, new[] { new MetricData { Label = "Metric1", Value = 50 } })
            );

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Title, "Title2")
                .Add(p => p.Metrics, new[] { new MetricData { Label = "Metric2", Value = 90 } })
            );

            var markup = component.Markup;
            Assert.Contains("Title2", markup);
            Assert.Contains("Metric2", markup);
        }
    }
}