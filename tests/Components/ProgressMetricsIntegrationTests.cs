using Bunit;
using Xunit;
using AgentSquad.Components;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsIntegrationTests : TestContext
    {
        [Fact]
        public async Task ProgressMetrics_IntegrationWithMultipleUpdates()
        {
            var initialMetrics = new ProjectMetrics 
            { 
                CompletionPercentage = 10,
                BurndownData = new[] { 1000, 900 }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, initialMetrics)
            );

            for (int i = 20; i <= 100; i += 20)
            {
                var metrics = new ProjectMetrics 
                { 
                    CompletionPercentage = i,
                    BurndownData = new[] { 1000, 1000 - (i * 10) }
                };

                await component.SetParametersAsync(parameters => parameters
                    .Add(p => p.Metrics, metrics)
                );

                Assert.NotNull(component.Instance);
            }
        }

        [Fact]
        public async Task ProgressMetrics_RapidParameterChanges()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.Metrics, new ProjectMetrics { CompletionPercentage = 0, BurndownData = Array.Empty<int>() })
            );

            var tasks = new System.Collections.Generic.List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(component.SetParametersAsync(parameters => parameters
                    .Add(p => p.Metrics, new ProjectMetrics 
                    { 
                        CompletionPercentage = i * 10,
                        BurndownData = new[] { 100 - (i * 10) }
                    })
                ));
            }

            await Task.WhenAll(tasks);
            Assert.NotNull(component.Instance);
        }

        [Fact]
        public void ProgressMetrics_ConcurrentRenderingInstances()
        {
            var metrics1 = new ProjectMetrics { CompletionPercentage = 25, BurndownData = new[] { 100, 75 } };
            var metrics2 = new ProjectMetrics { CompletionPercentage = 50, BurndownData = new[] { 100, 50 } };
            var metrics3 = new ProjectMetrics { CompletionPercentage = 75, BurndownData = new[] { 100, 25 } };

            var component1 = RenderComponent<ProgressMetrics>(p => p.Add(x => x.Metrics, metrics1));
            var component2 = RenderComponent<ProgressMetrics>(p => p.Add(x => x.Metrics, metrics2));
            var component3 = RenderComponent<ProgressMetrics>(p => p.Add(x => x.Metrics, metrics3));

            Assert.NotNull(component1.Instance);
            Assert.NotNull(component2.Instance);
            Assert.NotNull(component3.Instance);
        }
    }
}