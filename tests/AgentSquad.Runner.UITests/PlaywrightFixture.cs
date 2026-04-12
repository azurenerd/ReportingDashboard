using Microsoft.Playwright;
using Xunit;

namespace AgentSquad.Runner.UITests;

public class PlaywrightFixture : IAsyncLifetime
{
    private IBrowser? _browser;
    public IPage? Page { get; private set; }
    public string BaseUrl { get; }

    public PlaywrightFixture()
    {
        BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";
    }

    public async Task InitializeAsync()
    {
        var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        Page = await _browser.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (Page != null)
        {
            await Page.CloseAsync();
        }

        if (_browser != null)
        {
            await _browser.CloseAsync();
        }
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}