using Bunit;
using Xunit;
using AgentSquad.Components;
using System;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsIntegrationTests : TestContext
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
        public async System.Threading.Tasks.Task ProgressMetrics_IntegrationWithMultipleUpdates()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 10))
            );

            for (int i = 20; i <= 100; i += 20)
            {
                await component.SetParametersAsync(parameters => parameters
                    .Add(p => p.Metrics, CreateTestMetrics(100, i))
                );

                Assert.NotNull(component.Instance);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task ProgressMetrics_RapidParameterChanges()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 0))
            );

            for (int i = 0; i < 10; i++)
            {
                await component.SetParametersAsync(parameters => parameters
                    .Add(p => p.Metrics, CreateTestMetrics(100, i * 10))
                );

                await System.Threading.Tasks.Task.Delay(50);
            }

            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_ConcurrentRenderingInstances()
        {
            var metrics1 = CreateTestMetrics(100, 25);
            var metrics2 = CreateTestMetrics(100, 50);
            var metrics3 = CreateTestMetrics(100, 75);

            var component1 = RenderComponent<ProgressMetrics>(p => p.Add(x => x.Metrics, metrics1));
            var component2 = RenderComponent<ProgressMetrics>(p => p.Add(x => x.Metrics, metrics2));
            var component3 = RenderComponent<ProgressMetrics>(p => p.Add(x => x.Metrics, metrics3));

            Assert.NotNull(component1.Instance);
            Assert.NotNull(component2.Instance);
            Assert.NotNull(component3.Instance);
        }

        [Fact]
        public async System.Threading.Tasks.Task ProgressMetrics_UpdatesWithDifferentDateRanges()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 30, 15))
            );

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 60, 30))
            );

            await component.SetParametersAsync(parameters => parameters
                .Add(p => p.Metrics, CreateTestMetrics(100, 90, 5))
            );

            Assert.NotNull(component.Instance);
        }
    }
}