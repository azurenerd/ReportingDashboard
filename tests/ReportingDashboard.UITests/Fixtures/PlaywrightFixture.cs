using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests.Fixtures;

public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public string BaseUrl { get; }

    public PlaywrightFixture()
    {
        BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();

        var headed = Environment.GetEnvironmentVariable("HEADED");
        var headless = string.IsNullOrEmpty(headed) || headed == "0" || headed.Equals("false", StringComparison.OrdinalIgnoreCase);

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = headless
        });
    }

    public async Task<IPage> NewPageAsync()
    {
        if (_browser is null)
            throw new InvalidOperationException("Browser not initialized. Ensure InitializeAsync has been called.");

        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            IgnoreHTTPSErrors = true
        });

        return await context.NewPageAsync();
    }

    public async Task CaptureScreenshotAsync(IPage page, string testName)
    {
        var screenshotDir = Path.Combine(AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(screenshotDir);

        var sanitized = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));
        var path = Path.Combine(screenshotDir, $"{sanitized}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = path,
            FullPage = true
        });
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
            await _browser.DisposeAsync();

        _playwright?.Dispose();
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}