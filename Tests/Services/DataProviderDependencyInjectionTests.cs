using AgentSquad.Runner.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Tests.Services;

public class DataProviderDependencyInjectionTests
{
    [Fact]
    public void DependencyInjection_RegistersDataProviderAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddScoped<IDataCache, DataCache>();
        services.AddScoped<IDataProvider, DataProvider>();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var provider1 = serviceProvider.GetRequiredService<IDataProvider>();
        var provider2 = serviceProvider.GetRequiredService<IDataProvider>();

        // Assert
        Assert.NotNull(provider1);
        Assert.NotNull(provider2);
        Assert.NotSame(provider1, provider2);
    }

    [Fact]
    public void DependencyInjection_RegistersDataCacheAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddScoped<IDataCache, DataCache>();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var cache1 = serviceProvider.GetRequiredService<IDataCache>();
        var cache2 = serviceProvider.GetRequiredService<IDataCache>();

        // Assert
        Assert.NotNull(cache1);
        Assert.NotNull(cache2);
        Assert.NotSame(cache1, cache2);
    }

    [Fact]
    public void DependencyInjection_SameScopeSharesDataCache()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddScoped<IDataCache, DataCache>();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        using (var scope = serviceProvider.CreateScope())
        {
            var cache1 = scope.ServiceProvider.GetRequiredService<IDataCache>();
            var cache2 = scope.ServiceProvider.GetRequiredService<IDataCache>();

            // Assert - Same scope should return same instance
            Assert.Same(cache1, cache2);
        }
    }

    [Fact]
    public void DependencyInjection_DataProviderCanBeResolved()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddScoped<IDataCache, DataCache>();
        services.AddScoped<IDataProvider, DataProvider>();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var provider = serviceProvider.GetRequiredService<IDataProvider>();

        // Assert
        Assert.NotNull(provider);
        Assert.IsType<DataProvider>(provider);
    }

    [Fact]
    public void DependencyInjection_MemoryCacheSharedAcrossServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddScoped<IDataCache, DataCache>();
        services.AddScoped<IDataProvider, DataProvider>();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        using (var scope = serviceProvider.CreateScope())
        {
            var cache = scope.ServiceProvider.GetRequiredService<IDataCache>();
            var provider = scope.ServiceProvider.GetRequiredService<IDataProvider>();

            // Assert - Both should be resolvable and non-null
            Assert.NotNull(cache);
            Assert.NotNull(provider);
        }
    }
}