using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.Web.UITests;

public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public string BaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5080";

    public async Task InitializeAsync()
    {
        Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Timeout = 60000
        });
    }

    public Task<IPage> NewPageAsync() => Browser.NewPageAsync();

    public async Task DisposeAsync()
    {
        if (Browser is not null) await Browser.CloseAsync();
        Playwright?.Dispose();
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }