using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests.Infrastructure;

/// <summary>
/// Shared Playwright fixture that manages browser lifecycle.
/// Uses [CollectionDefinition] to share a single browser instance across tests.
/// Set BASE_URL env var to override default (http://localhost:5000).
/// Set HEADED=1 to run with visible browser.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public string BaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialized");

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();

        var headed = Environment.GetEnvironmentVariable("HEADED") == "1";
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !headed,
            Timeout = 30_000
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

    /// <summary>Alias kept for backward compatibility with older test files.</summary>
    public Task<IPage> CreatePageAsync() => NewPageAsync();

    public async Task CaptureScreenshotAsync(IPage page, string testName)
    {
        var dir = Path.Combine(Path.GetTempPath(), "playwright-screenshots");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{testName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}