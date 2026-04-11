using Microsoft.Playwright;
using Xunit;

namespace Project.UITests.Infrastructure;

/// <summary>
/// Shared Playwright fixture that manages browser lifecycle.
/// Runs headless by default. Captures screenshots on failure,
/// records video and traces when configured via environment variables.
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

    public async Task<(IPage Page, IBrowserContext Context)> NewPageWithContextAsync(string? testName = null)
    {
        var contextOptions = new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            IgnoreHTTPSErrors = true
        };

        // Enable video recording if PWVIDEO_DIR is set
        var videoDir = Environment.GetEnvironmentVariable("PWVIDEO_DIR");
        if (!string.IsNullOrEmpty(videoDir))
        {
            Directory.CreateDirectory(videoDir);
            contextOptions.RecordVideoDir = videoDir;
            contextOptions.RecordVideoSize = new RecordVideoSize { Width = 1280, Height = 720 };
        }

        var context = await Browser.NewContextAsync(contextOptions);

        // Enable tracing if PWTRACE_DIR is set
        var traceDir = Environment.GetEnvironmentVariable("PWTRACE_DIR");
        if (!string.IsNullOrEmpty(traceDir))
        {
            Directory.CreateDirectory(traceDir);
            await context.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }

        return (await context.NewPageAsync(), context);
    }

    public async Task<IPage> NewPageAsync()
    {
        var (page, _) = await NewPageWithContextAsync();
        return page;
    }

    /// <summary>Stops tracing and saves the trace file. Call in test cleanup.</summary>
    public static async Task StopTracingAsync(IBrowserContext context, string testName)
    {
        var traceDir = Environment.GetEnvironmentVariable("PWTRACE_DIR");
        if (string.IsNullOrEmpty(traceDir)) return;
        var tracePath = Path.Combine(traceDir, $"{testName}.zip");
        await context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath });
    }

    public static async Task CaptureScreenshotAsync(IPage page, string testName)
    {
        var resultsDir = Environment.GetEnvironmentVariable("PLAYWRIGHT_TEST_RESULTS_DIR") ?? "TestResults";
        var screenshotDir = Path.Combine(resultsDir, "screenshots");
        Directory.CreateDirectory(screenshotDir);
        var path = Path.Combine(screenshotDir, $"{testName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }
