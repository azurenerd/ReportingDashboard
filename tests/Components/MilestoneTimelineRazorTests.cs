using Xunit;
using Bunit;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Components;

namespace AgentSquad.Runner.Tests.Components
{
    public class MilestoneTimelineRazorTests : TestContext
    {
        [Fact]
        public void MilestoneTimeline_WithNullMilestones_DisplaysPlaceholder()
        {
            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, null)
                    .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                    .Add(p => p.ProjectEndDate, new DateTime(2024, 12, 31))
            );

            Assert.Contains("No milestones available", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithEmptyMilestoneList_DisplaysPlaceholder()
        {
            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, new List<Milestone>())
                    .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                    .Add(p => p.ProjectEndDate, new DateTime(2024, 12, 31))
            );

            Assert.Contains("No milestones available", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithSingleMilestone_DisplaysMilestoneInfo()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "m1",
                    Name = "Phase 1 Launch",
                    TargetDate = new DateTime(2024, 3, 31),
                    Status = MilestoneStatus.Completed,
                    CompletionPercentage = 100
                }
            };

            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, milestones)
                    .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                    .Add(p => p.ProjectEndDate, new DateTime(2024, 12, 31))
            );

            Assert.Contains("Phase 1 Launch", component.Markup);
            Assert.Contains("3/31/2024", component.Markup);
            Assert.Contains("100%", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithCompletedMilestone_DisplaysGreenMarker()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "m1",
                    Name = "Phase 1",
                    TargetDate = new DateTime(2024, 3, 31),
                    Status = MilestoneStatus.Completed,
                    CompletionPercentage = 100
                }
            };

            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, milestones)
                    .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                    .Add(p => p.ProjectEndDate, new DateTime(2024, 12, 31))
            );

            Assert.Contains("background-color: #28a745;", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithInProgressMilestone_DisplaysBlueMarker()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "m1",
                    Name = "Phase 2",
                    TargetDate = new DateTime(2024, 6, 30),
                    Status = MilestoneStatus.InProgress,
                    CompletionPercentage = 50
                }
            };

            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, milestones)
                    .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                    .Add(p => p.ProjectEndDate, new DateTime(2024, 12, 31))
            );

            Assert.Contains("background-color: #007bff;", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithPendingMilestone_DisplaysGrayMarker()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "m1",
                    Name = "Phase 3",
                    TargetDate = new DateTime(2024, 9, 30),
                    Status = MilestoneStatus.Pending,
                    CompletionPercentage = 0
                }
            };

            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, milestones)
                    .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                    .Add(p => p.ProjectEndDate, new DateTime(2024, 12, 31))
            );

            Assert.Contains("background-color: #6c757d;", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithMultipleMilestones_DisplaysAll()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = "m1", Name = "Phase 1", TargetDate = new DateTime(2024, 3, 31), Status = MilestoneStatus.Completed, CompletionPercentage = 100 },
                new Milestone { Id = "m2", Name = "Phase 2", TargetDate = new DateTime(2024, 6, 30), Status = MilestoneStatus.InProgress, CompletionPercentage = 50 },
                new Milestone { Id = "m3", Name = "Phase 3", TargetDate = new DateTime(2024, 9, 30), Status = MilestoneStatus.Pending, CompletionPercentage = 0 }
            };

            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, milestones)
                    .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                    .Add(p => p.ProjectEndDate, new DateTime(2024, 12, 31))
            );

            Assert.Contains("Phase 1", component.Markup);
            Assert.Contains("Phase 2", component.Markup);
            Assert.Contains("Phase 3", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithActualDate_DisplaysActualDateInfo()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "m1",
                    Name = "Phase 1",
                    TargetDate = new DateTime(2024, 3, 31),
                    ActualDate = new DateTime(2024, 3, 28),
                    Status = MilestoneStatus.Completed,
                    CompletionPercentage = 100
                }
            };

            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, milestones)
                    .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                    .Add(p => p.ProjectEndDate, new DateTime(2024, 12, 31))
            );

            Assert.Contains("Actual Date", component.Markup);
            Assert.Contains("3/28/2024", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithProgressBar_DisplaysPercentage()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "m1",
                    Name = "Phase 1",
                    TargetDate = new DateTime(2024, 3, 31),
                    Status = MilestoneStatus.InProgress,
                    CompletionPercentage = 75
                }
            };

            var component = RenderComponent<MilestoneTimeline>(
                parameters => parameters
                    .Add(p => p.Milestones, milestones)
                    .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                    .Add(p => p.ProjectEndDate, new DateTime(2024, 12, 31))
            );

            Assert.Contains("width: 75%", component.Markup);
            Assert.Contains("75%", component.Markup);
        }
    }
}