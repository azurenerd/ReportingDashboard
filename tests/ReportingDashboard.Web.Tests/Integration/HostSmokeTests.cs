using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests.Integration;

// Project is a public type in the web assembly; WebApplicationFactory<T>
// uses T's assembly only to locate the entry point (Program), so any
// public type from the web project works even though Program is internal.
public class HostSmokeTests : IClassFixture<WebApplicationFactory<Project>>
{
    private readonly WebApplicationFactory<Project> _factory;

    public HostSmokeTests(WebApplicationFactory<Project> factory)
    {
        _factory = factory.WithWebHostBuilder(b =>
        {
            b.UseSetting("urls", "http://127.0.0.1:0");
        });
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_Root_Returns200WithNonEmptyBody()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_Root_DoesNotContainBlazorServerScript()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        body.Should().NotContain("_framework/blazor.server.js");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_Healthz_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/healthz");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("ok");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void DI_Resolves_IDashboardDataService_AsSingleton()
    {
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();

        var a = scope1.ServiceProvider.GetRequiredService<IDashboardDataService>();
        var b = scope2.ServiceProvider.GetRequiredService<IDashboardDataService>();

        a.Should().BeSameAs(b);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void DashboardDataService_GetCurrent_ReturnsNotFoundStub_WithoutThrowing()
    {
        var svc = _factory.Services.GetRequiredService<IDashboardDataService>();

        var result = svc.GetCurrent();

        result.Should().NotBeNull();
        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be("NotFound");
    }
}