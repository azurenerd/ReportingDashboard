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
                new Milestone { Id = "m1", Name = "Q1", DueDate = DateTime.Now.AddMonths(1) },
                new Milestone { Id = "m2", Name = "Q2", DueDate = DateTime.Now.AddMonths(4) }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters => 
                parameters.Add(p => p.Milestones, milestones));

            Assert.NotNull(component);
            Assert.Contains("Q1", component.Markup);
        }

        [Fact]
        public void MilestoneTimeline_WithEmptyList_RendersWithoutError()
        {
            var milestones = new List<Milestone>();
            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones));

            Assert.NotNull(component);
        }

        [Fact]
        public void MilestoneTimeline_WithNullMilestones_HandlesGracefully()
        {
            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, (List<Milestone>)null!));

            Assert.NotNull(component);
        }

        [Fact]
        public void MilestoneTimeline_WithMultipleMilestones_DisplaysAllDates()
        {
            var milestones = new List<Milestone>
            {
                new Milestone { Id = "m1", Name = "Phase 1", DueDate = DateTime.Now },
                new Milestone { Id = "m2", Name = "Phase 2", DueDate = DateTime.Now.AddMonths(3) }
            };

            var component = RenderComponent<MilestoneTimeline>(parameters =>
                parameters.Add(p => p.Milestones, milestones));

            Assert.Contains("Phase 1", component.Markup);
            Assert.Contains("Phase 2", component.Markup);
        }
    }
}