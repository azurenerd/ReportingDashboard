using System;
using System.Collections.Generic;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;
using Bunit;
using Xunit;

namespace AgentSquad.Runner.Tests.Components
{
    public class ProgressMetricsTests : TestContext
    {
        [Fact]
        public void ProgressMetrics_DisplaysCompletionPercentage()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 75));

            Assert.Contains("75%", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_DisplaysProgressBar()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 50));

            Assert.Contains("progress", component.Markup, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ProgressMetrics_With0Percent_RendersSafely()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 0));

            Assert.Contains("0%", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_With100Percent_RendersSafely()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 100));

            Assert.Contains("100%", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_ProgressBarWidthMatchesPercentage()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 60));

            Assert.Contains("style=\"width: 60%\"", component.Markup);
        }

        [Fact]
        public void ProgressMetrics_DisplaysTitle()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 50));

            Assert.Contains("Progress", component.Markup, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ProgressMetrics_ContainsMilestoneChart()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Phase 1", Status = "Completed" },
                new Milestone { Name = "Phase 2", Status = "InProgress" }
            };

            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 50)
                .Add(p => p.Milestones, milestones));

            Assert.NotNull(component.Markup);
        }

        [Fact]
        public void ProgressMetrics_NoAnimations()
        {
            var component = RenderComponent<ProgressMetrics>(parameters => parameters
                .Add(p => p.CompletionPercentage, 75));

            Assert.DoesNotContain("transition", component.Markup, StringComparison.OrdinalIgnoreCase);
        }
    }
}