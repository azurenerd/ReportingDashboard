using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Tests verifying that the DI container is wired correctly at startup:
/// singleton registration, logger injection, and service state after startup loading.
/// </summary>
[Trait("Category", "Integration")]
public class ServiceRegistrationIntegrationTests : IClassFixture<WebApplicationFactory<ReportingDashboard.Components.App>>
{
    private readonly WebApplicationFactory<ReportingDashboard.Components.App> _factory;

    public ServiceRegistrationIntegrationTests(WebApplicationFactory<ReportingDashboard.Components.App> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void DI_DashboardDataService_IsRegistered()
    {
        var svc = _factory.Services.GetService<DashboardDataService>();
        Assert.NotNull(svc);
    }

    [Fact]
    public void DI_DashboardDataService_IsSingleton()
    {
        var svc1 = _factory.Services.GetRequiredService<DashboardDataService>();
        var svc2 = _factory.Services.GetRequiredService<DashboardDataService>();
        Assert.Same(svc1, svc2);
    }

    [Fact]
    public void DI_DashboardDataService_HasLoadedData()
    {
        // After startup, the service should have attempted to load data.json
        var svc = _factory.Services.GetRequiredService<DashboardDataService>();

        // Either data loaded successfully or error was set; it should not be in
        // the initial unloaded state (both null)
        var hasLoaded = svc.Data != null || svc.IsError;
        Assert.True(hasLoaded, "Service should have attempted to load data at startup");
    }

    [Fact]
    public void DI_LoggerFactory_IsAvailable()
    {
        var loggerFactory = _factory.Services.GetService<ILoggerFactory>();
        Assert.NotNull(loggerFactory);
    }

    [Fact]
    public void DI_DashboardDataServiceLogger_IsInjectable()
    {
        var logger = _factory.Services.GetService<ILogger<DashboardDataService>>();
        Assert.NotNull(logger);
    }

    [Fact]
    public void DI_MultipleScopes_ResolveSameServiceInstance()
    {
        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();

        var svc1 = scope1.ServiceProvider.GetRequiredService<DashboardDataService>();
        var svc2 = scope2.ServiceProvider.GetRequiredService<DashboardDataService>();

        Assert.Same(svc1, svc2);
    }
}