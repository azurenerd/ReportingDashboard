using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class AppStartupTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AppStartupTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void DI_DashboardDataService_IsRegisteredAsSingleton()
    {
        using var scope = _factory.Services.CreateScope();
        var service1 = _factory.Services.GetService<IDashboardDataService>();
        var service2 = _factory.Services.GetService<IDashboardDataService>();

        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().BeSameAs(service2, "DashboardDataService should be registered as singleton");
    }

    [Fact]
    public async Task Homepage_ReturnsSuccessAndHtml()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Executive Reporting Dashboard");
    }

    [Fact]
    public async Task StaticFiles_CssEndpoint_ReturnsSuccess()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/css/dashboard.css");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("dashboard");
    }

    [Fact]
    public async Task Homepage_ContainsBlazorServerScript()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public void DI_DashboardDataService_ImplementsInterface()
    {
        using var scope = _factory.Services.CreateScope();
        var service = _factory.Services.GetService<IDashboardDataService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<DashboardDataService>();
    }
}