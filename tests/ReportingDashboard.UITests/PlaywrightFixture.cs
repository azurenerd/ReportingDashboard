using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

public class PlaywrightFixture : IAsyncLifetime
{
    private Process? _serverProcess;
    public IBrowser Browser { get; private set; } = null!;
    public IPlaywright PlaywrightInstance { get; private set; } = null!;
    public string BaseUrl { get; } = "http://localhost:5000";

    public async Task InitializeAsync()
    {
        // Start the Blazor Server app
        var projectDir = FindProjectDirectory();
        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --no-launch-profile",
                WorkingDirectory = projectDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Environment =
                {
                    ["ASPNETCORE_URLS"] = BaseUrl,
                    ["ASPNETCORE_ENVIRONMENT"] = "Development",
                    ["DOTNET_NOLOGO"] = "1"
                }
            }
        };

        _serverProcess.Start();

        // Discard stdout/stderr asynchronously to prevent buffer deadlock
        _ = _serverProcess.StandardOutput.ReadToEndAsync();
        _ = _serverProcess.StandardError.ReadToEndAsync();

        // Wait for the server to be ready
        await WaitForServerAsync(BaseUrl, TimeSpan.FromSeconds(60));

        // Initialize Playwright
        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
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

        PlaywrightInstance?.Dispose();

        if (_serverProcess is not null && !_serverProcess.HasExited)
        {
            try
            {
                _serverProcess.Kill(entireProcessTree: true);
                await _serverProcess.WaitForExitAsync();
            }
            catch
            {
                // Best effort cleanup
            }
            finally
            {
                _serverProcess.Dispose();
            }
        }
    }

    private static string FindProjectDirectory()
    {
        // Walk up from the test assembly location to find the ReportingDashboard project
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "ReportingDashboard");
            if (Directory.Exists(candidate) &&
                File.Exists(Path.Combine(candidate, "ReportingDashboard.csproj")))
            {
                return candidate;
            }

            // Also check if we're already at the solution root
            var slnFiles = dir.GetFiles("*.sln");
            if (slnFiles.Length > 0)
            {
                candidate = Path.Combine(dir.FullName, "ReportingDashboard");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            "Could not find ReportingDashboard project directory. " +
            $"Searched upward from {AppContext.BaseDirectory}");
    }

    private static async Task WaitForServerAsync(string url, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };

        while (!cts.Token.IsCancellationRequested)
        {
            try
            {
                var response = await client.GetAsync(url, cts.Token);
                if (response.IsSuccessStatusCode ||
                    response.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
            }
            catch (Exception) when (!cts.Token.IsCancellationRequested)
            {
                // Server not ready yet
            }

            try
            {
                await Task.Delay(500, cts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        throw new TimeoutException(
            $"Server at {url} did not become available within {timeout.TotalSeconds}s");
    }
}