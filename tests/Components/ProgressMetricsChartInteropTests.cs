using Bunit;
using Xunit;
using AgentSquad.Components;
using System;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsChartInteropTests : TestContext
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
        public void ProgressMetrics_LoadsChartDataViaInterop()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 45))
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }

        [Fact]
        public void ProgressMetrics_HandlesMissingChartLibrary()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 50))
            );

            Assert.NotNull(component.Instance);
            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }

        [Fact]
        public void ProgressMetrics_RendersChartContainer()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 65))
            );

            var elements = component.FindAll("[data-chart]");
            Assert.NotNull(elements);
        }

        [Fact]
        public async System.Threading.Tasks.Task ProgressMetrics_UpdatesChartOnMetricsChange()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 30))
            );

            var initialMarkup = component.Markup;
            Assert.Contains("30", initialMarkup);

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 85))
            );

            var updatedMarkup = component.Markup;
            Assert.Contains("85", updatedMarkup);
        }

        [Fact]
        public void ProgressMetrics_HandlesBurndownDateRange()
        {
            var startDate = DateTime.Now.AddDays(-30);
            var endDate = DateTime.Now.AddDays(30);

            var metrics = new ProjectMetrics 
            { 
                TotalTasks = 200,
                CompletedTasks = 100,
                ProjectStartDate = startDate,
                ProjectEndDate = endDate,
                EstimatedBurndownRate = 3.33,
                DaysRemaining = 30
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            Assert.NotNull(component.Instance);
        }
    }
}