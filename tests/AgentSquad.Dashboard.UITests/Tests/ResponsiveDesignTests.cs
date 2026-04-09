using Xunit;
using AgentSquad.Dashboard.UITests.Fixtures;

namespace AgentSquad.Dashboard.UITests.Tests
{
    [Collection("Playwright")]
    [Trait("Category", "UI")]
    public class ResponsiveDesignTests : IAsyncLifetime
    {
        private readonly PlaywrightFixture _fixture;

        public ResponsiveDesignTests(PlaywrightFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await Task.CompletedTask;
        }

        [Fact]
        public async Task ResponsiveDesign_DesktopLayout1920x1080()
        {
            var context = await _fixture._browser!.NewContextAsync(new Microsoft.Playwright.BrowserNewContextOptions
            {
                ViewportSize = new Microsoft.Playwright.ViewportSize { Width = 1920, Height = 1080 }
            });
            var page = await context.NewPageAsync();

            await _fixture.NavigateToAsync(page, "/");
            await _fixture.WaitForLoadingAsync(page);
            await page.WaitForSelectorAsync(".container-fluid", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var content = await page.ContentAsync();
            Assert.NotEmpty(content);

            await page.CloseAsync();
            await context.CloseAsync();
        }

        [Fact]
        public async Task ResponsiveDesign_LaptopLayout1366x768()
        {
            var context = await _fixture._browser!.NewContextAsync(new Microsoft.Playwright.BrowserNewContextOptions
            {
                ViewportSize = new Microsoft.Playwright.ViewportSize { Width = 1366, Height = 768 }
            });
            var page = await context.NewPageAsync();

            await _fixture.NavigateToAsync(page, "/");
            await _fixture.WaitForLoadingAsync(page);
            await page.WaitForSelectorAsync(".container-fluid", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var content = await page.ContentAsync();
            Assert.NotEmpty(content);

            await page.CloseAsync();
            await context.CloseAsync();
        }

        [Fact]
        public async Task ResponsiveDesign_MinimumWidth1024x768()
        {
            var context = await _fixture._browser!.NewContextAsync(new Microsoft.Playwright.BrowserNewContextOptions
            {
                ViewportSize = new Microsoft.Playwright.ViewportSize { Width = 1024, Height = 768 }
            });
            var page = await context.NewPageAsync();

            await _fixture.NavigateToAsync(page, "/");
            await _fixture.WaitForLoadingAsync(page);
            await page.WaitForSelectorAsync(".container-fluid", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var content = await page.ContentAsync();
            Assert.NotEmpty(content);

            await page.CloseAsync();
            await context.CloseAsync();
        }
    }
}