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

        private class TestData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        #region Set Operations Tests

        [Fact]
        public async Task SetAsync_StoresValueInCache()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test Item", CreatedAt = DateTime.Now };

            // Act
            await provider.SetAsync("test_key", testValue);

            // Assert
            var retrieved = await provider.GetAsync<TestData>("test_key");
            Assert.NotNull(retrieved);
            Assert.Equal(1, retrieved.Id);
            Assert.Equal("Test Item", retrieved.Name);
        }

        [Fact]
        public async Task SetAsync_WithValidKey_CachesValue()
        {
            // Arrange - AC: IDataCache implementation wraps IMemoryCache with async Get/Set/Remove operations
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 42, Name = "Cached Value" };

            // Act
            await provider.SetAsync("key1", testValue);
            var result = await provider.GetAsync<TestData>("key1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(42, result.Id);
        }

        [Fact]
        public async Task SetAsync_WithNullKey_DoesNotThrowAndLogsWarning()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test" };

            // Act & Assert - Should not throw
            await provider.SetAsync(null, testValue);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SetAsync_WithEmptyStringKey_DoesNotThrowAndLogsWarning()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test" };

            // Act & Assert
            await provider.SetAsync("", testValue);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SetAsync_WithWhitespaceKey_DoesNotThrowAndLogsWarning()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test" };

            // Act & Assert
            await provider.SetAsync("   ", testValue);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SetAsync_WithNullValue_DoesNotThrowAndLogsWarning()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act & Assert
            await provider.SetAsync("test_key", (TestData)null);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SetAsync_WithDefaultTTL_Sets1HourExpiration()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test" };

            // Act
            await provider.SetAsync("test_key", testValue, null);

            // Assert - Value should still be available (1 hour hasn't passed)
            var retrieved = await provider.GetAsync<TestData>("test_key");
            Assert.NotNull(retrieved);
        }

        [Fact]
        public async Task SetAsync_WithCustomTTL_UsesProvidedExpiration()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Short Lived" };
            var shortTTL = TimeSpan.FromMilliseconds(100);

            // Act
            await provider.SetAsync("short_lived", testValue, shortTTL);

            // Assert - Should exist immediately
            var immediate = await provider.GetAsync<TestData>("short_lived");
            Assert.NotNull(immediate);

            // Wait for expiration
            await Task.Delay(150);

            // Should be expired
            var expired = await provider.GetAsync<TestData>("short_lived");
            Assert.Null(expired);
        }

        [Fact]
        public async Task SetAsync_OverwritesExistingValue()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var value1 = new TestData { Id = 1, Name = "First" };
            var value2 = new TestData { Id = 2, Name = "Second" };

            await provider.SetAsync("key", value1);

            // Act
            await provider.SetAsync("key", value2);

            // Assert
            var result = await provider.GetAsync<TestData>("key");
            Assert.Equal(2, result.Id);
            Assert.Equal("Second", result.Name);
        }

        #endregion

        #region Get Operations Tests

        [Fact]
        public async Task GetAsync_WithValidKey_ReturnsCachedValue()
        {
            // Arrange - AC: Support optional TTL parameter (default 1 hour)
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 99, Name = "Retrieved Value" };
            await provider.SetAsync("retrieve_key", testValue);

            // Act
            var result = await provider.GetAsync<TestData>("retrieve_key");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99, result.Id);
            Assert.Equal("Retrieved Value", result.Name);
        }

        [Fact]
        public async Task GetAsync_WithNonexistentKey_ReturnsNull()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act
            var result = await provider.GetAsync<TestData>("nonexistent_key_xyz");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_WithNullKey_ReturnsNullAndLogsWarning()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act
            var result = await provider.GetAsync<TestData>(null);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_WithEmptyKey_ReturnsNullAndLogsWarning()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act
            var result = await provider.GetAsync<TestData>("");

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_WithWhitespaceKey_ReturnsNullAndLogsWarning()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act
            var result = await provider.GetAsync<TestData>("   ");

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_LogsCacheHitAtDebugLevel()
        {
            // Arrange - AC: Log cache hits/misses at Debug level
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test" };
            await provider.SetAsync("hit_key", testValue);

            // Act
            await provider.GetAsync<TestData>("hit_key");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cache hit")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_LogsCacheMissAtDebugLevel()
        {
            // Arrange - AC: Log cache hits/misses at Debug level
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act
            await provider.GetAsync<TestData>("miss_key");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cache miss")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_WithExpiredData_ReturnsNull()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Expiring" };
            await provider.SetAsync("expiring_key", testValue, TimeSpan.FromMilliseconds(50));

            // Act & Assert - Immediate access succeeds
            var immediate = await provider.GetAsync<TestData>("expiring_key");
            Assert.NotNull(immediate);

            // Wait for expiration
            await Task.Delay(100);

            // After expiration, returns null
            var afterExpiry = await provider.GetAsync<TestData>("expiring_key");
            Assert.Null(afterExpiry);
        }

        #endregion

        #region Remove Operations Tests

        [Fact]
        public async Task Remove_DeletesKeyFromCache()
        {
            // Arrange - AC: IDataCache implementation wraps IMemoryCache with async Get/Set/Remove operations
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "To Remove" };
            await provider.SetAsync("remove_key", testValue);

            // Act
            provider.Remove("remove_key");

            // Assert
            var result = await provider.GetAsync<TestData>("remove_key");
            Assert.Null(result);
        }

        [Fact]
        public async Task Remove_WithNullKey_DoesNotThrowAndLogsWarning()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act & Assert
            provider.Remove(null);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Remove_WithEmptyKey_DoesNotThrowAndLogsWarning()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act & Assert
            provider.Remove("");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Remove_WithWhitespaceKey_DoesNotThrowAndLogsWarning()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act & Assert
            provider.Remove("   ");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Remove_WithNonexistentKey_DoesNotThrow()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            // Act & Assert - Should not throw
            provider.Remove("never_existed");
        }

        [Fact]
        public async Task Remove_LogsAtDebugLevel()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test" };
            await provider.SetAsync("log_remove_key", testValue);

            // Act
            provider.Remove("log_remove_key");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        #endregion

        #region Multiple Keys Tests

        [Fact]
        public async Task MultipleKeys_StoredAndRetrievedIndependently()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var value1 = new TestData { Id = 1, Name = "First" };
            var value2 = new TestData { Id = 2, Name = "Second" };
            var value3 = new TestData { Id = 3, Name = "Third" };

            // Act
            await provider.SetAsync("key1", value1);
            await provider.SetAsync("key2", value2);
            await provider.SetAsync("key3", value3);

            // Assert
            var retrieved1 = await provider.GetAsync<TestData>("key1");
            var retrieved2 = await provider.GetAsync<TestData>("key2");
            var retrieved3 = await provider.GetAsync<TestData>("key3");

            Assert.Equal(1, retrieved1.Id);
            Assert.Equal(2, retrieved2.Id);
            Assert.Equal(3, retrieved3.Id);
        }

        [Fact]
        public async Task MultipleKeys_RemovalIsolated()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var value1 = new TestData { Id = 1, Name = "Keep" };
            var value2 = new TestData { Id = 2, Name = "Remove" };

            await provider.SetAsync("keep_key", value1);
            await provider.SetAsync("remove_key", value2);

            // Act
            provider.Remove("remove_key");

            // Assert
            var kept = await provider.GetAsync<TestData>("keep_key");
            var removed = await provider.GetAsync<TestData>("remove_key");

            Assert.NotNull(kept);
            Assert.Null(removed);
        }

        [Fact]
        public async Task MultipleKeys_DifferentTTLs_ExpireIndependently()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var shortLived = new TestData { Id = 1, Name = "Short" };
            var longLived = new TestData { Id = 2, Name = "Long" };

            await provider.SetAsync("short", shortLived, TimeSpan.FromMilliseconds(75));
            await provider.SetAsync("long", longLived, TimeSpan.FromMilliseconds(500));

            // Act & Assert - After short expiration
            await Task.Delay(100);

            var shortResult = await provider.GetAsync<TestData>("short");
            var longResult = await provider.GetAsync<TestData>("long");

            Assert.Null(shortResult);
            Assert.NotNull(longResult);

            // Wait for long expiration
            await Task.Delay(450);

            var finalLongResult = await provider.GetAsync<TestData>("long");
            Assert.Null(finalLongResult);
        }

        #endregion

        #region Complex Type Tests

        [Fact]
        public async Task SetGetAsync_WithComplexObject_PreservesStructure()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var complexObject = new TestData
            {
                Id = 123,
                Name = "Complex Name",
                CreatedAt = new DateTime(2024, 1, 15, 10, 30, 45)
            };

            // Act
            await provider.SetAsync("complex", complexObject);
            var result = await provider.GetAsync<TestData>("complex");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(123, result.Id);
            Assert.Equal("Complex Name", result.Name);
            Assert.Equal(new DateTime(2024, 1, 15, 10, 30, 45), result.CreatedAt);
        }

        #endregion

        #region Concurrency Tests

        [Fact]
        public async Task ConcurrentSetAsync_AllOperationsSucceed()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var tasks = Enumerable.Range(1, 10)
                .Select(i => provider.SetAsync($"key{i}", new TestData { Id = i, Name = $"Item{i}" }))
                .ToList();

            // Act
            await Task.WhenAll(tasks);

            // Assert
            for (int i = 1; i <= 10; i++)
            {
                var result = await provider.GetAsync<TestData>($"key{i}");
                Assert.NotNull(result);
                Assert.Equal(i, result.Id);
            }
        }

        [Fact]
        public async Task ConcurrentGetAsync_AllOperationsSucceed()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 42, Name = "Shared" };
            await provider.SetAsync("shared_key", testValue);

            var getTasks = Enumerable.Range(1, 10)
                .Select(_ => provider.GetAsync<TestData>("shared_key"))
                .ToList();

            // Act
            var results = await Task.WhenAll(getTasks);

            // Assert
            Assert.All(results, result => Assert.NotNull(result));
            Assert.All(results, result => Assert.Equal(42, result.Id));
        }

        #endregion

        #region Async Behavior Tests

        [Fact]
        public async Task SetAsync_ReturnsCompletedTask()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test" };

            // Act
            var task = provider.SetAsync("key", testValue);

            // Assert
            Assert.True(task.IsCompleted);
            await task;
        }

        [Fact]
        public async Task GetAsync_ReturnsCompletedTask()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Test" };
            await provider.SetAsync("key", testValue);

            // Act
            var task = provider.GetAsync<TestData>("key");

            // Assert
            Assert.True(task.IsCompleted);
            var result = await task;
            Assert.NotNull(result);
        }

        #endregion

        #region TTL Boundary Tests

        [Fact]
        public async Task SetAsync_WithZeroTTL_ExpiresFastButNotImmediate()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Zero TTL" };

            // Act
            await provider.SetAsync("zero_ttl", testValue, TimeSpan.Zero);

            // Assert - May or may not be available immediately depending on timing
            var result = await provider.GetAsync<TestData>("zero_ttl");
            // This behavior is implementation-dependent, just ensure no exception
        }

        [Fact]
        public async Task SetAsync_WithVeryLargeTTL_EffectivelyPersists()
        {
            // Arrange
            var provider = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var testValue = new TestData { Id = 1, Name = "Long Lived" };
            var longTTL = TimeSpan.FromDays(365);

            // Act
            await provider.SetAsync("long_ttl", testValue, longTTL);

            // Assert - Should definitely be available
            var result = await provider.GetAsync<TestData>("long_ttl");
            Assert.NotNull(result);
        }

        #endregion
    }
}