using System.Diagnostics;
using System.Net;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright PlaywrightInstance { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; private set; } = null!;
    private Process? _serverProcess;

    public async Task InitializeAsync()
    {
        BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

        // Locate the main project directory relative to the test assembly output
        var assemblyDir = Path.GetDirectoryName(typeof(PlaywrightFixture).Assembly.Location)!;
        var repoRoot = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", ".."));
        var projectDir = Path.Combine(repoRoot, "ReportingDashboard");

        if (!Directory.Exists(projectDir))
        {
            // Fallback: try one level up from repo root
            projectDir = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", ".."));
            if (!File.Exists(Path.Combine(projectDir, "ReportingDashboard.csproj")))
            {
                throw new DirectoryNotFoundException(
                    $"Cannot find ReportingDashboard project directory. Assembly location: '{assemblyDir}'");
            }
        }

        // Start the web application as a child process
        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --no-launch-profile --urls \"{BaseUrl}\"",
                WorkingDirectory = projectDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        _serverProcess.StartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        _serverProcess.Start();

        // Wait for the server to be ready (up to 60 seconds for build + startup)
        using var httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        });
        httpClient.Timeout = TimeSpan.FromSeconds(5);

        var ready = false;
        for (var attempt = 0; attempt < 60; attempt++)
        {
            if (_serverProcess.HasExited)
            {
                var stderr = await _serverProcess.StandardError.ReadToEndAsync();
                throw new InvalidOperationException(
                    $"Server process exited with code {_serverProcess.ExitCode} before becoming ready. Stderr: {stderr}");
            }

            try
            {
                var response = await httpClient.GetAsync(BaseUrl);
                if (response.StatusCode != HttpStatusCode.ServiceUnavailable)
                {
                    ready = true;
                    break;
                }
            }
            catch (Exception) when (attempt < 59)
            {
                // Server not ready yet
            }

            await Task.Delay(1000);
        }

        if (!ready)
        {
            throw new TimeoutException(
                $"Server at {BaseUrl} did not become ready within 60 seconds.");
        }

        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        if (Browser != null)
            await Browser.DisposeAsync();

        PlaywrightInstance?.Dispose();

        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            try
            {
                _serverProcess.Kill(entireProcessTree: true);
                await _serverProcess.WaitForExitAsync();
            }
            catch
            {
                // Best-effort cleanup
            }
            finally
            {
                _serverProcess.Dispose();
            }
        }
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}