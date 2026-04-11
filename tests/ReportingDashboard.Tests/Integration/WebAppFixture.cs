using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

public class WebAppFixture : IAsyncLifetime
{
    private WebApplicationFactory<ReportingDashboard.Components.App>? _factory;

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();
    }

    private WebApplicationFactory<ReportingDashboard.Components.App> CreateFactory(Action<IWebHostBuilder>? configure = null)
    {
        _factory?.Dispose();
        _factory = new WebApplicationFactory<ReportingDashboard.Components.App>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                configure?.Invoke(builder);
            });
        return _factory;
    }

    public HttpClient CreateClientWithValidData()
    {
        var factory = CreateFactory();
        return factory.CreateClient();
    }

    public HttpClient CreateClientWithMissingData()
    {
        var factory = CreateFactory(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // The app already loads data.json at startup; if it doesn't exist
                // in the test environment, IsError will be true.
                // We override by re-loading with a nonexistent path after build.
            });
            builder.UseSetting("WebRootPath", Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}"));
        });
        return factory.CreateClient();
    }

    public HttpClient CreateClientWithMalformedData()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"malformed_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "data.json"), "{ broken json {{{}");

        var factory = CreateFactory(builder =>
        {
            builder.UseSetting("WebRootPath", tempDir);
        });
        return factory.CreateClient();
    }

    public HttpClient Client => CreateClientWithValidData();
}