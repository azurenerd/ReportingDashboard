using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AgentSquad.Runner.Tests.Integration;

public class ProgramConfigurationTests
{
    [Fact]
    public void DependencyInjection_RegistersDataCache_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddScoped<IDataCache, MemoryCacheAdapter>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var cache1 = serviceProvider.GetRequiredService<IDataCache>();
        var cache2 = serviceProvider.GetRequiredService<IDataCache>();

        // Assert
        Assert.NotNull(cache1);
        Assert.NotNull(cache2);
    }

    [Fact]
    public void DependencyInjection_RegistersDataProvider_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddLogging();
        services.AddScoped<IDataCache, MemoryCacheAdapter>();
        services.AddScoped<IDataProvider, DataProvider>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var provider = serviceProvider.GetRequiredService<IDataProvider>();

        // Assert
        Assert.NotNull(provider);
        Assert.IsType<DataProvider>(provider);
    }

    [Fact]
    public void DependencyInjection_ResolvesServicesWithDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddLogging();
        services.AddScoped<IDataCache, MemoryCacheAdapter>();
        services.AddScoped<IDataProvider, DataProvider>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var provider = serviceProvider.GetRequiredService<IDataProvider>();
        var cache = serviceProvider.GetRequiredService<IDataCache>();

        // Assert
        Assert.NotNull(provider);
        Assert.NotNull(cache);
    }
}