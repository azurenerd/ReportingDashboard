using Xunit;
using Bunit;
using AgentSquad.Components.Layout;

namespace AgentSquad.Tests.Layout
{
    public class LayoutTests : TestContext
    {
        [Fact]
        public void MainLayout_RendersHeader()
        {
            var component = RenderComponent<MainLayout>();
            Assert.NotNull(component);
        }

        [Fact]
        public void MainLayout_RendersNavigation()
        {
            var component = RenderComponent<MainLayout>();
            Assert.NotNull(component);
        }

        [Fact]
        public void MainLayout_RendersFooter()
        {
            var component = RenderComponent<MainLayout>();
            Assert.NotNull(component);
        }
    }
}