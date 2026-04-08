using Xunit;
using Moq;
using AgentSquad.Services;
using AgentSquad.Models;
using System.Threading.Tasks;

namespace AgentSquad.Tests.Services
{
    public class MemoryCacheDataProviderTests
    {
        private readonly MemoryCacheDataProvider _cacheProvider;

        public MemoryCacheDataProviderTests()
        {
            _cacheProvider = new MemoryCacheDataProvider();
        }

        [Fact]
        public async Task GetAsync_CachedValue_ReturnsCachedValue()
        {
            var key = "test-key";
            var value = new Project { Id = "p1", Name = "Test" };

            await _cacheProvider.SetAsync(key, value);
            var result = await _cacheProvider.GetAsync<Project>(key);

            Assert.NotNull(result);
            Assert.Equal("p1", result.Id);
        }

        [Fact]
        public async Task GetAsync_MissingKey_ReturnsNull()
        {
            var result = await _cacheProvider.GetAsync<Project>("nonexistent-key");
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_NewValue_CachesValue()
        {
            var key = "new-key";
            var value = new Milestone { Id = "m1", Name = "Release" };

            await _cacheProvider.SetAsync(key, value);
            var result = await _cacheProvider.GetAsync<Milestone>(key);

            Assert.NotNull(result);
            Assert.Equal("m1", result.Id);
        }

        [Fact]
        public async Task SetAsync_OverwriteExisting_UpdatesValue()
        {
            var key = "update-key";
            var value1 = new Project { Id = "p1", Name = "First" };
            var value2 = new Project { Id = "p1", Name = "Updated" };

            await _cacheProvider.SetAsync(key, value1);
            await _cacheProvider.SetAsync(key, value2);
            var result = await _cacheProvider.GetAsync<Project>(key);

            Assert.Equal("Updated", result.Name);
        }

        [Fact]
        public async Task TryGetValueAsync_ExistingKey_ReturnsTrue()
        {
            var key = "exists-key";
            await _cacheProvider.SetAsync(key, new { data = "test" });

            var success = await _cacheProvider.TryGetValueAsync<dynamic>(key, out var value);

            Assert.True(success);
            Assert.NotNull(value);
        }

        [Fact]
        public async Task TryGetValueAsync_MissingKey_ReturnsFalse()
        {
            var success = await _cacheProvider.TryGetValueAsync<Project>("missing", out var value);

            Assert.False(success);
            Assert.Null(value);
        }

        [Fact]
        public async Task ClearAsync_RemovesAllCachedValues()
        {
            await _cacheProvider.SetAsync("key1", "value1");
            await _cacheProvider.SetAsync("key2", "value2");
            
            await _cacheProvider.ClearAsync();
            
            var result1 = await _cacheProvider.GetAsync<string>("key1");
            var result2 = await _cacheProvider.GetAsync<string>("key2");

            Assert.Null(result1);
            Assert.Null(result2);
        }
    }
}