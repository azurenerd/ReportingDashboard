using System.Diagnostics;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright PlaywrightInstance { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; private set; } = null!;
    private Process? _serverProcess;
    private string? _createdDataJson;

    public async Task InitializeAsync()
    {
        BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

        // Only start the server if no BASE_URL is explicitly provided
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BASE_URL")))
        {
            var projectDir = FindProjectDir();
            if (projectDir != null)
            {
                // Ensure data.json exists by copying from data.sample.json if needed
                var dataJson = Path.Combine(projectDir, "data.json");
                var sampleJson = Path.Combine(projectDir, "data.sample.json");
                if (!File.Exists(dataJson) && File.Exists(sampleJson))
                {
                    File.Copy(sampleJson, dataJson);
                    _createdDataJson = dataJson;
                }

                var csproj = Path.Combine(projectDir, "ReportingDashboard.Web.csproj");
                _serverProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"run --project \"{csproj}\" --no-build",
                        WorkingDirectory = projectDir,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                _serverProcess.Start();

                // Wait for the server to be ready
                using var httpClient = new HttpClient();
                var ready = false;
                for (var i = 0; i < 30; i++)
                {
                    try
                    {
                        var response = await httpClient.GetAsync(BaseUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            ready = true;
                            break;
                        }
                    }
                    catch
                    {
                        // Server not ready yet
                    }
                    await Task.Delay(1000);
                }

                if (!ready)
                {
                    throw new Exception($"Server at {BaseUrl} did not start within 30 seconds.");
                }
            }
        }

        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    private static string? FindProjectDir()
    {
        // Walk up from the test output directory to find the repo root,
        // then navigate to the web project directory
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ReportingDashboard.Web");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "ReportingDashboard.Web.csproj")))
                return candidate;
            dir = dir.Parent;
        }
        return null;
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        PlaywrightInstance.Dispose();

        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            try
            {
                _serverProcess.Kill(entireProcessTree: true);
                _serverProcess.WaitForExit(5000);
            }
            catch
            {
                // Best effort cleanup
            }
            _serverProcess.Dispose();
        }

        // Clean up data.json if we created it
        if (_createdDataJson != null && File.Exists(_createdDataJson))
        {
            try { File.Delete(_createdDataJson); } catch { }
        }
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}