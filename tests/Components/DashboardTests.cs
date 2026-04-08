using Xunit;
using FluentAssertions;
using Bunit;
using AgentSquad.Components;
using AgentSquad.Services.Models;
using System.Collections.Generic;

namespace AgentSquad.Tests.Components
{
    public class DashboardTests : TestContext
    {
        [Fact]
        public void Dashboard_RendersMilestoneTimeline_WhenMilestonesProvided()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = 1, Title = "Phase 1", Status = MilestoneStatus.Active }
            };
            var component = RenderComponent<Dashboard>(parameters =>
                parameters.Add(p => p.Milestones, milestones));
            
            component.Find("milestone-timeline").Should().NotBeNull();
        }

        [Fact]
        public void Dashboard_RendersMilestoneTimeline_WithEmptyMilestones()
        {
            var component = RenderComponent<Dashboard>(parameters =>
                parameters.Add(p => p.Milestones, new List<Milestone>()));
            
            component.Find(".milestone-timeline").Should().NotBeNull();
        }

        [Fact]
        public void Dashboard_RendersMilestoneTimeline_WithNullMilestones()
        {
            var component = RenderComponent<Dashboard>(parameters =>
                parameters.Add(p => p.Milestones, (List<Milestone>)null));
            
            component.Markup.Should().Contain("milestone-timeline");
        }

        [Fact]
        public void Dashboard_RendersStatusCard_WhenTasksProvided()
        {
            var tasks = new List<TaskItem>
            {
                new TaskItem { Id = 1, Title = "Task 1", Status = TaskStatus.InProgress }
            };
            var component = RenderComponent<Dashboard>(parameters =>
                parameters.Add(p => p.Tasks, tasks));
            
            component.Find("status-card").Should().NotBeNull();
        }

        [Fact]
        public void Dashboard_RendersStatusCard_WithNullTasks()
        {
            var component = RenderComponent<Dashboard>(parameters =>
                parameters.Add(p => p.Tasks, (List<TaskItem>)null));
            
            component.Markup.Should().Contain("status-card");
        }

        [Fact]
        public void Dashboard_RendersProgressMetrics_WhenMetricsProvided()
        {
            var metrics = new ProjectMetrics { CompletionPercentage = 75 };
            var component = RenderComponent<Dashboard>(parameters =>
                parameters.Add(p => p.Metrics, metrics));
            
            component.Find("progress-metrics").Should().NotBeNull();
        }

        [Fact]
        public void Dashboard_RendersProgressMetrics_WithNullMetrics()
        {
            var component = RenderComponent<Dashboard>(parameters =>
                parameters.Add(p => p.Metrics, (ProjectMetrics)null));
            
            component.Markup.Should().Contain("progress-metrics");
        }

        [Fact]
        public void Dashboard_RendersChildrenInCorrectOrder()
        {
            var component = RenderComponent<Dashboard>(parameters =>
                parameters.Add(p => p.Milestones, new List<Milestone>())
                .Add(p => p.Tasks, new List<TaskItem>())
                .Add(p => p.Metrics, new ProjectMetrics()));
            
            var timelineIndex = component.Markup.IndexOf("milestone-timeline");
            var statusIndex = component.Markup.IndexOf("status-card");
            var metricsIndex = component.Markup.IndexOf("progress-metrics");
            
            timelineIndex.Should().BeLessThan(statusIndex);
            statusIndex.Should().BeLessThan(metricsIndex);
        }

        [Fact]
        public void Dashboard_AppliesResponsiveGridClasses()
        {
            var component = RenderComponent<Dashboard>(parameters =>
                parameters.Add(p => p.Milestones, new List<Milestone>())
                .Add(p => p.Tasks, new List<TaskItem>())
                .Add(p => p.Metrics, new ProjectMetrics()));
            
            component.Markup.Should().Contain("col-12").And.Contain("col-md-4");
        }
    }
}