#nullable enable

using AgentSquad.Runner.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AgentSquad.Runner.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void ServiceProviderCanResolveDashboardDataService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("Dashboard:DataFilePath", "./data.json") })
            .Build());
        services.AddScoped<DashboardDataService>();
        services.AddSingleton<IDateCalculationService, DateCalculationService>();
        services.AddSingleton<IVisualizationService, VisualizationService>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<DashboardDataService>();

        Assert.NotNull(service);
    }

    [Fact]
    public void DateCalculationServiceIsRegisteredAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IDateCalculationService, DateCalculationService>();

        var provider = services.BuildServiceProvider();
        var service1 = provider.GetRequiredService<IDateCalculationService>();
        var service2 = provider.GetRequiredService<IDateCalculationService>();

        Assert.Same(service1, service2);
    }

    [Fact]
    public void VisualizationServiceIsRegisteredAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IVisualizationService, VisualizationService>();

        var provider = services.BuildServiceProvider();
        var service1 = provider.GetRequiredService<IVisualizationService>();
        var service2 = provider.GetRequiredService<IVisualizationService>();

        Assert.Same(service1, service2);
    }

    [Fact]
    public void DashboardDataServiceIsRegisteredAsScoped()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("Dashboard:DataFilePath", "./data.json") })
            .Build());
        services.AddScoped<DashboardDataService>();

        var provider = services.BuildServiceProvider();
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var service1 = scope1.ServiceProvider.GetRequiredService<DashboardDataService>();
        var service2 = scope2.ServiceProvider.GetRequiredService<DashboardDataService>();

        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void IConfigurationCanBeResolvedWithDataFilePath()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("Dashboard:DataFilePath", "./wwwroot/data/data.json") })
            .Build());

        var provider = services.BuildServiceProvider();
        var config = provider.GetRequiredService<IConfiguration>();

        Assert.Equal("./wwwroot/data/data.json", config["Dashboard:DataFilePath"]);
    }
}