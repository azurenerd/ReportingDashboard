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
    private string GetProjectPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var projectRoot = currentDirectory;
        
        while (!Directory.Exists(Path.Combine(projectRoot, "src", "AgentSquad.Runner")))
        {
            var parent = Directory.GetParent(projectRoot);
            if (parent == null || parent.FullName == projectRoot)
                break;
            projectRoot = parent.FullName;
        }
        
        return Path.Combine(projectRoot, "src", "AgentSquad.Runner");
    }

    private IServiceProvider SetupServiceProvider()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(config);
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IDashboardDataService, DashboardDataService>();
        services.AddSingleton<IDateCalculationService, DateCalculationService>();
        services.AddSingleton<IVisualizationService, VisualizationService>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void DashboardDataService_RegistersSuccessfully()
    {
        var provider = SetupServiceProvider();

        var service = provider.GetService<IDashboardDataService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<DashboardDataService>();
    }

    [Fact]
    public void DateCalculationService_RegistersSuccessfully()
    {
        var provider = SetupServiceProvider();

        var service = provider.GetService<IDateCalculationService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<DateCalculationService>();
    }

    [Fact]
    public void VisualizationService_RegistersSuccessfully()
    {
        var provider = SetupServiceProvider();

        var service = provider.GetService<IVisualizationService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<VisualizationService>();
    }

    [Fact]
    public void AllServices_CanBeResolvedTogether()
    {
        var provider = SetupServiceProvider();

        var dataService = provider.GetService<IDashboardDataService>();
        var dateService = provider.GetService<IDateCalculationService>();
        var vizService = provider.GetService<IVisualizationService>();

        dataService.Should().NotBeNull();
        dateService.Should().NotBeNull();
        vizService.Should().NotBeNull();
    }

    [Fact]
    public void Services_RegisteredAsSingleton_ReturnSameInstance()
    {
        var provider = SetupServiceProvider();

        var service1 = provider.GetService<IDashboardDataService>();
        var service2 = provider.GetService<IDashboardDataService>();

        service1.Should().BeSameAs(service2);
    }
}