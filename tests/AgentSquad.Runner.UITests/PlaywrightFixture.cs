using Microsoft.Playwright;
using Xunit;
using System;
using System.Threading.Tasks;

namespace AgentSquad.Runner.UITests
{
    public class PlaywrightFixture : IAsyncLifetime
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IBrowserContext _context;
        public string BaseUrl { get; private set; }
        public bool Headless { get; private set; }

        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();
            
            BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5050";
            Headless = !bool.TryParse(Environment.GetEnvironmentVariable("HEADED"), out var headed) || !headed;

            _browser = await _playwright.Chromium.LaunchAsync(new BrowserLaunchOptions
            {
                Headless = Headless
            });

            _context = await _browser.NewContextAsync();
        }

        public async Task DisposeAsync()
        {
            if (_context != null)
                await _context.CloseAsync();
            if (_browser != null)
                await _browser.CloseAsync();
            _playwright?.Dispose();
        }

        public async Task<IPage> NewPageAsync()
        {
            return await _context.NewPageAsync();
        }

        public async Task CaptureScreenshotAsync(IPage page, string fileName)
        {
            var screenshotPath = Path.Combine("screenshots", fileName);
            Directory.CreateDirectory("screenshots");
            await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
        }
    }
}