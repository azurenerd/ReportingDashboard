using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;

namespace AgentSquad.Tests.Components
{
    public class MilestoneTimelineComponentTests : TestContext
    {
        [Fact]
        public void MilestoneTimelineComponent_Renders()
        {
            var component = RenderComponent<MilestoneTimeline>();
            Assert.NotNull(component);
        }

        [Fact]
        public void MilestoneTimelineComponent_DisplaysTimeline()
        {
            var component = RenderComponent<MilestoneTimeline>();
            Assert.NotEmpty(component.Markup);
        }

        [Fact]
        public void MilestoneTimelineComponent_WithEmptyMilestones_Renders()
        {
            var component = RenderComponent<MilestoneTimeline>();
            Assert.NotNull(component);
        }
    }
}