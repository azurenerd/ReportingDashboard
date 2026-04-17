using System.Diagnostics;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; private set; } = null!;
    private Process? _appProcess;

    public async Task InitializeAsync()
    {
        BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

        // Only start the server if BASE_URL is not explicitly set (i.e., we use the default)
        if (Environment.GetEnvironmentVariable("BASE_URL") == null)
        {
            var projectDir = FindProjectDir();
            _appProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run --no-launch-profile --urls http://localhost:5000",
                    WorkingDirectory = projectDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Environment =
                    {
                        ["ASPNETCORE_ENVIRONMENT"] = "Development"
                    }
                }
            };
            _appProcess.Start();

            // Wait for the server to be ready
            using var httpClient = new HttpClient();
            var timeout = DateTime.UtcNow.AddSeconds(30);
            while (DateTime.UtcNow < timeout)
            {
                try
                {
                    var response = await httpClient.GetAsync(BaseUrl);
                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.OK)
                        break;
                }
                catch
                {
                    // Server not ready yet
                }
                await Task.Delay(500);
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

        if (_appProcess != null && !_appProcess.HasExited)
        {
            try
            {
                _appProcess.Kill(entireProcessTree: true);
                await _appProcess.WaitForExitAsync();
            }
            catch
            {
                // Best effort cleanup
            }
            _appProcess.Dispose();
        }
    }

    private static string FindProjectDir()
    {
        // Walk up from the test output directory to find the src project
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "src", "ReportingDashboard");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "ReportingDashboard.csproj")))
                return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new InvalidOperationException(
            "Could not find src/ReportingDashboard project directory. " +
            "Set the BASE_URL environment variable to point to a running instance.");
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }