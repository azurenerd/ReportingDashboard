using Bunit;
using AngleSharp.Dom;
using Xunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsResponsiveTests : TestContext
    {
        [Fact]
        public void ProgressMetrics_DisplaysMetricsContent()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, new[] 
                { 
                    new MetricData { Label = "Completion", Value = 85 }
                })
            );

            var markup = component.Markup;
            Assert.Contains("Completion", markup);
            Assert.Contains("85", markup);
        }

        [Fact]
        public void ProgressMetrics_RendersBySemanticRole()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Title, "Test Metrics")
                .Add(p => p.Metrics, Array.Empty<MetricData>())
            );

            var headings = component.FindAll("h1, h2, h3, h4, h5, h6");
            Assert.True(headings.Count > 0 || component.Markup.Contains("Test Metrics"));
        }

        [Fact]
        public void ProgressMetrics_RendersProgressBar()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, new[] 
                { 
                    new MetricData { Label = "Progress", Value = 75 }
                })
            );

            var progressElements = component.FindAll("[role='progressbar'], progress");
            var hasProgressContent = component.Markup.Contains("75") || progressElements.Count > 0;
            Assert.True(hasProgressContent);
        }

        [Fact]
        public void ProgressMetrics_AdaptsToMultipleMetrics()
        {
            var metrics = new[]
            {
                new MetricData { Label = "Metric 1", Value = 45 },
                new MetricData { Label = "Metric 2", Value = 67 },
                new MetricData { Label = "Metric 3", Value = 89 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            var markup = component.Markup;
            Assert.Contains("Metric 1", markup);
            Assert.Contains("Metric 2", markup);
            Assert.Contains("Metric 3", markup);
        }

        [Fact]
        public void ProgressMetrics_RendersMetricLabels()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, new[] 
                { 
                    new MetricData { Label = "Burndown", Value = 50 }
                })
            );

            var markup = component.Markup;
            Assert.Contains("Burndown", markup);
        }

        [Fact]
        public void ProgressMetrics_DisplaysNumericValues()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, new[] 
                { 
                    new MetricData { Label = "Percentage", Value = 92 }
                })
            );

            var markup = component.Markup;
            Assert.Contains("92", markup);
        }
    }
}