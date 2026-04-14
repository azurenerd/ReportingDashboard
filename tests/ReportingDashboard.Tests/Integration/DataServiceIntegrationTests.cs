using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

public class DataServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DataServiceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void ServiceRegistration_ResolvesAsSingleton()
    {
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();

        var service1 = scope1.ServiceProvider.GetRequiredService<DashboardDataService>();
        var service2 = scope2.ServiceProvider.GetRequiredService<DashboardDataService>();

        Assert.NotNull(service1);
        Assert.Same(service1, service2);
    }

    [Fact]
    public void DataAvailableAfterStartup()
    {
        var service = _factory.Services.GetRequiredService<DashboardDataService>();

        Assert.False(service.IsError, service.ErrorMessage);
        Assert.NotNull(service.Data);
    }

    [Fact]
    public void DataContainsExpectedTitle()
    {
        var service = _factory.Services.GetRequiredService<DashboardDataService>();

        Assert.Equal("Privacy Automation Release Roadmap", service.Data?.Title);
    }
}