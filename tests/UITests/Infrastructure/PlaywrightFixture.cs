using Microsoft.Playwright;
using Xunit;

namespace Project.UITests.Infrastructure;

/// <summary>
/// Shared Playwright fixture that manages browser lifecycle.
/// Runs headless by default. Captures screenshots on test failure.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public string BaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Environment.GetEnvironmentVariable("HEADED") != "1"
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }

    public async Task<IPage> NewPageAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            IgnoreHTTPSErrors = true
        });
        return await context.NewPageAsync();
    }

    public static async Task CaptureScreenshotAsync(IPage page, string testName)
    {
        var screenshotDir = Path.Combine("TestResults", "screenshots");
        Directory.CreateDirectory(screenshotDir);
        var path = Path.Combine(screenshotDir, $"{testName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }
