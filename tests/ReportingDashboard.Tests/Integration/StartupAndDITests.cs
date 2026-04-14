using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

public class StartupAndDITests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public StartupAndDITests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void DI_Resolves_DashboardDataService_AsSingleton()
    {
        using var scope1 = _fixture.Factory.Services.CreateScope();
        using var scope2 = _fixture.Factory.Services.CreateScope();

        var svc1 = scope1.ServiceProvider.GetRequiredService<DashboardDataService>();
        var svc2 = scope2.ServiceProvider.GetRequiredService<DashboardDataService>();

        svc1.Should().NotBeNull();
        svc2.Should().NotBeNull();
        svc1.Should().BeSameAs(svc2, "DashboardDataService is registered as singleton");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void DashboardDataService_LoadedAtStartup_HasDataOrError()
    {
        var svc = _fixture.Factory.Services.GetRequiredService<DashboardDataService>();

        // Service must have attempted load during startup
        var loaded = svc.Data is not null || svc.IsError;
        loaded.Should().BeTrue("LoadAsync should have been called during Program.cs startup");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GET_Root_Returns_SuccessStatusCode()
    {
        var response = await _fixture.Client.GetAsync("/");

        response.IsSuccessStatusCode.Should().BeTrue(
            $"GET / returned {response.StatusCode}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GET_StaticFile_DashboardCss_Returns_200()
    {
        var response = await _fixture.Client.GetAsync("/css/dashboard.css");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK,
            "static files middleware should serve wwwroot/css/dashboard.css");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GET_NonExistentRoute_Returns_NotFound_Or_FallbackPage()
    {
        var response = await _fixture.Client.GetAsync("/this-does-not-exist-xyz");

        ((int)response.StatusCode).Should().BeLessThan(500,
            "unknown routes should not cause server errors");
    }
}