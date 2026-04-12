#nullable enable

using AgentSquad.Runner.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Configuration;

[Trait("Category", "Unit")]
public class ConfigurationTests
{
    [Fact]
    public void BuildServiceProvider_WithAllRequiredServices_Succeeds()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Dashboard:DataFilePath", "./wwwroot/data/data.json" }
            })
            .Build());
        services.AddScoped<IDashboardDataService, DashboardDataService>();
        services.AddSingleton<IDateCalculationService, DateCalculationService>();
        services.AddSingleton<IVisualizationService, VisualizationService>();

        var provider = services.BuildServiceProvider();
        provider.Should().NotBeNull();

        var dataService = provider.GetRequiredService<IDashboardDataService>();
        var dateService = provider.GetRequiredService<IDateCalculationService>();
        var vizService = provider.GetRequiredService<IVisualizationService>();

        dataService.Should().NotBeNull();
        dateService.Should().NotBeNull();
        vizService.Should().NotBeNull();
    }
}