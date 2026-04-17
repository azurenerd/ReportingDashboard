using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright PlaywrightInstance { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; private set; } = null!;
    public bool ServerAvailable { get; private set; }

    public async Task InitializeAsync()
    {
        BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

        // Probe server before launching browser
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        try
        {
            await http.GetAsync(BaseUrl);
            ServerAvailable = true;
        }
        catch
        {
            ServerAvailable = false;
        }

        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Timeout = 60000
        });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null) await Browser.DisposeAsync();
        PlaywrightInstance?.Dispose();
    }

    public void EnsureServerAvailable()
    {
        if (!ServerAvailable)
            throw new SkipException($"Dashboard server not running at {BaseUrl}. Start with 'dotnet run' before running UI tests.");
    }
}

/// <summary>
/// Custom exception that xUnit treats as a skipped test.
/// xUnit 2.6+ recognizes exceptions with "$XunitDynamicSkip" in the message.
/// </summary>
public class SkipException : Exception
{
    public SkipException(string reason)
        : base($"$XunitDynamicSkip${reason}")
    {
    }
}