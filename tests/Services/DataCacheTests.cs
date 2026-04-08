using Moq;
using Xunit;
using Microsoft.Extensions.Caching.Memory;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Services
{
    public class DataCacheTests
    {
        [Fact]
        public async Task SetAsync_StoresValueInCache()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new DataCache(memoryCache);
            var project = new Project { Name = "Test" };

            await cache.SetAsync("test_key", project);

            var retrieved = await cache.GetAsync<Project>("test_key");
            Assert.NotNull(retrieved);
            Assert.Equal("Test", retrieved.Name);
        }

        [Fact]
        public async Task GetAsync_ReturnsNull_WhenKeyDoesNotExist()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new DataCache(memoryCache);

            var result = await cache.GetAsync<Project>("nonexistent_key");

            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_WithExpiration_ExpiresAfterTimespan()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new DataCache(memoryCache);
            var project = new Project { Name = "Test" };

            await cache.SetAsync("test_key", project, TimeSpan.FromMilliseconds(100));

            var retrieved = await cache.GetAsync<Project>("test_key");
            Assert.NotNull(retrieved);

            await Task.Delay(150);

            var expiredResult = await cache.GetAsync<Project>("test_key");
            Assert.Null(expiredResult);
        }

        [Fact]
        public void Remove_DeletesValueFromCache()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new DataCache(memoryCache);
            var project = new Project { Name = "Test" };

            memoryCache.Set("test_key", project);
            cache.Remove("test_key");

            var result = memoryCache.TryGetValue("test_key", out Project retrievedProject);
            Assert.False(result);
        }

        [Fact]
        public async Task SetAsync_WithoutExpiration_StoresIndefinitely()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new DataCache(memoryCache);
            var project = new Project { Name = "Test" };

            await cache.SetAsync("test_key", project);

            var retrieved1 = await cache.GetAsync<Project>("test_key");
            Assert.NotNull(retrieved1);

            await Task.Delay(100);

            var retrieved2 = await cache.GetAsync<Project>("test_key");
            Assert.NotNull(retrieved2);
        }

        [Fact]
        public async Task GetAsync_ReturnsCorrectType()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new DataCache(memoryCache);
            var project = new Project { Name = "Test", Description = "Test Description" };

            await cache.SetAsync("project", project);

            var retrieved = await cache.GetAsync<Project>("project");

            Assert.NotNull(retrieved);
            Assert.IsType<Project>(retrieved);
            Assert.Equal("Test Description", retrieved.Description);
        }

        [Fact]
        public async Task MultipleValues_StoredAndRetrievedIndependently()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new DataCache(memoryCache);
            var project1 = new Project { Name = "Project1" };
            var project2 = new Project { Name = "Project2" };

            await cache.SetAsync("key1", project1);
            await cache.SetAsync("key2", project2);

            var retrieved1 = await cache.GetAsync<Project>("key1");
            var retrieved2 = await cache.GetAsync<Project>("key2");

            Assert.Equal("Project1", retrieved1.Name);
            Assert.Equal("Project2", retrieved2.Name);
        }

        [Fact]
        public void Remove_NonexistentKey_DoesNotThrow()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cache = new DataCache(memoryCache);

            cache.Remove("nonexistent_key");

            // Should not throw
            Assert.True(true);
        }
    }
}