using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; private set; } = null!;

    private Process? _serverProcess;
    private Process? _clientProcess;
    private bool _externalServer;

    public async Task InitializeAsync()
    {
        BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5173";
        _externalServer = Environment.GetEnvironmentVariable("BASE_URL") != null;

        // If no external BASE_URL was provided, start the dev servers ourselves
        if (!_externalServer)
        {
            var repoRoot = FindRepoRoot();
            if (repoRoot != null)
            {
                await StartDevServersAsync(repoRoot);
            }
        }

        // Wait for the app to be reachable
        await WaitForServerAsync(BaseUrl, TimeSpan.FromSeconds(30));

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();

        // Shut down dev servers if we started them
        StopProcess(_clientProcess);
        StopProcess(_serverProcess);
    }

    public async Task<IPage> NewPageAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        return page;
    }

    private static string? FindRepoRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "package.json")) &&
                Directory.Exists(Path.Combine(dir, "client")) &&
                Directory.Exists(Path.Combine(dir, "server")))
            {
                return dir;
            }
            // Also check if there's a nested structure
            if (File.Exists(Path.Combine(dir, "ReportingDashboard.sln")))
            {
                // Check if client/server dirs exist at this level
                if (Directory.Exists(Path.Combine(dir, "client")) &&
                    Directory.Exists(Path.Combine(dir, "server")))
                {
                    return dir;
                }
            }
            dir = Directory.GetParent(dir)?.FullName;
        }
        return null;
    }

    private async Task StartDevServersAsync(string repoRoot)
    {
        var npmCmd = OperatingSystem.IsWindows() ? "npm.cmd" : "npm";

        // Check if node_modules exists; if not, run npm install
        if (!Directory.Exists(Path.Combine(repoRoot, "node_modules")))
        {
            var install = Process.Start(new ProcessStartInfo
            {
                FileName = npmCmd,
                Arguments = "install",
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            if (install != null)
            {
                await install.WaitForExitAsync();
            }
        }

        // Start the backend server (Express on port 3001)
        var serverDir = Path.Combine(repoRoot, "server");
        if (Directory.Exists(serverDir))
        {
            var npxCmd = OperatingSystem.IsWindows() ? "npx.cmd" : "npx";
            _serverProcess = Process.Start(new ProcessStartInfo
            {
                FileName = npxCmd,
                Arguments = "tsx index.ts",
                WorkingDirectory = serverDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Environment = { ["PORT"] = "3001" },
            });
        }

        // Start the frontend dev server (Vite on port 5173)
        var clientDir = Path.Combine(repoRoot, "client");
        if (Directory.Exists(clientDir))
        {
            var npxCmd = OperatingSystem.IsWindows() ? "npx.cmd" : "npx";
            _clientProcess = Process.Start(new ProcessStartInfo
            {
                FileName = npxCmd,
                Arguments = "vite --port 5173",
                WorkingDirectory = clientDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }

        // If neither server started, try npm run dev from root
        if (_serverProcess == null && _clientProcess == null)
        {
            _serverProcess = Process.Start(new ProcessStartInfo
            {
                FileName = npmCmd,
                Arguments = "run dev",
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }
    }

    private static async Task WaitForServerAsync(string url, TimeSpan timeout)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(3);
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync(url);
                if (response.StatusCode != HttpStatusCode.ServiceUnavailable)
                {
                    return; // Server is up
                }
            }
            catch (HttpRequestException)
            {
                // Not yet available
            }
            catch (TaskCanceledException)
            {
                // Timeout on request
            }

            await Task.Delay(500);
        }

        // If server never came up, tests will fail with connection refused
        // which is acceptable — it means the dev environment isn't set up
    }

    private static void StopProcess(Process? process)
    {
        if (process == null || process.HasExited) return;
        try
        {
            process.Kill(entireProcessTree: true);
            process.Dispose();
        }
        catch
        {
            // Best effort cleanup
        }
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}