using System.Diagnostics;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

public class PlaywrightFixture : IAsyncLifetime
{
    private Process? _serverProcess;
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

        // Check if the server is already running (e.g., started by test harness)
        using var checkClient = new HttpClient();
        checkClient.Timeout = TimeSpan.FromSeconds(2);
        bool alreadyRunning = false;
        try
        {
            var resp = await checkClient.GetAsync(BaseUrl);
            alreadyRunning = true;
        }
        catch
        {
            alreadyRunning = false;
        }

        if (!alreadyRunning)
        {
            var webProjectDir = FindWebProjectDirectory();
            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run --no-launch-profile",
                    WorkingDirectory = webProjectDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };
            _serverProcess.StartInfo.Environment["ASPNETCORE_URLS"] = BaseUrl;
            _serverProcess.Start();
            _serverProcess.BeginOutputReadLine();
            _serverProcess.BeginErrorReadLine();

            // Wait for the server to become ready
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < TimeSpan.FromSeconds(90))
            {
                try
                {
                    var response = await httpClient.GetAsync(BaseUrl);
                    break;
                }
                catch (Exception)
                {
                    if (_serverProcess.HasExited)
                        throw new InvalidOperationException(
                            $"Web server process exited prematurely with code {_serverProcess.ExitCode}.");
                    await Task.Delay(1000);
                }
            }
        }

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();

        if (_serverProcess is { HasExited: false })
        {
            try { _serverProcess.Kill(entireProcessTree: true); }
            catch { /* best effort */ }
        }
        _serverProcess?.Dispose();
    }

    private static string FindWebProjectDirectory()
    {
        // Search from the test assembly output directory upward
        foreach (var startDir in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var dir = startDir;
            while (dir != null)
            {
                var candidate = Path.Combine(dir, "src", "ReportingDashboard.Web");
                if (File.Exists(Path.Combine(candidate, "ReportingDashboard.Web.csproj")))
                    return candidate;
                dir = Directory.GetParent(dir)?.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not find ReportingDashboard.Web project. " +
            $"Searched from: {AppContext.BaseDirectory} and {Directory.GetCurrentDirectory()}");
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}