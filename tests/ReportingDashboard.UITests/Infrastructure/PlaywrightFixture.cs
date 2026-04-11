using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests.Infrastructure;

public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public string BaseUrl { get; } =
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialized");

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();

        var headed = Environment.GetEnvironmentVariable("HEADED") == "1";

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !headed,
            Timeout = 30000
        });
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
            await _browser.DisposeAsync();
        _playwright?.Dispose();
    }

    public async Task<IPage> NewPageAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            IgnoreHTTPSErrors = true
        });
        return await context.NewPageAsync();
    }

    public async Task CaptureScreenshotAsync(IPage page, string name)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}