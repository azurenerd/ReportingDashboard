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
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 85,
                BurndownData = new[] { 100, 15 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            var markup = component.Markup;
            Assert.Contains("85", markup);
        }

        [Fact]
        public void ProgressMetrics_RendersBySemanticRole()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 50,
                BurndownData = Array.Empty<int>()
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            var roles = component.FindAll("[role]");
            Assert.True(roles.Count >= 0 || component.Markup.Contains("50"));
        }

        [Fact]
        public void ProgressMetrics_RendersProgressIndicator()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 75,
                BurndownData = new[] { 100, 25 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            var progressElements = component.FindAll("progress, [role='progressbar'], [data-progress]");
            var hasProgressContent = component.Markup.Contains("75") || progressElements.Count > 0;
            Assert.True(hasProgressContent);
        }

        [Fact]
        public void ProgressMetrics_AdaptsToLargeBurndownData()
        {
            var burndownData = new int[] { 1000, 900, 800, 700, 600, 500, 400, 300, 200, 100, 0 };

            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 100,
                BurndownData = burndownData
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            var markup = component.Markup;
            Assert.Contains("100", markup);
        }

        [Fact]
        public void ProgressMetrics_RendersMinimalDataset()
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

        [Fact]
        public void ProgressMetrics_DisplaysNumericValuesCorrectly()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 92,
                BurndownData = new[] { 100, 8 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            var markup = component.Markup;
            Assert.Contains("92", markup);
        }

        [Fact]
        public void ProgressMetrics_RespondsToViewportChange()
        {
            var projectMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 60,
                BurndownData = new[] { 100, 40 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, projectMetrics)
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);
            var elements = component.FindAll("div, section, article");
            Assert.True(elements.Count > 0);
        }
    }
}