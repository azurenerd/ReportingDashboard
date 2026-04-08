using Bunit;
using Xunit;
using AgentSquad.Components;
using System;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsParameterBindingTests : TestContext
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
        public async System.Threading.Tasks.Task ProgressMetrics_UpdatesWhenMetricsParameterChanges()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 25))
            );

            var initialText = component.Markup;
            Assert.Contains("25", initialText);

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 75))
            );

            var updatedText = component.Markup;
            Assert.Contains("75", updatedText);
        }

        [Fact]
        public async System.Threading.Tasks.Task ProgressMetrics_HandlesNullMetrics()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, null as ProjectMetrics)
            );

            Assert.NotNull(component.Instance);

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 40))
            );

            var rendered = component.GetComponentInstance<ProgressMetrics>();
            Assert.NotNull(rendered);
        }

        [Fact]
        public async System.Threading.Tasks.Task ProgressMetrics_HandlesZeroCompletion()
        {
            var metrics = CreateTestMetrics(100, 0);

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 100))
            );

            var updatedMarkup = component.Markup;
            Assert.Contains("100", updatedMarkup);
        }

        [Fact]
        public async System.Threading.Tasks.Task ProgressMetrics_HandlesFullCompletion()
        {
            var metrics = CreateTestMetrics(50, 50);

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, metrics)
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }

        [Fact]
        public async System.Threading.Tasks.Task ProgressMetrics_UpdatesMultiplePropertiesSimultaneously()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 20))
            );

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(150, 90))
            );

            var markup = component.Markup;
            Assert.NotEmpty(markup);
        }
    }
}