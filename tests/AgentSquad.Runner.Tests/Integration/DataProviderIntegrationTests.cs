using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests.Integration
{
    public class DataProviderIntegrationTests
    {
        [Fact]
        public async Task LoadProjectDataAsync_WithActualWwwrootDataJson_LoadsSuccessfully()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            var provider = new DataProvider(cache, "wwwroot/data.json");

            // Act
            var project = await provider.LoadProjectDataAsync();

            // Assert
            Assert.NotNull(project);
            Assert.NotEmpty(project.Milestones);
        }

        [Fact]
        public async Task LoadProjectDataAsync_CacheExpiration_ReloadsFromFile()
        {
            // Arrange
            var cacheOptions = new MemoryCacheOptions();
            var cache = new MemoryCache(cacheOptions);
            var provider = new DataProvider(cache, "wwwroot/data.json");

            // Act
            var first = await provider.LoadProjectDataAsync();
            cache.Remove("project_data");
            var second = await provider.LoadProjectDataAsync();

            // Assert
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Equal(first.Name, second.Name);
        }
    }
}