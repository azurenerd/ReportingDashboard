using Xunit;
using Bunit;
using AgentSquad.Components;
using AgentSquad.Models;
using System;
using System.Collections.Generic;

namespace AgentSquad.Tests.Components
{
    public class MilestoneTimelineTests : TestContext
    {
        [Fact]
        public void MilestoneTimeline_RendersWithMilestoneList()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Phase 1", TargetDate = DateTime.Now.AddDays(30), Status = MilestoneStatus.Pending, CompletionPercentage = 0 },
                new Milestone { Name = "Phase 2", TargetDate = DateTime.Now.AddDays(60), Status = MilestoneStatus.InProgress, CompletionPercentage = 50 }
            };

            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(90);

            // Act
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectStartDate, startDate)
                .Add(p => p.ProjectEndDate, endDate)
                .Add(p => p.ProjectDurationDays, 90));

            // Assert
            component.MarkupMatches(@"
                <div class=""milestone-timeline-container"">
                    <h2 class=""timeline-title"">Project Milestones</h2>
                    <div class=""timeline-wrapper"">
                        <div class=""timeline-grid"">
                            <div class=""milestone-card status-pending"">*</div>
                            <div class=""milestone-card status-inprogress"">*</div>
                        </div>
                    </div>
                </div>"
            );
        }

        [Fact]
        public void MilestoneTimeline_DisplaysMilestoneNames()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Planning", TargetDate = DateTime.Now.AddDays(30), Status = MilestoneStatus.Completed, CompletionPercentage = 100 }
            };

            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(90);

            // Act
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectStartDate, startDate)
                .Add(p => p.ProjectEndDate, endDate)
                .Add(p => p.ProjectDurationDays, 90));

            // Assert
            Assert.Contains("Planning", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_AppliesCorrectStatusClass()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Test", TargetDate = DateTime.Now.AddDays(30), Status = MilestoneStatus.Completed, CompletionPercentage = 100 }
            };

            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(90);

            // Act
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectStartDate, startDate)
                .Add(p => p.ProjectEndDate, endDate)
                .Add(p => p.ProjectDurationDays, 90));

            // Assert
            Assert.Contains("status-completed", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_CalculatesProjectDurationDays()
        {
            // Arrange
            var milestones = new List<Milestone>();
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(60);

            // Act
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectStartDate, startDate)
                .Add(p => p.ProjectEndDate, endDate)
                .Add(p => p.ProjectDurationDays, 0));

            // Assert - Duration should be calculated as 60 days
            var instance = component.Instance;
            Assert.Equal(60, instance.ProjectDurationDays);
        }

        [Fact]
        public void MilestoneTimeline_DisplaysCompletionPercentage()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Phase 1", TargetDate = DateTime.Now.AddDays(30), Status = MilestoneStatus.InProgress, CompletionPercentage = 75 }
            };

            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(90);

            // Act
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectStartDate, startDate)
                .Add(p => p.ProjectEndDate, endDate)
                .Add(p => p.ProjectDurationDays, 90));

            // Assert
            Assert.Contains("75%", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_RendersFontSizes()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Planning", TargetDate = DateTime.Now.AddDays(30), Status = MilestoneStatus.Pending, CompletionPercentage = 0 }
            };

            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(90);

            // Act
            var component = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectStartDate, startDate)
                .Add(p => p.ProjectEndDate, endDate)
                .Add(p => p.ProjectDurationDays, 90));

            // Assert - Verify CSS contains font-size declarations at 16px or higher
            Assert.Contains("font-size: 16px", component.Markup);
        }
    }
}