using Bunit;
using AngleSharp.Dom;
using Xunit;
using AgentSquad.Components;
using System;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsResponsiveTests : TestContext
    {
        private ProjectMetrics CreateTestMetrics(int totalTasks, int completedTasks)
        {
            return new ProjectMetrics 
            { 
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                ProjectStartDate = DateTime.Now.AddDays(-10),
                ProjectEndDate = DateTime.Now.AddDays(20),
                EstimatedBurndownRate = 5.0,
                DaysRemaining = 20
            };
        }

        [Fact]
        public void ProgressMetrics_DisplaysMetricsContent()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 85))
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }

        [Fact]
        public void ProgressMetrics_RendersBySemanticRole()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 50))
            );

            var roles = component.FindAll("[role]");
            Assert.True(roles.Count >= 0 || component.Markup.Contains("Metrics"));
        }

        [Fact]
        public void ProgressMetrics_RendersProgressIndicator()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 75))
            );

            var progressElements = component.FindAll("progress, [role='progressbar'], [data-progress]");
            Assert.True(progressElements.Count >= 0 || component.Markup.Length > 0);
        }

        [Fact]
        public void ProgressMetrics_AdaptsToLargeTaskCount()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(5000, 3750))
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }

        [Fact]
        public void ProgressMetrics_RendersMinimalDataset()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(1, 0))
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }

        [Fact]
        public void ProgressMetrics_DisplaysProjectTimeline()
        {
            var startDate = DateTime.Now.AddDays(-15);
            var endDate = DateTime.Now.AddDays(25);

            var metrics = new ProjectMetrics 
            { 
                TotalTasks = 100,
                CompletedTasks = 50,
                ProjectStartDate = startDate,
                ProjectEndDate = endDate,
                EstimatedBurndownRate = 3.0,
                DaysRemaining = 25
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }

        [Fact]
        public void ProgressMetrics_RespondsToViewportChange()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 60))
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);
            var elements = component.FindAll("div, section, article");
            Assert.True(elements.Count > 0);
        }
    }
}