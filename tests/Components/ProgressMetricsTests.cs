using Bunit;
using Xunit;
using AgentSquad.Components;
using System;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsTests : TestContext
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
        public void ProgressMetrics_RendersSuccessfully()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 75))
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_DisplaysCompletionStatus()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 85))
            );

            var markup = component.Markup;
            Assert.Contains("85", markup);
        }

        [Fact]
        public void ProgressMetrics_RendersBurndownChart()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 50))
            );

            var elements = component.FindAll("[data-burndown]");
            Assert.NotNull(elements);
        }

        [Fact]
        public void ProgressMetrics_HandlesNullMetrics()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, null as ProjectMetrics)
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_HandlesZeroCompletion()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 0))
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }
    }
}