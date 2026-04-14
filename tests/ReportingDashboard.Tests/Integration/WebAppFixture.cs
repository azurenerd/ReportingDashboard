using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReportingDashboard.Tests.Integration;

public class WebAppFixture : IDisposable
{
    private readonly List<string> _tempDirs = new();

    public WebApplicationFactory<Program> Factory { get; }
    public HttpClient Client { get; }

    public WebAppFixture()
    {
        Factory = new WebApplicationFactory<Program>();
        Client = Factory.CreateClient();
    }

    public HttpClient CreateClientWithValidData()
    {
        var tempDir = CreateTempWebRoot();
        var wwwroot = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwroot);
        var cssDir = Path.Combine(wwwroot, "css");
        Directory.CreateDirectory(cssDir);
        File.WriteAllText(Path.Combine(cssDir, "dashboard.css"), "/* test css */");
        File.WriteAllText(Path.Combine(wwwroot, "data.json"), """
        {
            "title": "Test Project",
            "subtitle": "Team A · Stream B · April 2026",
            "backlogLink": "https://dev.azure.com/test",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{
                    "name": "M1",
                    "label": "Core",
                    "color": "#4285F4",
                    "milestones": [
                        { "date": "2026-02-14", "type": "poc", "label": "PoC" }
                    ]
                }]
            },
            "heatmap": {
                "shipped": {},
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """);

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseWebRoot(wwwroot);
            });
        return factory.CreateClient();
    }

    public HttpClient CreateClientWithMissingData()
    {
        var tempDir = CreateTempWebRoot();
        var wwwroot = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwroot);
        // No data.json file

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseWebRoot(wwwroot);
            });
        return factory.CreateClient();
    }

    public HttpClient CreateClientWithMalformedData()
    {
        var tempDir = CreateTempWebRoot();
        var wwwroot = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwroot);
        File.WriteAllText(Path.Combine(wwwroot, "data.json"), "{ not valid json }}}");

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseWebRoot(wwwroot);
            });
        return factory.CreateClient();
    }

    private string CreateTempWebRoot()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"RD_Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        _tempDirs.Add(dir);
        return dir;
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        foreach (var dir in _tempDirs)
        {
            try { Directory.Delete(dir, true); } catch { }
        }
    }
}