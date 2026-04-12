using AgentSquad.Runner.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AgentSquad.Runner.Tests.Integration;

[Trait("Category", "Integration")]
public class DependencyInjectionTests
{
    [Fact]
    public void ServiceCollection_CanResolveIDashboardDataService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IDashboardDataService, DashboardDataService>();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        var provider = services.BuildServiceProvider();
        var service = provider.GetService<IDashboardDataService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<DashboardDataService>();
    }

    [Fact]
    public void ServiceCollection_CanResolveIDateCalculationService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IDateCalculationService, DateCalculationService>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetService<IDateCalculationService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<DateCalculationService>();
    }

    [Fact]
    public void ServiceCollection_CanResolveIVisualizationService()
    {
        var services = new ServiceCollection();
        services.AddScoped<IVisualizationService, VisualizationService>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetService<IVisualizationService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<VisualizationService>();
    }

    [Fact]
    public void ServiceCollection_AllServicesCanBeResolvedTogether()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IDashboardDataService, DashboardDataService>();
        services.AddScoped<IDateCalculationService, DateCalculationService>();
        services.AddScoped<IVisualizationService, VisualizationService>();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        var provider = services.BuildServiceProvider();

        var dataService = provider.GetService<IDashboardDataService>();
        var dateService = provider.GetService<IDateCalculationService>();
        var vizService = provider.GetService<IVisualizationService>();

        dataService.Should().NotBeNull();
        dateService.Should().NotBeNull();
        vizService.Should().NotBeNull();
    }

    [Fact]
    public void DashboardDataService_HasCorrectLifetime()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IDashboardDataService, DashboardDataService>();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        var provider = services.BuildServiceProvider();

        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var service1 = scope1.ServiceProvider.GetService<IDashboardDataService>();
        var service2 = scope2.ServiceProvider.GetService<IDashboardDataService>();

        service1.Should().NotBeSameAs(service2);
    }

    [Fact]
    public void VisualizationService_CanBeResolved()
    {
        var services = new ServiceCollection();
        services.AddScoped<IVisualizationService, VisualizationService>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetService<IVisualizationService>();

        service.Should().NotBeNull();
    }
}