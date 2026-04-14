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
    public void DI_Resolves_DashboardDataService_AsSingleton()
    {
        using var scope1 = _fixture.Factory.Services.CreateScope();
        using var scope2 = _fixture.Factory.Services.CreateScope();

        var service1 = scope1.ServiceProvider.GetRequiredService<DashboardDataService>();
        var service2 = scope2.ServiceProvider.GetRequiredService<DashboardDataService>();

        Assert.Same(service1, service2);
    }

    [Fact]
    public void DI_Resolves_DashboardDataService_WithDataLoaded()
    {
        var service = _fixture.Factory.Services.GetRequiredService<DashboardDataService>();

        // Data should be loaded (or error set) since LoadAsync runs before app.Run()
        Assert.True(service.Data is not null || service.IsError);
    }
}