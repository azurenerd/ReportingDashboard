using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;
using System;
using System.Collections.Generic;

namespace AgentSquad.Tests.Integration
{
    public class DashboardMilestoneIntegrationTests : TestContext
    {
        [Fact]
        public void MilestoneTimeline_IntegratesWithDashboardLayout()
        {
            var milestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Phase 1 Complete",
                    TargetDate = new DateTime(2026, 3, 31),
                    Status = MilestoneStatus.Completed,
                    Description = "MVP released"
                },
                new Milestone
                {
                    Name = "Phase 2 In Progress",
                    TargetDate = new DateTime(2026, 6, 30),
                    Status = MilestoneStatus.InProgress,
                    Description = "Feature expansion"
                }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            Assert.NotNull(cut);
            var items = cut.FindAll(".timeline-item");
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public void MilestoneTimeline_RendersProperly_WithCompletedMilestone()
        {
            var milestone = new Milestone
            {
                Name = "Project Kickoff",
                TargetDate = new DateTime(2026, 1, 15),
                Status = MilestoneStatus.Completed,
                Description = "Initial team alignment"
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            Assert.Equal("completed", item.GetAttribute("data-status"));
        }

        [Fact]
        public void MilestoneTimeline_RendersProperly_WithInProgressMilestone()
        {
            var milestone = new Milestone
            {
                Name = "Core Development",
                TargetDate = new DateTime(2026, 4, 30),
                Status = MilestoneStatus.InProgress,
                Description = "Active development"
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            Assert.Equal("inprogress", item.GetAttribute("data-status"));
        }

        [Fact]
        public void MilestoneTimeline_RendersProperly_WithAtRiskMilestone()
        {
            var milestone = new Milestone
            {
                Name = "QA Phase",
                TargetDate = new DateTime(2026, 5, 31),
                Status = MilestoneStatus.AtRisk,
                Description = "Testing challenges"
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            Assert.Equal("atrisk", item.GetAttribute("data-status"));
        }

        [Fact]
        public void MilestoneTimeline_RendersProperly_WithFutureMilestone()
        {
            var milestone = new Milestone
            {
                Name = "Production Release",
                TargetDate = new DateTime(2026, 9, 30),
                Status = MilestoneStatus.Future,
                Description = "Live deployment"
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, new List<Milestone> { milestone }));

            var item = cut.Find(".timeline-item");
            Assert.Equal("future", item.GetAttribute("data-status"));
        }

        [Fact]
        public void MilestoneTimeline_RespondsToParameterChanges()
        {
            var initialMilestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Phase 1",
                    TargetDate = new DateTime(2026, 6, 30),
                    Status = MilestoneStatus.Completed
                }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, initialMilestones));

            var initialItems = cut.FindAll(".timeline-item");
            Assert.Single(initialItems);

            var updatedMilestones = new List<Milestone>
            {
                new Milestone
                {
                    Name = "Phase 1",
                    TargetDate = new DateTime(2026, 6, 30),
                    Status = MilestoneStatus.Completed
                },
                new Milestone
                {
                    Name = "Phase 2",
                    TargetDate = new DateTime(2026, 9, 30),
                    Status = MilestoneStatus.Future
                }
            };

            cut.SetParametersAndRender(parameters => parameters
                .Add(p => p.Milestones, updatedMilestones));

            var updatedItems = cut.FindAll(".timeline-item");
            Assert.Equal(2, updatedItems.Count);
        }

        [Fact]
        public void MilestoneTimeline_DisplaysAllStatuses_OnDashboard()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "Complete", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed },
                new Milestone { Name = "InProgress", TargetDate = DateTime.Now, Status = MilestoneStatus.InProgress },
                new Milestone { Name = "AtRisk", TargetDate = DateTime.Now, Status = MilestoneStatus.AtRisk },
                new Milestone { Name = "Future", TargetDate = DateTime.Now, Status = MilestoneStatus.Future }
            };

            var cut = RenderComponent<MilestoneTimeline>(parameters => parameters
                .Add(p => p.Milestones, milestones));

            var items = cut.FindAll(".timeline-item");
            Assert.Equal(4, items.Count);

            var statuses = new List<string>();
            foreach (var item in items)
            {
                statuses.Add(item.GetAttribute("data-status"));
            }

            Assert.Contains("completed", statuses);
            Assert.Contains("inprogress", statuses);
            Assert.Contains("atrisk", statuses);
            Assert.Contains("future", statuses);
        }
    }
}