using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright? Playwright { get; private set; }
    public IBrowser? Browser { get; private set; }
    public string BaseUrl { get; private set; } = null!;
    public bool BrowserAvailable { get; private set; }

    public async Task InitializeAsync()
    {
        BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5050";

        try
        {
            var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
            if (exitCode != 0)
            {
                Console.WriteLine($"[PlaywrightFixture] Browser install returned exit code {exitCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlaywrightFixture] Browser install failed: {ex.Message}");
        }

        try
        {
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
            BrowserAvailable = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlaywrightFixture] Browser not available: {ex.Message}");
            BrowserAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.DisposeAsync();
        }
        Playwright?.Dispose();
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}