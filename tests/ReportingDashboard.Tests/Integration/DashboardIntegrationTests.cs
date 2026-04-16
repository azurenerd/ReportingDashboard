using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory that locates the entry point assembly by using
/// a public type from the ReportingDashboard project, avoiding the inaccessible
/// top-level-statements-generated internal Program class.
/// </summary>
public class DashboardWebApplicationFactory : WebApplicationFactory<DashboardDataService>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // WebApplicationFactory<T> uses typeof(T).Assembly to find the entry assembly.
        // DashboardDataService is a public type in the ReportingDashboard assembly,
        // so this works without needing access to the internal Program class.
        return base.CreateHost(builder);
    }
}

[Trait("Category", "Integration")]
public class DashboardIntegrationTests : IClassFixture<DashboardWebApplicationFactory>
{
    private readonly DashboardWebApplicationFactory _factory;

    public DashboardIntegrationTests(DashboardWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HomePage_ReturnsSuccessStatusCode()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task HomePage_ContainsBlazorScript()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("_framework/blazor");
    }

    [Fact]
    public void DashboardDataService_IsRegisteredAsSingleton()
    {
        var service1 = _factory.Services.GetService<DashboardDataService>();
        var service2 = _factory.Services.GetService<DashboardDataService>();

        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().BeSameAs(service2);
    }

    [Fact]
    public async Task HomePage_ContainsDashboardMarkup()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        var hasDashboard = content.Contains("dashboard") || content.Contains("error-container");
        hasDashboard.Should().BeTrue();
    }

    [Fact]
    public async Task StaticFiles_CssIsServed()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/css/app.css");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("1920px");
        content.Should().Contain("1080px");
    }
}