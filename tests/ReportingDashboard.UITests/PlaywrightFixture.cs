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
    public string BaseUrl { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var port = GetAvailablePort();
        BaseUrl = $"http://localhost:{port}";

        var projectDir = FindProjectDirectory();

        // Pre-build so that `dotnet run --no-build` starts quickly and doesn't fail silently
        var buildProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "build -c Debug --nologo -v q",
            WorkingDirectory = projectDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        });
        if (buildProcess is not null)
        {
            await buildProcess.WaitForExitAsync();
            if (buildProcess.ExitCode != 0)
            {
                var stderr = await buildProcess.StandardError.ReadToEndAsync();
                throw new InvalidOperationException($"dotnet build failed (exit {buildProcess.ExitCode}): {stderr}");
            }
        }

        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --no-build --no-launch-profile",
                WorkingDirectory = projectDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        // Set environment variables on the existing dictionary (preserves PATH, DOTNET_ROOT, etc.)
        _serverProcess.StartInfo.EnvironmentVariables["ASPNETCORE_URLS"] = BaseUrl;
        _serverProcess.StartInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
        _serverProcess.StartInfo.EnvironmentVariables["DOTNET_NOLOGO"] = "1";

        _serverProcess.Start();

        // Consume stdout/stderr to prevent buffer deadlock
        _ = Task.Run(async () =>
        {
            try { await _serverProcess.StandardOutput.ReadToEndAsync(); } catch { }
        });
        _ = Task.Run(async () =>
        {
            try { await _serverProcess.StandardError.ReadToEndAsync(); } catch { }
        });

        // Wait for the server to be ready
        await WaitForServerAsync(BaseUrl, TimeSpan.FromSeconds(90));

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

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string FindProjectDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "ReportingDashboard");
            if (Directory.Exists(candidate) &&
                File.Exists(Path.Combine(candidate, "ReportingDashboard.csproj")))
            {
                return candidate;
            }

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
            $"Could not find ReportingDashboard project directory. Searched upward from {AppContext.BaseDirectory}");
    }

    private static async Task WaitForServerAsync(string url, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        using var handler = new HttpClientHandler { AllowAutoRedirect = true };
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };

        while (!cts.Token.IsCancellationRequested)
        {
            try
            {
                var response = await client.GetAsync(url, cts.Token);
                // Blazor Server returns 200 when ready (even if page content loads via SignalR)
                if ((int)response.StatusCode < 500)
                {
                    // Give Blazor a moment to fully initialize its SignalR hub
                    await Task.Delay(1000, cts.Token);
                    return;
                }
            }
            catch (Exception) when (!cts.Token.IsCancellationRequested)
            {
                // Server not ready yet - connection refused, timeout, etc.
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