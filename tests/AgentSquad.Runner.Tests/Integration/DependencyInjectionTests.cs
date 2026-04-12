using AgentSquad.Runner.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AgentSquad.Runner.Tests.Integration;

[Trait("Category", "Integration")]
public class DependencyInjectionTests
{
    private string GetProjectPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var projectPath = Path.Combine(currentDirectory, "..", "..", "..", "src", "AgentSquad.Runner");
        return Path.GetFullPath(projectPath);
    }

    [Fact]
    public void DIContainer_CanResolveDashboardDataService()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddScoped<IDashboardDataService, DashboardDataService>();

        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<IDashboardDataService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<DashboardDataService>();
    }

    [Fact]
    public void DIContainer_CanResolveDateCalculationService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IDateCalculationService, DateCalculationService>();

        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<IDateCalculationService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<DateCalculationService>();
    }

    [Fact]
    public void DIContainer_CanResolveVisualizationService()
    {
        var services = new ServiceCollection();
        services.AddScoped<IVisualizationService, VisualizationService>();

        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<IVisualizationService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<VisualizationService>();
    }

    [Fact]
    public void DIContainer_CanResolveAllThreeServices()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddScoped<IDashboardDataService, DashboardDataService>();
        services.AddScoped<IDateCalculationService, DateCalculationService>();
        services.AddScoped<IVisualizationService, VisualizationService>();

        var serviceProvider = services.BuildServiceProvider();

        var dashboardService = serviceProvider.GetService<IDashboardDataService>();
        var dateService = serviceProvider.GetService<IDateCalculationService>();
        var vizService = serviceProvider.GetService<IVisualizationService>();

        dashboardService.Should().NotBeNull();
        dateService.Should().NotBeNull();
        vizService.Should().NotBeNull();
    }

    [Fact]
    public void DIContainer_ServicesAreCorrectlyScoped()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IDateCalculationService, DateCalculationService>();

        var serviceProvider = services.BuildServiceProvider();

        using var scope1 = serviceProvider.CreateScope();
        var service1 = scope1.ServiceProvider.GetService<IDateCalculationService>();

        using var scope2 = serviceProvider.CreateScope();
        var service2 = scope2.ServiceProvider.GetService<IDateCalculationService>();

        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().NotBe(service2);
    }
}