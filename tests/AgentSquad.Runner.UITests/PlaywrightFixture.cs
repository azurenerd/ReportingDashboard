using Microsoft.Playwright;
using Xunit;

namespace AgentSquad.Runner.UITests;

/// <summary>
/// Fixture for managing Playwright browser and page lifecycle across UI tests.
/// Provides a shared browser context and page for all tests in the collection.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    private IBrowser? _browser;
    private IBrowserContext? _context;
    public IPage? Page { get; private set; }
    public string BaseUrl { get; private set; }

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

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });

        Page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (Page != null)
            await Page.CloseAsync();

        if (_context != null)
            await _context.CloseAsync();

        if (_browser != null)
            await _browser.CloseAsync();
    }
}

/// <summary>
/// Collection definition for Playwright tests to ensure fixture is shared across tests.
/// </summary>
[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}