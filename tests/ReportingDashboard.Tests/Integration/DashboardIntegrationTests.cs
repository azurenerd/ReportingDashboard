using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

public class DashboardWebApplicationFactory : WebApplicationFactory<DashboardDataService>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
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
    public async Task StaticFiles_CssIsServed()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/css/app.css");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("1920px");
        content.Should().Contain("1080px");
    }

    [Fact]
    public async Task DashboardDataService_LoadsDataThroughDI()
    {
        var service = _factory.Services.GetRequiredService<DashboardDataService>();
        var data = await service.GetDashboardDataAsync();

        // Service should either load data or set an error — never throw
        if (data != null)
        {
            data.Title.Should().NotBeNull();
            data.Milestones.Should().NotBeNull();
            data.Heatmap.Should().NotBeNull();
            service.GetError().Should().BeNull();
        }
        else
        {
            service.GetError().Should().NotBeNullOrEmpty();
        }
    }
}