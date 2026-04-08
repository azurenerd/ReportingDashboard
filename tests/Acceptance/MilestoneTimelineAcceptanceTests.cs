using Xunit;
using FluentAssertions;
using Bunit;
using AgentSquad.Components;
using AgentSquad.Services.Models;
using System.Collections.Generic;
using System.Linq;

namespace AgentSquad.Tests.Acceptance
{
    public class MilestoneTimelineAcceptanceTests : TestContext
    {
        [Fact]
        public void AC1_TimelineDisplaysMilestones()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = 1, Title = "Phase 1", Status = MilestoneStatus.Active }
            };
            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones));
            
            component.Markup.Should().Contain("Phase 1");
        }

        [Fact]
        public void AC2_TimelineDisplaysMultipleMilestones()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = 1, Title = "Phase 1", Status = MilestoneStatus.Active },
                new Milestone { Id = 2, Title = "Phase 2", Status = MilestoneStatus.Active },
                new Milestone { Id = 3, Title = "Phase 3", Status = MilestoneStatus.Active }
            };
            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones));
            
            component.Markup.Should().Contain("Phase 1").And.Contain("Phase 2").And.Contain("Phase 3");
        }

        [Fact]
        public void AC3_TimelineCalculatesMilestonePositions()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = 1, Title = "Phase 1", Status = MilestoneStatus.Active, DaysFromStart = 0 },
                new Milestone { Id = 2, Title = "Phase 2", Status = MilestoneStatus.Active, DaysFromStart = 50 }
            };
            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectDurationDays, 100));
            
            var phase1 = component.Find(".milestone-item:nth-child(1)");
            var phase2 = component.Find(".milestone-item:nth-child(2)");
            
            var left1 = phase1.GetAttribute("style");
            var left2 = phase2.GetAttribute("style");
            
            left1.Should().Contain("left: 0%");
            left2.Should().Contain("left: 50%");
        }

        [Fact]
        public void AC4_TimelineHorizontalLayoutMath()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = 1, Title = "Phase 1", Status = MilestoneStatus.Active, DaysFromStart = 25 },
                new Milestone { Id = 2, Title = "Phase 2", Status = MilestoneStatus.Active, DaysFromStart = 75 }
            };
            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectDurationDays, 100));
            
            var timeline = component.Find(".timeline-axis");
            
            timeline.Should().NotBeNull();
            timeline.GetAttribute("style").Should().Contain("width");
        }

        [Fact]
        public void AC5_FontSizeIs12ptOrGreater()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = 1, Title = "Phase 1", Status = MilestoneStatus.Active }
            };
            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones));
            
            var titleElement = component.Find(".milestone-title");
            var style = titleElement.GetAttribute("style");
            
            style.Should().NotBeNullOrEmpty();
            style.Should().NotContain("font-size: 8pt");
            style.Should().NotContain("font-size: 9pt");
            style.Should().NotContain("font-size: 10pt");
            style.Should().NotContain("font-size: 11pt");
        }

        [Fact]
        public void AC6_StatusColorCodeByMilestoneState()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = 1, Title = "Active Milestone", Status = MilestoneStatus.Active },
                new Milestone { Id = 2, Title = "Completed Milestone", Status = MilestoneStatus.Completed },
                new Milestone { Id = 3, Title = "Blocked Milestone", Status = MilestoneStatus.Blocked }
            };
            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones));
            
            component.Markup.Should().Contain("status-active").And.Contain("status-completed").And.Contain("status-blocked");
        }

        [Fact]
        public void AC7_NoAnimationsInterfering()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = 1, Title = "Phase 1", Status = MilestoneStatus.Active }
            };
            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones));
            
            var styleElements = component.FindAll("[style*='animation']").ToList();
            var transitionElements = component.FindAll("[style*='transition']").ToList();
            
            styleElements.Should().BeEmpty("no animation properties should be applied");
            transitionElements.Should().BeEmpty("no transition properties should be applied");
        }

        [Fact]
        public void AC8_TimelineResponsiveAtMobileViewport()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = 1, Title = "Phase 1", Status = MilestoneStatus.Active }
            };
            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones));
            
            component.Markup.Should().Contain("col-12");
        }

        [Fact]
        public void AC9_IntegrationFullWorkflow_LoadData_RenderTimeline()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = 1, Title = "Discovery", Status = MilestoneStatus.Completed, DaysFromStart = 0 },
                new Milestone { Id = 2, Title = "Development", Status = MilestoneStatus.Active, DaysFromStart = 30 },
                new Milestone { Id = 3, Title = "Testing", Status = MilestoneStatus.Active, DaysFromStart = 60 },
                new Milestone { Id = 4, Title = "Launch", Status = MilestoneStatus.Active, DaysFromStart = 90 }
            };
            
            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectDurationDays, 100));
            
            component.Markup.Should().Contain("Discovery");
            component.Markup.Should().Contain("Development");
            component.Markup.Should().Contain("Testing");
            component.Markup.Should().Contain("Launch");
            
            component.FindAll(".milestone-item").Should().HaveCount(4);
        }
    }
}