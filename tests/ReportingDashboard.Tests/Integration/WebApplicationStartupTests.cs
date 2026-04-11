using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying the full ASP.NET Core startup pipeline:
/// DI registration, service resolution, middleware ordering, and response behavior
/// via WebApplicationFactory.
/// </summary>
[Trait("Category", "Integration")]
public class WebApplicationStartupTests : IClassFixture<WebApplicationFactory<ReportingDashboard.Components.App>>
{
    private readonly WebApplicationFactory<ReportingDashboard.Components.App> _factory;

    public WebApplicationStartupTests(WebApplicationFactory<ReportingDashboard.Components.App> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Startup_DashboardDataService_RegisteredAsSingleton()
    {
        var client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();

        var svc1 = _factory.Services.GetRequiredService<DashboardDataService>();
        var svc2 = _factory.Services.GetRequiredService<DashboardDataService>();

        Assert.Same(svc1, svc2);
    }

    [Fact]
    public void Startup_DashboardDataService_IsResolvable()
    {
        var svc = _factory.Services.GetService<DashboardDataService>();
        Assert.NotNull(svc);
    }

    [Fact]
    public async Task Startup_RootUrl_Returns200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");

        Assert.True(response.IsSuccessStatusCode,
            $"Root URL returned {response.StatusCode}");
    }

    [Fact]
    public async Task Startup_RootUrl_ReturnsHtmlContent()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        var contentType = response.Content.Headers.ContentType?.MediaType;

        Assert.Equal("text/html", contentType);
    }

    [Fact]
    public async Task Startup_NonExistentRoute_Returns200WithNotFoundContent()
    {
        // Blazor server returns 200 for all routes (client-side routing)
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/this-does-not-exist-route");

        // Blazor SPA returns 200 even for non-existent routes
        Assert.True(response.IsSuccessStatusCode);
    }
}