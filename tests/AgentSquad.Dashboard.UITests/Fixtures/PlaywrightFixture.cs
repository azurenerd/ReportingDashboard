using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace AgentSquad.Dashboard.UITests.Fixtures
{
    public class PlaywrightFixture : IAsyncLifetime
    {
        private IBrowser? _browser;
        private string? _baseUrl;
        private bool _headless = true;

        public async Task InitializeAsync()
        {
            var playwright = await Playwright.CreateAsync();
            _baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
            var headed = Environment.GetEnvironmentVariable("HEADED");
            _headless = string.IsNullOrEmpty(headed) || headed != "true";

            _browser = await playwright.Chromium.LaunchAsync(new BrowserLaunchOptions
            {
                Headless = _headless,
                SlowMo = 50
            });
        }

        public async Task DisposeAsync()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
            }
        }

        public async Task<IPage> NewPageAsync()
        {
            var context = await _browser!.NewContextAsync();
            return await context.NewPageAsync();
        }

        public string BaseUrl => _baseUrl ?? "http://localhost:5000";

        public async Task<IPage> NavigateToAsync(IPage page, string path)
        {
            await page.GotoAsync($"{BaseUrl}{path}");
            return page;
        }

        public async Task CaptureScreenshotAsync(IPage page, string fileName)
        {
            var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
            Directory.CreateDirectory(screenshotDir);
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(screenshotDir, $"{fileName}-{DateTime.Now:yyyyMMdd-HHmmss}.png")
            });
        }

        public async Task WaitForLoadingAsync(IPage page)
        {
            try
            {
                await page.WaitForSelectorAsync(".spinner-border", new PageWaitForSelectorOptions { Timeout = 5000 });
                await page.WaitForSelectorAsync(".spinner-border", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
            }
            catch
            {
            }
        }
    }
}