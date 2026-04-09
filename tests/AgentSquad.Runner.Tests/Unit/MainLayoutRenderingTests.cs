using Xunit;
using Microsoft.AspNetCore.Components;
using Bunit;

namespace AgentSquad.Runner.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class MainLayoutRenderingTests : TestContext
    {
        [Fact]
        public void MainLayout_ContainsMetaCharset_UTF8()
        {
            var cut = RenderComponent<MainLayout>();
            var html = cut.Markup;

            Assert.Contains("charset=\"utf-8\"", html);
        }

        [Fact]
        public void MainLayout_ContainsBootstrapCSS_FromCDN()
        {
            var cut = RenderComponent<MainLayout>();
            var html = cut.Markup;

            Assert.Contains("bootstrap@5.3.0", html);
            Assert.Contains(".css", html);
        }

        [Fact]
        public void MainLayout_ContainsChartJS_FromCDN()
        {
            var cut = RenderComponent<MainLayout>();
            var html = cut.Markup;

            Assert.Contains("chart.js@4.4.0", html);
        }

        [Fact]
        public void MainLayout_ContainsBlazorServerScript_Required()
        {
            var cut = RenderComponent<MainLayout>();
            var html = cut.Markup;

            Assert.Contains("blazor.server.js", html);
        }

        [Fact]
        public void MainLayout_HasLanguageAttribute_EnglishUS()
        {
            var cut = RenderComponent<MainLayout>();
            var html = cut.Markup;

            Assert.Contains("lang=\"en\"", html);
        }

        [Fact]
        public void MainLayout_ContainsCustomAppCSS_Reference()
        {
            var cut = RenderComponent<MainLayout>();
            var html = cut.Markup;

            Assert.Contains("app.css", html);
        }

        [Fact]
        public void MainLayout_ContainsBootstrapBundle_NotSeparate()
        {
            var cut = RenderComponent<MainLayout>();
            var html = cut.Markup;

            Assert.Contains("bootstrap.bundle.min.js", html);
        }

        [Fact]
        public void MainLayout_ContainsViewportMeta_ForResponsiveness()
        {
            var cut = RenderComponent<MainLayout>();
            var html = cut.Markup;

            Assert.Contains("viewport", html);
            Assert.Contains("width=device-width", html);
        }

        [Fact]
        public void MainLayout_PageTitle_SetToAgentSquadDashboard()
        {
            var cut = RenderComponent<MainLayout>();
            var html = cut.Markup;

            Assert.Contains("AgentSquad - Executive Dashboard", html);
        }

        [Fact]
        public void MainLayout_Body_RenderedInContainer()
        {
            var cut = RenderComponent<MainLayout>();
            var html = cut.Markup;

            Assert.Contains("container-fluid", html);
        }
    }
}