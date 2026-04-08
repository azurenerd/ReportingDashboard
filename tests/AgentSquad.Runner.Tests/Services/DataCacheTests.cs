using AgentSquad.Runner.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Services
{
    public class DataCacheTests
    {
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<ILogger<DataCache>> _mockLogger;
        private readonly DataCache _dataCache;

        public DataCacheTests()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _mockLogger = new Mock<ILogger<DataCache>>();
            _dataCache = new DataCache(_memoryCache, _mockLogger.Object);
        }

        [Fact]
        public async Task SetAsync_WithValidKeyAndValue_StoresValueInCache()
        {
            var key = "test_key";
            var value = "test_value";

            await _dataCache.SetAsync(key, value);

            var result = await _dataCache.GetAsync<string>(key);
            Assert.Equal(value, result);
        }

        [Fact]
        public async Task GetAsync_WithNonExistentKey_ReturnsNull()
        {
            var key = "non_existent_key";

            var result = await _dataCache.GetAsync<string>(key);

            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_WithCustomExpiration_ExpiresAfterTimeout()
        {
            var key = "expiring_key";
            var value = "expiring_value";
            var expiration = TimeSpan.FromMilliseconds(100);

            await _dataCache.SetAsync(key, value, expiration);

            await Task.Delay(150);
            var result = await _dataCache.GetAsync<string>(key);

            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_WithDefaultExpiration_CachesForOneHour()
        {
            var key = "default_expiration_key";
            var value = "default_expiration_value";

            await _dataCache.SetAsync(key, value);

            var result = await _dataCache.GetAsync<string>(key);
            Assert.Equal(value, result);
        }

        [Fact]
        public async Task Remove_WithExistingKey_RemovesValueFromCache()
        {
            var key = "removable_key";
            var value = "removable_value";

            await _dataCache.SetAsync(key, value);
            _dataCache.Remove(key);

            var result = await _dataCache.GetAsync<string>(key);
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_WithNullKey_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _dataCache.SetAsync(null, "value"));
        }

        [Fact]
        public async Task SetAsync_WithNullValue_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _dataCache.SetAsync("key", null));
        }

        [Fact]
        public async Task GetAsync_WithNullKey_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _dataCache.GetAsync<string>(null));
        }

        [Fact]
        public void Remove_WithNullKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                _dataCache.Remove(null));
        }

        [Fact]
        public async Task SetAsync_WithMultipleObjects_StoresEachIndependently()
        {
            var key1 = "key1";
            var value1 = "value1";
            var key2 = "key2";
            var value2 = "value2";

            await _dataCache.SetAsync(key1, value1);
            await _dataCache.SetAsync(key2, value2);

            var result1 = await _dataCache.GetAsync<string>(key1);
            var result2 = await _dataCache.GetAsync<string>(key2);

            Assert.Equal(value1, result1);
            Assert.Equal(value2, result2);
        }
    }
}