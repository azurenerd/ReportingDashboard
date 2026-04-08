using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests.Services;

public class DataCacheTests
{
    private readonly Mock<ILogger<DataCache>> _mockLogger;
    private readonly IMemoryCache _memoryCache;
    private readonly DataCache _dataCache;

    public DataCacheTests()
    {
        _mockLogger = new Mock<ILogger<DataCache>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _dataCache = new DataCache(_memoryCache, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAsync_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var key = "test-key";
        var testValue = new TestObject { Id = 1, Name = "Test" };
        await _dataCache.SetAsync(key, testValue);

        // Act
        var result = await _dataCache.GetAsync<TestObject>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testValue.Id, result.Id);
        Assert.Equal(testValue.Name, result.Name);
    }

    [Fact]
    public async Task GetAsync_WithNonExistentKey_ReturnsNull()
    {
        // Act
        var result = await _dataCache.GetAsync<TestObject>("non-existent-key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WithNullKey_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _dataCache.GetAsync<TestObject>(null!));
    }

    [Fact]
    public async Task GetAsync_WithWhitespaceKey_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _dataCache.GetAsync<TestObject>("   "));
    }

    [Fact]
    public async Task SetAsync_WithValidKeyAndValue_CachesValue()
    {
        // Arrange
        var key = "test-key";
        var testValue = new TestObject { Id = 1, Name = "Test" };

        // Act
        await _dataCache.SetAsync(key, testValue);
        var result = await _dataCache.GetAsync<TestObject>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testValue.Id, result.Id);
    }

    [Fact]
    public async Task SetAsync_WithCustomExpiration_CachesWithExpiration()
    {
        // Arrange
        var key = "test-key";
        var testValue = new TestObject { Id = 1, Name = "Test" };
        var expiration = TimeSpan.FromMilliseconds(100);

        // Act
        await _dataCache.SetAsync(key, testValue, expiration);
        var resultBefore = await _dataCache.GetAsync<TestObject>(key);
        
        await Task.Delay(150);
        var resultAfter = await _dataCache.GetAsync<TestObject>(key);

        // Assert
        Assert.NotNull(resultBefore);
        Assert.Null(resultAfter);
    }

    [Fact]
    public async Task SetAsync_WithDefaultExpiration_CachesFor1Hour()
    {
        // Arrange
        var key = "test-key";
        var testValue = new TestObject { Id = 1, Name = "Test" };

        // Act
        await _dataCache.SetAsync(key, testValue);
        var result = await _dataCache.GetAsync<TestObject>(key);

        // Assert - Value should exist (1 hour TTL not expired immediately)
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SetAsync_WithNullKey_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _dataCache.SetAsync(null!, new TestObject { Id = 1 }));
    }

    [Fact]
    public async Task SetAsync_WithNullValue_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _dataCache.SetAsync("key", (TestObject)null!));
    }

    [Fact]
    public async Task Remove_WithExistingKey_DeletesValue()
    {
        // Arrange
        var key = "test-key";
        var testValue = new TestObject { Id = 1, Name = "Test" };
        await _dataCache.SetAsync(key, testValue);

        // Act
        _dataCache.Remove(key);
        var result = await _dataCache.GetAsync<TestObject>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Remove_WithNonExistentKey_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        _dataCache.Remove("non-existent-key");
    }

    [Fact]
    public void Remove_WithNullKey_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _dataCache.Remove(null!));
    }

    [Fact]
    public async Task SetAsync_LogsDebugMessage()
    {
        // Arrange
        var key = "test-key";
        var testValue = new TestObject { Id = 1, Name = "Test" };

        // Act
        await _dataCache.SetAsync(key, testValue);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(key)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Remove_LogsDebugMessage()
    {
        // Arrange
        var key = "test-key";

        // Act
        _dataCache.Remove(key);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(key)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_ReturnsSameObjectReference()
    {
        // Arrange
        var key = "test-key";
        var testValue = new TestObject { Id = 1, Name = "Test" };
        await _dataCache.SetAsync(key, testValue);

        // Act
        var result1 = await _dataCache.GetAsync<TestObject>(key);
        var result2 = await _dataCache.GetAsync<TestObject>(key);

        // Assert
        Assert.Same(result1, result2);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}