using System.Diagnostics;
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
    private Process? _appProcess;

    public async Task InitializeAsync()
    {
        if (Environment.GetEnvironmentVariable("BASE_URL") == null)
        {
            _appProcess = StartApp();
            await WaitForAppAsync();
        }

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

        if (_appProcess != null && !_appProcess.HasExited)
        {
            _appProcess.Kill(entireProcessTree: true);
            _appProcess.Dispose();
        }
    }

    private Process StartApp()
    {
        var repoRoot = FindRepoRoot();
        var projectPath = Path.Combine(repoRoot, "ReportingDashboard");

        var psi = new ProcessStartInfo("dotnet", "run --no-build --urls http://localhost:5000")
        {
            WorkingDirectory = projectPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start the application process.");

        return process;
    }

    private async Task WaitForAppAsync()
    {
        using var client = new HttpClient();
        var deadline = DateTime.UtcNow.AddSeconds(60);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync(BaseUrl);
                if (response.IsSuccessStatusCode || (int)response.StatusCode < 500)
                    return;
            }
            catch
            {
                // not ready yet
            }
            await Task.Delay(500);
        }

        throw new TimeoutException("Application did not start within 60 seconds.");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (dir.GetFiles("*.sln").Length > 0)
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not find solution root.");
    }
}