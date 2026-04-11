using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests.Infrastructure;

/// <summary>
/// Shared Playwright fixture for all UI tests. Manages browser lifecycle.
/// Set BASE_URL env var to override default (http://localhost:5000).
/// Set HEADED=1 to run in headed mode.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public string BaseUrl { get; } =
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

    public string ScreenshotDir { get; } =
        Environment.GetEnvironmentVariable("SCREENSHOT_DIR")
        ?? Path.Combine(Path.GetTempPath(), "playwright-screenshots");

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();

        var headless = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HEADED"));

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = headless
        });

        Directory.CreateDirectory(ScreenshotDir);
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
            await _browser.CloseAsync();
        _playwright?.Dispose();
    }

    /// <summary>
    /// Creates a new page with 1920x1080 viewport matching the dashboard's fixed layout.
    /// </summary>
    public async Task<IPage> NewPageAsync()
    {
        var context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            IgnoreHTTPSErrors = true
        });
        return await context.NewPageAsync();
    }

    /// <summary>
    /// Alias for NewPageAsync for backward compatibility.
    /// </summary>
    public Task<IPage> CreatePageAsync() => NewPageAsync();

    /// <summary>
    /// Captures a screenshot on test failure for debugging.
    /// </summary>
    public async Task CaptureScreenshotAsync(IPage page, string testName)
    {
        var sanitized = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));
        var path = Path.Combine(ScreenshotDir, $"{sanitized}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }