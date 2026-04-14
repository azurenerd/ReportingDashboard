using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests.Infrastructure;

public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public IPlaywright Playwright => _playwright!;
    public IBrowser Browser => _browser!;
    public string BaseUrl { get; } = "http://localhost:5000";

    public async Task InitializeAsync()
    {
        try
        {
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }
        catch
        {
            // Playwright browsers may not be installed in CI/test environments.
            // Swallow so collection cleanup doesn't throw NullReferenceException.
        }
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }
        _playwright?.Dispose();
    }

    public async Task<IPage> CreatePageAsync()
    {
        if (_browser is null)
            throw new InvalidOperationException("Playwright browser is not available. Ensure browsers are installed.");

        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        return await context.NewPageAsync();
    }
}