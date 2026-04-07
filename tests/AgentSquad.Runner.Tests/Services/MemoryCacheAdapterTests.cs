using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace AgentSquad.Runner.Tests.Services;

public class MemoryCacheAdapterTests
{
    [Fact]
    public async Task SetAsync_StoresValue_AndGetAsyncRetrievesIt()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var adapter = new MemoryCacheAdapter(cache);
        var project = new Project { Name = "Test Project" };
        var key = "test_key";

        // Act
        await adapter.SetAsync(key, project);
        var result = await adapter.GetAsync<Project>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Project", result.Name);
    }

    [Fact]
    public async Task GetAsync_WithNonexistentKey_ReturnsNull()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var adapter = new MemoryCacheAdapter(cache);

        // Act
        var result = await adapter.GetAsync<Project>("nonexistent_key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Remove_DeletesKey_AndGetAsyncReturnsNull()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var adapter = new MemoryCacheAdapter(cache);
        var project = new Project { Name = "Test Project" };
        var key = "test_key";

        cache.Set(key, project);

        // Act
        adapter.Remove(key);
        var result = cache.Get<Project>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_WithExpiration_RemovesValueAfterExpiry()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var adapter = new MemoryCacheAdapter(cache);
        var project = new Project { Name = "Expiring Project" };
        var key = "expiring_key";
        var expiration = TimeSpan.FromMilliseconds(100);

        // Act
        await adapter.SetAsync(key, project, expiration);
        var resultBefore = await adapter.GetAsync<Project>(key);

        await Task.Delay(150);
        var resultAfter = await adapter.GetAsync<Project>(key);

        // Assert
        Assert.NotNull(resultBefore);
        Assert.Null(resultAfter);
    }

    [Fact]
    public async Task SetAsync_WithoutExpiration_UsesDefaultSliding()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var adapter = new MemoryCacheAdapter(cache);
        var project = new Project { Name = "Sliding Project" };
        var key = "sliding_key";

        // Act
        await adapter.SetAsync(key, project);
        var result = await adapter.GetAsync<Project>(key);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task MultipleValuesInCache_StoreAndRetrieveIndependently()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var adapter = new MemoryCacheAdapter(cache);
        var project1 = new Project { Name = "Project 1" };
        var project2 = new Project { Name = "Project 2" };

        // Act
        await adapter.SetAsync("key1", project1);
        await adapter.SetAsync("key2", project2);

        var result1 = await adapter.GetAsync<Project>("key1");
        var result2 = await adapter.GetAsync<Project>("key2");

        // Assert
        Assert.Equal("Project 1", result1.Name);
        Assert.Equal("Project 2", result2.Name);
    }
}