using System;
using System.Collections.Generic;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;
using Bunit;
using Xunit;

namespace AgentSquad.Runner.Tests.Components
{
    public class MilestoneTimelineTests : TestContext
    {
        [Fact]
        public void MilestoneTimeline_DisplaysAllMilestones()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Phase 1", TargetDate = new DateTime(2024, 3, 1), Status = "Completed", CompletionPercentage = 100 },
                new Milestone { Name = "Phase 2", TargetDate = new DateTime(2024, 6, 1), Status = "InProgress", CompletionPercentage = 50 },
                new Milestone { Name = "Phase 3", TargetDate = new DateTime(2024, 9, 1), Status = "Pending", CompletionPercentage = 0 }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectDurationDays, 273));

            Assert.Contains("Phase 1", component.Markup);
            Assert.Contains("Phase 2", component.Markup);
            Assert.Contains("Phase 3", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_DisplaysMilestoneTargetDates()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Milestone 1", TargetDate = new DateTime(2024, 3, 15), Status = "Completed" }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            Assert.Contains("2024-03-15", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_ColorCodesCompletedMilestones()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Completed", Status = "Completed" }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            Assert.Contains("bg-success", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_ColorCodesInProgressMilestones()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "In Progress", Status = "InProgress" }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            Assert.Contains("bg-info", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_ColorCodesPendingMilestones()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Pending", Status = "Pending" }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            Assert.Contains("bg-secondary", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_DisplaysCompletionPercentage()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Phase 1", Status = "InProgress", CompletionPercentage = 75 }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            Assert.Contains("75%", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithEmptyList_RendersSafely()
        {
            var milestones = new List<Milestone>();

            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            Assert.NotNull(component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_IsFullWidth()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Test", Status = "Completed" }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            Assert.Contains("w-100", component.Markup);
        }
    }
}