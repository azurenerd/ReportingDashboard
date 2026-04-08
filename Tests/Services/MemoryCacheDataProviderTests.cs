using System;
using Xunit;
using Microsoft.Extensions.Caching.Memory;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests.Services
{
    public class MemoryCacheDataProviderTests
    {
        private IMemoryCache CreateMemoryCache() => new MemoryCache(new MemoryCacheOptions());

        [Fact]
        public void Set_StoresValueInCache()
        {
            var cache = CreateMemoryCache();
            var provider = new MemoryCacheDataProvider(cache);
            var project = new Project { Name = "Test" };

            provider.Set("test_key", project);

            var retrieved = provider.Get<Project>("test_key");
            Assert.NotNull(retrieved);
            Assert.Equal("Test", retrieved.Name);
        }

        [Fact]
        public void Get_ReturnsNullForMissingKey()
        {
            var cache = CreateMemoryCache();
            var provider = new MemoryCacheDataProvider(cache);

            var result = provider.Get<Project>("nonexistent");

            Assert.Null(result);
        }

        [Fact]
        public void TryGetValue_ReturnsTrueForExistingKey()
        {
            var cache = CreateMemoryCache();
            var provider = new MemoryCacheDataProvider(cache);
            var project = new Project { Name = "Test" };
            provider.Set("key", project);

            bool success = provider.TryGetValue("key", out Project retrieved);

            Assert.True(success);
            Assert.NotNull(retrieved);
            Assert.Equal("Test", retrieved.Name);
        }

        [Fact]
        public void TryGetValue_ReturnsFalseForMissingKey()
        {
            var cache = CreateMemoryCache();
            var provider = new MemoryCacheDataProvider(cache);

            bool success = provider.TryGetValue("nonexistent", out Project retrieved);

            Assert.False(success);
            Assert.Null(retrieved);
        }

        [Fact]
        public void Remove_DeletesKeyFromCache()
        {
            var cache = CreateMemoryCache();
            var provider = new MemoryCacheDataProvider(cache);
            var project = new Project { Name = "Test" };
            provider.Set("key", project);

            provider.Remove("key");

            bool success = provider.TryGetValue("key", out _);
            Assert.False(success);
        }

        [Fact]
        public void Set_WithSameKey_UpdatesValue()
        {
            var cache = CreateMemoryCache();
            var provider = new MemoryCacheDataProvider(cache);
            var project1 = new Project { Name = "First" };
            var project2 = new Project { Name = "Second" };

            provider.Set("key", project1);
            provider.Set("key", project2);

            var retrieved = provider.Get<Project>("key");
            Assert.Equal("Second", retrieved.Name);
        }

        [Fact]
        public void Set_CachesMultipleTypes()
        {
            var cache = CreateMemoryCache();
            var provider = new MemoryCacheDataProvider(cache);
            var project = new Project { Name = "Test" };
            var metrics = new ProjectMetrics { CompletionPercentage = 50 };

            provider.Set("project", project);
            provider.Set("metrics", metrics);

            var retrievedProject = provider.Get<Project>("project");
            var retrievedMetrics = provider.Get<ProjectMetrics>("metrics");

            Assert.NotNull(retrievedProject);
            Assert.NotNull(retrievedMetrics);
            Assert.Equal("Test", retrievedProject.Name);
            Assert.Equal(50, retrievedMetrics.CompletionPercentage);
        }
    }
}