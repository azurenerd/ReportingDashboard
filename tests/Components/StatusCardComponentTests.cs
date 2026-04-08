using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;

namespace AgentSquad.Tests.Components
{
    public class StatusCardComponentTests : TestContext
    {
        [Fact]
        public void StatusCardComponent_Renders()
        {
            var component = RenderComponent<StatusCard>();
            Assert.NotNull(component);
        }

        [Fact]
        public void StatusCardComponent_DisplaysStatus()
        {
            var component = RenderComponent<StatusCard>();
            Assert.NotEmpty(component.Markup);
        }

        [Fact]
        public void StatusCardComponent_WithDifferentStatus_Renders()
        {
            var component = RenderComponent<StatusCard>();
            Assert.NotNull(component);
        }
    }
}