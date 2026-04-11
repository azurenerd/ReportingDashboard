using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests.Infrastructure;

public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public string BaseUrl { get; } =
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();

        var headed = Environment.GetEnvironmentVariable("HEADED") == "1";
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !headed
        });
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
            await _browser.CloseAsync();

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
        var screenshotDir = Path.Combine(
            AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(screenshotDir);

        var fileName = $"{testName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
        var filePath = Path.Combine(screenshotDir, fileName);

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = filePath,
            FullPage = true
        });
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}