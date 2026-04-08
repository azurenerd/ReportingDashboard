using AgentSquad.Runner.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Services
{
    public class MemoryCacheProviderTests
    {
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<ILogger<MemoryCacheProvider>> _mockLogger;

        public MemoryCacheProviderTests()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _mockLogger = new Mock<ILogger<MemoryCacheProvider>>();
        }

        [Fact]
        public async Task SetAsync_StoresValueInCache()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test" };

            // Act
            await provider.SetAsync("test_key", testValue);

            // Assert
            var retrieved = await provider.GetAsync<TestData>("test_key");
            Assert.NotNull(retrieved);
            Assert.Equal(1, retrieved.Id);
            Assert.Equal("Test", retrieved.Name);
        }

        [Fact]
        public async Task GetAsync_ReturnsNullWhenKeyNotFound()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act
            var result = await provider.GetAsync<TestData>("nonexistent_key");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_WithNullKey_ReturnsNull()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act
            var result = await provider.GetAsync<TestData>(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_WithNullKey_DoesNotThrow()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test" };

            // Act & Assert
            await provider.SetAsync(null, testValue);
        }

        [Fact]
        public async Task SetAsync_WithNullValue_DoesNotThrow()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act & Assert
            await provider.SetAsync("test_key", null);
        }

        [Fact]
        public async Task Remove_DeletesKeyFromCache()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test" };
            await provider.SetAsync("test_key", testValue);

            // Act
            provider.Remove("test_key");

            // Assert
            var result = await provider.GetAsync<TestData>("test_key");
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_WithCustomTTL_ExpiresData()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test" };

            // Act
            await provider.SetAsync("test_key", testValue, TimeSpan.FromMilliseconds(100));

            // Assert - immediate retrieval should work
            var retrieved = await provider.GetAsync<TestData>("test_key");
            Assert.NotNull(retrieved);

            // Wait for expiration
            await Task.Delay(150);

            // Should return null after expiration
            var expired = await provider.GetAsync<TestData>("test_key");
            Assert.Null(expired);
        }

        [Fact]
        public async Task SetAsync_WithDefaultTTL_SetsOneHour()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test" };

            // Act
            await provider.SetAsync("test_key", testValue, null);

            // Assert - should still be in cache (not expired)
            var retrieved = await provider.GetAsync<TestData>("test_key");
            Assert.NotNull(retrieved);
        }

        [Fact]
        public async Task SetAsync_OverwritesExistingValue()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var value1 = new TestData { Id = 1, Name = "First" };
            var value2 = new TestData { Id = 2, Name = "Second" };

            await provider.SetAsync("test_key", value1);

            // Act
            await provider.SetAsync("test_key", value2);

            // Assert
            var retrieved = await provider.GetAsync<TestData>("test_key");
            Assert.Equal(2, retrieved.Id);
            Assert.Equal("Second", retrieved.Name);
        }

        [Fact]
        public async Task MultipleKeys_StoredIndependently()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var value1 = new TestData { Id = 1, Name = "First" };
            var value2 = new TestData { Id = 2, Name = "Second" };

            // Act
            await provider.SetAsync("key1", value1);
            await provider.SetAsync("key2", value2);

            // Assert
            var retrieved1 = await provider.GetAsync<TestData>("key1");
            var retrieved2 = await provider.GetAsync<TestData>("key2");

            Assert.Equal(1, retrieved1.Id);
            Assert.Equal(2, retrieved2.Id);
        }

        [Fact]
        public async Task Remove_WithNullKey_DoesNotThrow()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act & Assert
            provider.Remove(null);
        }

        private class TestData
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}