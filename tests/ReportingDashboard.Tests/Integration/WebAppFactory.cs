using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory that configures a temp Data directory
/// so tests can control the data.json file independently.
/// </summary>
public class WebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _tempContentRoot;

    public string DataDir { get; }
    public string DataJsonPath { get; }

    public WebAppFactory()
    {
        _tempContentRoot = Path.Combine(Path.GetTempPath(), $"DashboardIntTest_{Guid.NewGuid():N}");
        DataDir = Path.Combine(_tempContentRoot, "Data");
        Directory.CreateDirectory(DataDir);
        DataJsonPath = Path.Combine(DataDir, "data.json");

        // Copy wwwroot if needed for static files
        var wwwroot = Path.Combine(_tempContentRoot, "wwwroot", "css");
        Directory.CreateDirectory(wwwroot);
    }

    public void WriteDataJson(string content)
    {
        File.WriteAllText(DataJsonPath, content);
    }

    public void DeleteDataJson()
    {
        if (File.Exists(DataJsonPath))
            File.Delete(DataJsonPath);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(_tempContentRoot);
        builder.UseEnvironment("Development");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (Directory.Exists(_tempContentRoot))
        {
            try { Directory.Delete(_tempContentRoot, recursive: true); } catch { }
        }
    }
}