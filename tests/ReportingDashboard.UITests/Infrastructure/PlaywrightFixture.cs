using System.Diagnostics;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests.Infrastructure;

[CollectionDefinition(PlaywrightCollection.Name)]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
    public const string Name = "Playwright";
}

/// <summary>
/// Shared Playwright fixture providing browser lifecycle and temp directory cleanup.
/// This is the single authoritative PlaywrightFixture for the solution — the orphaned
/// <c>tests/UITests/</c> duplicate has been removed per review feedback #2.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private Process? _serverProcess;
    private readonly List<string> _tempDirectories = [];

    public IBrowser Browser => _browser
        ?? throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");

    public string BaseUrl { get; private set; } = "http://localhost:5000";

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync();
            _browser = null;
        }

        _playwright?.Dispose();
        _playwright = null;

        if (_serverProcess is not null && !_serverProcess.HasExited)
        {
            _serverProcess.Kill(entireProcessTree: true);
            _serverProcess.Dispose();
            _serverProcess = null;
        }

        foreach (var dir in _tempDirectories)
        {
            try
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
            catch
            {
                // Best-effort cleanup
            }
        }
        _tempDirectories.Clear();
    }

    public async Task<IPage> CreatePageAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            IgnoreHTTPSErrors = true
        });
        return await context.NewPageAsync();
    }

    public void TrackTempDirectory(string path)
    {
        _tempDirectories.Add(path);
    }
}