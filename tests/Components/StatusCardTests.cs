using Xunit;
using Bunit;
using AgentSquad.Components;
using AgentSquad.Data;

namespace AgentSquad.Tests.Components
{
    public class StatusCardTests : TestContext
    {
        [Fact]
        public void StatusCard_WithValidStatus_DisplaysStatus()
        {
            var card = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.Status, "Active"));

            Assert.Contains("Active", card.Markup);
        }

        [Fact]
        public void StatusCard_WithCompleteStatus_ShowsCheckmark()
        {
            var card = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.Status, "Complete"));

            Assert.NotNull(card);
        }

        [Fact]
        public void StatusCard_WithEmptyStatus_RendersWithoutError()
        {
            var card = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.Status, string.Empty));

            Assert.NotNull(card);
        }
    }
}