using Bunit;
using Xunit;
using AgentSquad.Components;
using System;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsEdgeCasesTests : TestContext
    {
        private ProjectMetrics CreateTestMetrics(int totalTasks, int completedTasks, int daysRemaining = 20)
        {
            return new ProjectMetrics 
            { 
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                ProjectStartDate = DateTime.Now.AddDays(-10),
                ProjectEndDate = DateTime.Now.AddDays(daysRemaining),
                EstimatedBurndownRate = 5.0,
                DaysRemaining = daysRemaining
            };
        }

        [Fact]
        public void ProgressMetrics_HandleFullCompletion()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 100))
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_HandlesSingleTask()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(1, 0))
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
        public void ProgressMetrics_HandlesMoreCompletedThanTotal()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(50, 75))
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_HandlesZeroTotalTasks()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(0, 0))
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_HandlesPastEndDate()
        {
            var metrics = new ProjectMetrics 
            { 
                TotalTasks = 100,
                CompletedTasks = 50,
                ProjectStartDate = DateTime.Now.AddDays(-30),
                ProjectEndDate = DateTime.Now.AddDays(-5),
                EstimatedBurndownRate = 5.0,
                DaysRemaining = -5
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_HandlesVeryLargeTaskCount()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(999999, 500000))
            );

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_HandlesZeroBurndownRate()
        {
            var metrics = new ProjectMetrics 
            { 
                TotalTasks = 100,
                CompletedTasks = 25,
                ProjectStartDate = DateTime.Now.AddDays(-10),
                ProjectEndDate = DateTime.Now.AddDays(20),
                EstimatedBurndownRate = 0.0,
                DaysRemaining = 20
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            Assert.NotNull(component.Instance);
        }
    }
}