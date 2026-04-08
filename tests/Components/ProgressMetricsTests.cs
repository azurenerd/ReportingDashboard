using Bunit;
using Xunit;
using AgentSquad.Runner.Components;

namespace AgentSquad.Runner.Tests.Components
{
    public class ProgressMetricsTests : TestContext
    {
        [Fact]
        public void ProgressMetrics_Renders_WithDefaults()
        {
            // Act
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 0)
                    .Add(p => p.TotalTasks, 0)
                    .Add(p => p.CompletedTasks, 0)
                    .Add(p => p.BurndownRate, 0.0)
            );

            // Assert
            Assert.NotNull(component);
        }

        [Fact]
        public void ProgressMetrics_Renders_WithProgressData()
        {
            // Act
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 50)
                    .Add(p => p.TotalTasks, 20)
                    .Add(p => p.CompletedTasks, 10)
                    .Add(p => p.BurndownRate, 0.5)
            );

            // Assert
            Assert.NotNull(component);
            component.Markup.Should().Contain("progress");
        }

        [Fact]
        public void ProgressMetrics_Renders_With100PercentCompletion()
        {
            // Act
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 100)
                    .Add(p => p.TotalTasks, 10)
                    .Add(p => p.CompletedTasks, 10)
                    .Add(p => p.BurndownRate, 1.0)
            );

            // Assert
            Assert.NotNull(component);
        }

        [Fact]
        public void ProgressMetrics_Renders_WithVaryingCompletionPercentages()
        {
            // Test cases
            var percentages = new[] { 0, 25, 50, 75, 100 };

            foreach (var percentage in percentages)
            {
                // Act
                var component = RenderComponent<ProgressMetrics>(
                    parameters => parameters
                        .Add(p => p.CompletionPercentage, percentage)
                        .Add(p => p.TotalTasks, 100)
                        .Add(p => p.CompletedTasks, percentage)
                        .Add(p => p.BurndownRate, 0.5)
                );

                // Assert
                Assert.NotNull(component);
            }
        }

        [Fact]
        public void ProgressMetrics_Renders_WithPlaceholderText()
        {
            // Act
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 0)
                    .Add(p => p.TotalTasks, 0)
                    .Add(p => p.CompletedTasks, 0)
                    .Add(p => p.BurndownRate, 0.0)
            );

            // Assert
            component.Markup.Should().Contain("placeholder");
        }

        [Fact]
        public void ProgressMetrics_Renders_Section()
        {
            // Act
            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.CompletionPercentage, 50)
                    .Add(p => p.TotalTasks, 20)
                    .Add(p => p.CompletedTasks, 10)
                    .Add(p => p.BurndownRate, 0.5)
            );

            // Assert
            var section = component.Find(".progress-section");
            Assert.NotNull(section);
        }
    }
}