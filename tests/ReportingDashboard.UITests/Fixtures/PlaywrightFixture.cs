using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests.Fixtures;

public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        var headed = Environment.GetEnvironmentVariable("HEADED") == "1";

        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !headed
        });
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
        var screenshotDir = Path.Combine(
            AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(screenshotDir);

        var fileName = $"{name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(screenshotDir, fileName),
            FullPage = true
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}