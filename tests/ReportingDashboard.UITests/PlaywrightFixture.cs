using System.Diagnostics;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; private set; } = null!;
    private Process? _serverProcess;
    private bool _weStartedServer;

    public async Task InitializeAsync()
    {
        BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

        // Start the web server if it isn't already running
        if (!await IsServerReachableAsync())
        {
            await StartWebServerAsync();
            _weStartedServer = true;
        }

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.DisposeAsync();
        }
        Playwright?.Dispose();

        if (_weStartedServer && _serverProcess is not null)
        {
            try
            {
                if (!_serverProcess.HasExited)
                    _serverProcess.Kill(entireProcessTree: true);
            }
            catch { /* best-effort cleanup */ }
            _serverProcess.Dispose();
            _serverProcess = null;
        }
    }

    public async Task<IPage> CreatePageAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        return page;
    }

    private async Task<bool> IsServerReachableAsync()
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        try
        {
            await client.GetAsync(BaseUrl);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task StartWebServerAsync()
    {
        var solutionDir = FindSolutionDirectory(AppContext.BaseDirectory)
            ?? throw new InvalidOperationException(
                $"Cannot find .sln file searching upward from {AppContext.BaseDirectory}");

        var webCsproj = Path.Combine(solutionDir, "ReportingDashboard.Web", "ReportingDashboard.Web.csproj");
        if (!File.Exists(webCsproj))
            throw new FileNotFoundException($"Web project not found at {webCsproj}");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{webCsproj}\" --urls \"{BaseUrl}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        _serverProcess = new Process { StartInfo = startInfo };
        _serverProcess.Start();

        // Drain stdout/stderr asynchronously to prevent deadlocks
        _serverProcess.BeginOutputReadLine();
        _serverProcess.BeginErrorReadLine();

        // Poll until the server responds or timeout
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        for (var attempt = 0; attempt < 60; attempt++)
        {
            await Task.Delay(1000);

            if (_serverProcess.HasExited)
                throw new InvalidOperationException(
                    $"Web server process exited with code {_serverProcess.ExitCode} before becoming ready");

            try
            {
                var response = await client.GetAsync(BaseUrl);
                return; // Any HTTP response means the server is up
            }
            catch (Exception) when (attempt < 59)
            {
                // Not ready yet, keep waiting
            }
        }

        throw new TimeoutException($"Web server did not become reachable at {BaseUrl} within 60 seconds");
    }

    private static string? FindSolutionDirectory(string startPath)
    {
        var dir = new DirectoryInfo(startPath);
        while (dir is not null)
        {
            if (dir.GetFiles("*.sln").Length > 0)
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }