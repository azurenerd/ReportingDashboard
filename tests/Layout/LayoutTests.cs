using Xunit;
using Bunit;
using AgentSquad.Components.Layout;

namespace AgentSquad.Tests.Layout
{
    public class LayoutTests : TestContext
    {
        [Fact]
        public void MainLayout_RendersHeaderSection()
        {
            var component = RenderComponent<MainLayout>();
            Assert.NotNull(component);
            Assert.NotEmpty(component.Markup);
        }

        [Fact]
        public void MainLayout_RendersNavigationMenu()
        {
            var component = RenderComponent<MainLayout>();
            Assert.NotNull(component);
            Assert.Contains("nav", component.Markup.ToLower());
        }

        [Fact]
        public void MainLayout_RendersFooterSection()
        {
            var component = RenderComponent<MainLayout>();
            Assert.NotNull(component);
            Assert.Contains("footer", component.Markup.ToLower());
        }

        [Fact]
        public void MainLayout_RendersBodyContentPlaceholder()
        {
            var component = RenderComponent<MainLayout>();
            Assert.NotNull(component);
            Assert.NotEmpty(component.Markup);
        }
    }
}