using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }

public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright? Playwright { get; private set; }
    public IBrowser? Browser { get; private set; }
    public bool BrowserAvailable { get; private set; }
    public string BaseUrl { get; } = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5050";

    public async Task InitializeAsync()
    {
        try
        {
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
            BrowserAvailable = true;
        }
        catch
        {
            BrowserAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
            await Browser.CloseAsync();
        Playwright?.Dispose();
    }
}