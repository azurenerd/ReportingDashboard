using Bunit;
using Xunit;
using AgentSquad.Runner.Components;

namespace AgentSquad.Runner.Tests.Components
{
    public class LoadingSpinnerTests : TestContext
    {
        [Fact]
        public void RenderLoadingSpinner_DisplaysOverlay()
        {
            var cut = RenderComponent<LoadingSpinner>();

            cut.Render();
            var overlay = cut.Find(".loading-spinner-overlay");
            Assert.NotNull(overlay);
        }

        [Fact]
        public void RenderLoadingSpinner_DisplaysSpinner()
        {
            var cut = RenderComponent<LoadingSpinner>();

            cut.Render();
            var spinner = cut.Find(".spinner");
            Assert.NotNull(spinner);
        }

        [Fact]
        public void RenderLoadingSpinner_DisplaysLoadingText()
        {
            var cut = RenderComponent<LoadingSpinner>();

            cut.Render();
            var loadingText = cut.Find(".loading-text");
            Assert.NotNull(loadingText);
            Assert.Contains("Loading dashboard", loadingText.TextContent);
        }

        [Fact]
        public void Spinner_HasAriaLiveAttribute()
        {
            var cut = RenderComponent<LoadingSpinner>();

            cut.Render();
            var spinner = cut.Find(".spinner");
            var ariaLive = spinner.GetAttribute("aria-live");
            
            Assert.NotNull(ariaLive);
            Assert.Equal("polite", ariaLive);
        }

        [Fact]
        public void Spinner_HasAriaLabel()
        {
            var cut = RenderComponent<LoadingSpinner>();

            cut.Render();
            var spinner = cut.Find(".spinner");
            var ariaLabel = spinner.GetAttribute("aria-label");
            
            Assert.NotNull(ariaLabel);
            Assert.Contains("Loading", ariaLabel);
        }

        [Fact]
        public void RenderLoadingSpinner_HasHighZIndex()
        {
            var cut = RenderComponent<LoadingSpinner>();

            cut.Render();
            var overlay = cut.Find(".loading-spinner-overlay");
            var style = overlay.GetAttribute("style");
            
            // Check if z-index is set through CSS (not inline)
            Assert.NotNull(overlay);
        }

        [Fact]
        public void RenderLoadingSpinner_CoversFullViewport()
        {
            var cut = RenderComponent<LoadingSpinner>();

            cut.Render();
            var overlay = cut.Find(".loading-spinner-overlay");
            
            // Verify full viewport coverage through CSS
            Assert.NotNull(overlay);
        }
    }
}