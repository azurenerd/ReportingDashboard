using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }

public class PlaywrightFixture : IAsyncLifetime
{
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; } = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

    private IPlaywright _playwright = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Timeout = 60000
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        _playwright.Dispose();
    }
}