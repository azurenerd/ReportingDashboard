using Xunit;
using Bunit;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Components;

namespace AgentSquad.Runner.Tests.Components
{
    public class ProgressMetricsRazorTests : TestContext
    {
        [Fact]
        public void ProgressMetrics_WithValidMetrics_DisplaysCompletionPercentage()
        {
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 50)
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.Metrics, new ProjectMetrics())
            );

            Assert.Contains("50%", component.Markup);
            Assert.Contains("Overall Project Completion", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithValidMetrics_DisplaysProjectName()
        {
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 50)
                    .Add(p => p.ProjectName, "Q2 Mobile App Release")
                    .Add(p => p.Metrics, new ProjectMetrics())
            );

            Assert.Contains("Q2 Mobile App Release", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithNullMetrics_DoesNotDisplayMetricBoxes()
        {
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 50)
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.Metrics, null)
            );

            Assert.DoesNotContain("Total Tasks", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithMetrics_DisplaysTotalTasks()
        {
            var metrics = new ProjectMetrics { TotalTasks = 10 };
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 50)
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.Metrics, metrics)
            );

            Assert.Contains("Total Tasks", component.Markup);
            Assert.Contains("10", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithMetrics_DisplaysCompletedTasks()
        {
            var metrics = new ProjectMetrics { CompletedTasks = 5 };
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 50)
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.Metrics, metrics)
            );

            Assert.Contains("Completed", component.Markup);
            Assert.Contains("5", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithMetrics_DisplaysInProgressTasks()
        {
            var metrics = new ProjectMetrics { InProgressTasks = 3 };
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 50)
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.Metrics, metrics)
            );

            Assert.Contains("In-Progress", component.Markup);
            Assert.Contains("3", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithMetrics_DisplaysCarriedOverTasks()
        {
            var metrics = new ProjectMetrics { CarriedOverTasks = 2 };
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 50)
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.Metrics, metrics)
            );

            Assert.Contains("Carried-Over", component.Markup);
            Assert.Contains("2", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithMetrics_DisplaysBurndownRate()
        {
            var metrics = new ProjectMetrics { EstimatedBurndownRate = 0.25m };
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 50)
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.Metrics, metrics)
            );

            Assert.Contains("Estimated Burndown Rate", component.Markup);
            Assert.Contains("25%", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithZeroCompletion_DisplaysProgressBar()
        {
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 0)
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.Metrics, new ProjectMetrics())
            );

            Assert.Contains("progress-bar", component.Markup);
            Assert.Contains("width: 0%", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_With100Percent_DisplaysProgressBar()
        {
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 100)
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.Metrics, new ProjectMetrics())
            );

            Assert.Contains("width: 100%", component.Markup);
            Assert.Contains("100%", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithPartialCompletion_DisplaysProgressBar()
        {
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 75)
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.Metrics, new ProjectMetrics())
            );

            Assert.Contains("width: 75%", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_WithAllMetricsPopulated_DisplaysAllBoxes()
        {
            var metrics = new ProjectMetrics
            {
                TotalTasks = 10,
                CompletedTasks = 5,
                InProgressTasks = 3,
                CarriedOverTasks = 2,
                EstimatedBurndownRate = 0.20m
            };

            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 50)
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.Metrics, metrics)
            );

            var markup = component.Markup;
            Assert.Contains("Total Tasks", markup);
            Assert.Contains("Completed", markup);
            Assert.Contains("In-Progress", markup);
            Assert.Contains("Carried-Over", markup);
            Assert.Contains("Estimated Burndown Rate", markup);
        }
    }
}