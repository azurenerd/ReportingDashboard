using Xunit;
using Bunit;
using AgentSquad.Components;
using AgentSquad.Data;

namespace AgentSquad.Tests.Components
{
    public class MilestoneTimelineTests : TestContext
    {
        [Fact]
        public void MilestoneTimeline_WithValidMilestones_RendersCorrectly()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = "m1", Name = "Q1", TargetDate = DateTime.Now.AddMonths(1) },
                new Milestone { Id = "m2", Name = "Q2", TargetDate = DateTime.Now.AddMonths(4) }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, milestones));

            Assert.NotNull(component);
            Assert.Contains("Q1", component.Markup);
            Assert.Contains("Q2", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithEmptyList_RendersEmptyState()
        {
            var milestones = new List<Milestone>();
            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones));

            Assert.NotNull(component);
            Assert.DoesNotContain("Q1", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithNullMilestones_HandlesGracefully()
        {
            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, (List<Milestone>)null!));

            Assert.NotNull(component);
        }

        [Fact]
        public void MilestoneTimeline_RendersTargetDatesCorrectly()
        {
            var targetDate = new DateTime(2026, 6, 30);
            var milestones = new List<Milestone>
            {
                new Milestone { Id = "m1", Name = "Phase 1", TargetDate = targetDate }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones));

            Assert.Contains("Phase 1", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithMultipleMilestones_MaintainsOrder()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = "m1", Name = "First", TargetDate = DateTime.Now.AddMonths(1) },
                new Milestone { Id = "m2", Name = "Second", TargetDate = DateTime.Now.AddMonths(2) },
                new Milestone { Id = "m3", Name = "Third", TargetDate = DateTime.Now.AddMonths(3) }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones));

            var markupIndex1 = component.Markup.IndexOf("First");
            var markupIndex2 = component.Markup.IndexOf("Second");
            var markupIndex3 = component.Markup.IndexOf("Third");

            Assert.True(markupIndex1 < markupIndex2 && markupIndex2 < markupIndex3);
        }
    }
}