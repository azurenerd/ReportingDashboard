using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests.Infrastructure;

public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialized. Ensure IAsyncLifetime.InitializeAsync has been called.");
    public string BaseUrl { get; }

    public PlaywrightFixture()
    {
        BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();

        var headed = Environment.GetEnvironmentVariable("HEADED") is "1" or "true";

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !headed,
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

    public async Task CaptureScreenshotAsync(IPage page, string testName)
    {
        var screenshotsDir = Path.Combine(AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(screenshotsDir);

        var safeName = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));
        var path = Path.Combine(screenshotsDir, $"{safeName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = path,
            FullPage = true
        });
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}