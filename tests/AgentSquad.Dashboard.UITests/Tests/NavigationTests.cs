using Xunit;
using AgentSquad.Dashboard.UITests.Fixtures;

namespace AgentSquad.Dashboard.UITests.Tests
{
    [Collection("Playwright")]
    [Trait("Category", "UI")]
    public class NavigationTests : IAsyncLifetime
    {
        private readonly PlaywrightFixture _fixture;
        private Microsoft.Playwright.IPage? _page;

        public NavigationTests(PlaywrightFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            _page = await _fixture.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            if (_page != null)
            {
                await _page.CloseAsync();
            }
        }

        [Fact]
        public async Task Navigation_RootPathLoadsDashboard()
        {
            await _fixture.NavigateToAsync(_page!, "/");
            await _fixture.WaitForLoadingAsync(_page!);

            var title = await _page!.GetTitleAsync();
            Assert.NotEmpty(title);
        }

        [Fact]
        public async Task Navigation_InvalidPathShowsNotFound()
        {
            await _fixture.NavigateToAsync(_page!, "/nonexistent-page");
            await _page!.WaitForTimeoutAsync(500);

            var content = await _page!.GetTextContentAsync("body");
            Assert.Contains("404", content) | Assert.Contains("nothing", content);
        }

        [Fact]
        public async Task Navigation_FooterIsPresent()
        {
            await _fixture.NavigateToAsync(_page!, "/");
            await _fixture.WaitForLoadingAsync(_page!);

            var footer = await _page!.QuerySelectorAsync("footer");
            Assert.NotNull(footer);
        }

        [Fact]
        public async Task Navigation_PageHasCorrectDoctype()
        {
            await _fixture.NavigateToAsync(_page!, "/");
            await _fixture.WaitForLoadingAsync(_page!);

            var content = await _page!.ContentAsync();
            Assert.StartsWith("<!DOCTYPE html>", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Navigation_PageIsResponsive()
        {
            var context = await _fixture._browser!.NewContextAsync(new Microsoft.Playwright.BrowserNewContextOptions
            {
                ViewportSize = new Microsoft.Playwright.ViewportSize { Width = 1024, Height = 768 }
            });
            var page = await context.NewPageAsync();

            await _fixture.NavigateToAsync(page, "/");
            await _fixture.WaitForLoadingAsync(page);

            var containerFluid = await page.QuerySelectorAsync(".container-fluid");
            Assert.NotNull(containerFluid);

            await page.CloseAsync();
            await context.CloseAsync();
        }
    }
}