using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace ReportingDashboard.Tests.Integration;

public class WebAppFixture : IDisposable
{
    public WebApplicationFactory<Program> Factory { get; }
    public HttpClient Client { get; }

    private readonly List<IDisposable> _disposables = new();
    private readonly List<string> _tempDirs = new();

    public WebAppFixture()
    {
        Factory = new WebApplicationFactory<Program>();
        Client = Factory.CreateClient();
    }

    public HttpClient CreateClientWithValidData()
    {
        // Default factory uses the real project wwwroot which contains data.json
        var client = Factory.CreateClient();
        _disposables.Add(client);
        return client;
    }

    public HttpClient CreateClientWithMissingData()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"WebAppTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        _tempDirs.Add(tempDir);

        var factory = Factory.WithWebHostBuilder(builder =>
        {
            builder.UseWebRoot(wwwrootDir);
        });
        _disposables.Add(factory);

        var client = factory.CreateClient();
        _disposables.Add(client);
        return client;
    }

    public HttpClient CreateClientWithMalformedData()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"WebAppTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);
        File.WriteAllText(Path.Combine(wwwrootDir, "data.json"), "{ this is not valid json !!! }");
        _tempDirs.Add(tempDir);

        var factory = Factory.WithWebHostBuilder(builder =>
        {
            builder.UseWebRoot(wwwrootDir);
        });
        _disposables.Add(factory);

        var client = factory.CreateClient();
        _disposables.Add(client);
        return client;
    }

    public void Dispose()
    {
        foreach (var d in _disposables)
        {
            try { d.Dispose(); } catch { }
        }
        Client.Dispose();
        Factory.Dispose();

        foreach (var dir in _tempDirs)
        {
            try
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
            catch { }
        }
    }
}