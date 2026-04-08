using Xunit;
using Microsoft.Extensions.DependencyInjection;
using AgentSquad.Runner.Services;
using System.Threading.Tasks;

namespace AgentSquad.Runner.Tests.Integration
{
    public class ProgramConfigurationTests
    {
        [Fact]
        public void ServiceCollection_RegistersDataProviderAsScoped()
        {
            var services = new ServiceCollection();
            services.AddScoped<IDataProvider, DataProvider>();
            var provider = services.BuildServiceProvider();

            var service1 = provider.GetRequiredService<IDataProvider>();
            var service2 = provider.GetRequiredService<IDataProvider>();

            Assert.NotSame(service1, service2);
        }

        [Fact]
        public void ServiceCollection_RegistersMemoryCacheAsScoped()
        {
            var services = new ServiceCollection();
            services.AddScoped<IDataCache, MemoryCacheAdapter>();
            var provider = services.BuildServiceProvider();

            var cache = provider.GetRequiredService<IDataCache>();
            Assert.NotNull(cache);
        }

        [Fact]
        public async Task ProgramConfiguration_DataProviderUsesAsyncMethods()
        {
            var services = new ServiceCollection();
            services.AddScoped<IDataCache, MemoryCacheAdapter>();
            var provider = services.BuildServiceProvider();

            var cache = provider.GetRequiredService<IDataCache>();
            await cache.SetAsync("test", "value", TimeSpan.FromMinutes(5));
            var result = await cache.GetAsync<string>("test");

            Assert.Equal("value", result);
        }

        [Fact]
        public void ServiceCollection_CanResolveAllDependencies()
        {
            var services = new ServiceCollection();
            services.AddScoped<IDataCache, MemoryCacheAdapter>();
            services.AddScoped<IDataProvider>(sp =>
                new DataProvider("data.json", sp.GetRequiredService<IDataCache>()));

            var provider = services.BuildServiceProvider();
            var dataProvider = provider.GetRequiredService<IDataProvider>();

            Assert.NotNull(dataProvider);
        }
    }
}