using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;

namespace ReportingDashboard.UITests.Infrastructure;

/// <summary>
/// Shared fixture that manages a Playwright browser instance and test web server.
/// Usage: Implement IClassFixture&lt;PlaywrightFixture&gt; on test classes.
/// Prerequisites: Run "pwsh bin/Debug/net8.0/playwright.ps1 install" before first use.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private WebApplicationFactory<Program>? _factory;

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");
    public WebApplicationFactory<Program> Factory => _factory ?? throw new InvalidOperationException("Factory not initialized. Call InitializeAsync first.");
    public string BaseUrl { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // Start the test web server
        _factory = new WebApplicationFactory<Program>();
        var client = _factory.CreateClient();
        BaseUrl = client.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:5000";
        client.Dispose();

        // Initialize Playwright and launch browser
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    /// <summary>
    /// Creates a new browser page with a 1920x1080 viewport matching the dashboard design.
    /// </summary>
    public async Task<IPage> CreatePageAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        return await context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
            await _browser.DisposeAsync();

        _playwright?.Dispose();
        _factory?.Dispose();
    }
}