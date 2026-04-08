using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;
using System;
using System.Collections.Generic;

namespace AgentSquad.Tests.Components
{
    public class MilestoneTimelineTests : TestContext
    {
        [Fact]
        public void MilestoneTimeline_RendersMilestones()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = "m1", Name = "Phase 1", Status = "Completed", TargetDate = DateTime.Now },
                new Milestone { Id = "m2", Name = "Phase 2", Status = "InProgress", TargetDate = DateTime.Now.AddMonths(1) }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones)
            );

            var html = component.Markup;
            Assert.Contains("Phase 1", html);
            Assert.Contains("Phase 2", html);
        }

        [Fact]
        public void MilestoneTimeline_DisplaysMilestoneStatus()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = "m1", Name = "Milestone", Status = "Planning", TargetDate = DateTime.Now }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones)
            );

            Assert.Contains("Planning", component.Markup);
        }
    }
}