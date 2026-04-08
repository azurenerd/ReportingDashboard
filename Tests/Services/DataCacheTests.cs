using Microsoft.Extensions.Caching.Memory;
using AgentSquad.Runner.Services;
using Xunit;

namespace AgentSquad.Runner.Tests.Services;

public class DataCacheTests : IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly DataCache _dataCache;

    public DataCacheTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _dataCache = new DataCache(_memoryCache);
    }

    public void Dispose()
    {
        _memoryCache?.Dispose();
    }

    [Fact]
    public async Task GetAsync_WhenKeyNotFound_ReturnsNull()
    {
        var result = await _dataCache.GetAsync<TestData>("nonexistent_key");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WhenKeyFound_ReturnsValue()
    {
        var testData = new TestData { Id = 1, Name = "Test" };
        await _dataCache.SetAsync("test_key", testData);

        var result = await _dataCache.GetAsync<TestData>("test_key");

        Assert.NotNull(result);
        Assert.Equal(testData.Id, result.Id);
        Assert.Equal(testData.Name, result.Name);
    }

    [Fact]
    public async Task GetAsync_WithNullKey_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _dataCache.GetAsync<TestData>(null));
    }

    [Fact]
    public async Task GetAsync_WithEmptyKey_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _dataCache.GetAsync<TestData>(string.Empty));
    }

    [Fact]
    public async Task SetAsync_WithValidData_StoresInCache()
    {
        var testData = new TestData { Id = 42, Name = "Cached Item" };
        await _dataCache.SetAsync("valid_key", testData);

        var retrieved = await _dataCache.GetAsync<TestData>("valid_key");

        Assert.NotNull(retrieved);
        Assert.Equal(42, retrieved.Id);
        Assert.Equal("Cached Item", retrieved.Name);
    }

    [Fact]
    public async Task SetAsync_WithDefaultTTL_ExpiresAfterOneHour()
    {
        var testData = new TestData { Id = 1, Name = "Expiring Data" };
        await _dataCache.SetAsync("expiring_key", testData);

        var retrieved = await _dataCache.GetAsync<TestData>("expiring_key");
        Assert.NotNull(retrieved);
    }

    [Fact]
    public async Task SetAsync_WithCustomTTL_UsesProvidedExpiration()
    {
        var testData = new TestData { Id = 1, Name = "Custom TTL Data" };
        var customTtl = TimeSpan.FromMinutes(5);
        
        await _dataCache.SetAsync("custom_ttl_key", testData, customTtl);
        var retrieved = await _dataCache.GetAsync<TestData>("custom_ttl_key");

        Assert.NotNull(retrieved);
    }

    [Fact]
    public async Task SetAsync_WithNullKey_ThrowsArgumentException()
    {
        var testData = new TestData { Id = 1, Name = "Test" };
        await Assert.ThrowsAsync<ArgumentException>(() => _dataCache.SetAsync(null, testData));
    }

    [Fact]
    public async Task SetAsync_WithEmptyKey_ThrowsArgumentException()
    {
        var testData = new TestData { Id = 1, Name = "Test" };
        await Assert.ThrowsAsync<ArgumentException>(() => _dataCache.SetAsync(string.Empty, testData));
    }

    [Fact]
    public async Task SetAsync_WithNullValue_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _dataCache.SetAsync("some_key", null));
    }

    [Fact]
    public void Remove_WhenKeyExists_RemovesFromCache()
    {
        var testData = new TestData { Id = 1, Name = "To Remove" };
        _dataCache.SetAsync("remove_key", testData).Wait();

        _dataCache.Remove("remove_key");

        var retrieved = _dataCache.GetAsync<TestData>("remove_key").Result;
        Assert.Null(retrieved);
    }

    [Fact]
    public void Remove_WhenKeyNotExists_DoesNotThrow()
    {
        _dataCache.Remove("nonexistent_remove_key");
    }

    [Fact]
    public void Remove_WithNullKey_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _dataCache.Remove(null));
    }

    [Fact]
    public void Remove_WithEmptyKey_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _dataCache.Remove(string.Empty));
    }

    [Fact]
    public async Task SetAsync_MultipleKeys_StoresIndependently()
    {
        var data1 = new TestData { Id = 1, Name = "First" };
        var data2 = new TestData { Id = 2, Name = "Second" };

        await _dataCache.SetAsync("key1", data1);
        await _dataCache.SetAsync("key2", data2);

        var retrieved1 = await _dataCache.GetAsync<TestData>("key1");
        var retrieved2 = await _dataCache.GetAsync<TestData>("key2");

        Assert.NotNull(retrieved1);
        Assert.NotNull(retrieved2);
        Assert.Equal("First", retrieved1.Name);
        Assert.Equal("Second", retrieved2.Name);
    }

    [Fact]
    public async Task SetAsync_OverwriteExistingKey_UpdatesValue()
    {
        var originalData = new TestData { Id = 1, Name = "Original" };
        var updatedData = new TestData { Id = 1, Name = "Updated" };

        await _dataCache.SetAsync("overwrite_key", originalData);
        await _dataCache.SetAsync("overwrite_key", updatedData);

        var retrieved = await _dataCache.GetAsync<TestData>("overwrite_key");

        Assert.NotNull(retrieved);
        Assert.Equal("Updated", retrieved.Name);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}