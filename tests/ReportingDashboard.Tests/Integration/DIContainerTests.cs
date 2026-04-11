using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class DIContainerTests : IDisposable
{
    private readonly WebAppFactory _factory;

    public DIContainerTests()
    {
        _factory = new WebAppFactory();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public void DashboardDataService_IsRegistered()
    {
        var service = _factory.Services.GetService<DashboardDataService>();
        service.Should().NotBeNull();
    }

    [Fact]
    public void DashboardDataService_IsSingleton()
    {
        var s1 = _factory.Services.GetRequiredService<DashboardDataService>();
        var s2 = _factory.Services.GetRequiredService<DashboardDataService>();
        s1.Should().BeSameAs(s2);
    }

    [Fact]
    public void IWebHostEnvironment_IsAvailable()
    {
        var env = _factory.Services.GetService<IWebHostEnvironment>();
        env.Should().NotBeNull();
    }

    [Fact]
    public void DashboardDataService_ResolvesFromScopedProvider()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetService<DashboardDataService>();
        service.Should().NotBeNull();
    }

    [Fact]
    public void DashboardDataService_SameInstanceAcrossScopes()
    {
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();

        var s1 = scope1.ServiceProvider.GetRequiredService<DashboardDataService>();
        var s2 = scope2.ServiceProvider.GetRequiredService<DashboardDataService>();

        s1.Should().BeSameAs(s2, "singleton should return same instance across scopes");
    }
}