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
    public void DI_ResolvesDashboardDataService_AsSingleton()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var scope1 = factory.Services.CreateScope();
        using var scope2 = factory.Services.CreateScope();

        var svc1 = scope1.ServiceProvider.GetService<DashboardDataService>();
        var svc2 = scope2.ServiceProvider.GetService<DashboardDataService>();

        svc1.Should().NotBeNull();
        svc2.Should().NotBeNull();
        svc1.Should().BeSameAs(svc2, "DashboardDataService should be registered as a singleton");
    }

    [Fact]
    public void DI_ResolvesDashboardDataService_NotNull()
    {
        using var factory = new WebApplicationFactory<Program>();
        var service = factory.Services.GetService<DashboardDataService>();

        service.Should().NotBeNull("DashboardDataService must be registered in DI");
    }
}