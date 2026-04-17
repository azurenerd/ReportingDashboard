using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using ReportingDashboard.Components;
using ReportingDashboard.Data;
using Xunit;

namespace ReportingDashboard.UITests;

public class PlaywrightFixture : IAsyncLifetime
{
    private WebApplication? _app;
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; } = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

    public async Task InitializeAsync()
    {
        if (Environment.GetEnvironmentVariable("BASE_URL") == null)
        {
            var contentRoot = FindContentRoot();
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = contentRoot,
                WebRootPath = Path.Combine(contentRoot, "wwwroot")
            });

            builder.WebHost.UseUrls(BaseUrl);
            builder.Services.AddRazorComponents();
            builder.Services.AddScoped<DashboardDataService>();

            _app = builder.Build();
            _app.UseStaticFiles();
            _app.UseAntiforgery();
            _app.MapRazorComponents<App>();

            await _app.StartAsync();
        }

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    private static string FindContentRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "ReportingDashboard");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "ReportingDashboard.csproj")))
                return candidate;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate ReportingDashboard project content root.");
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
        if (_app != null)
            await _app.StopAsync();
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }