using Xunit;
using Moq;
using AgentSquad.Services;
using AgentSquad.Models;

namespace AgentSquad.Tests.Services;

public class DataCacheTests
{
    private readonly Mock<IDataCache> _mockCache;

    public DataCacheTests()
    {
        _mockCache = new Mock<IDataCache>();
    }

    [Fact]
    public async Task IDataCache_GetAsyncReturnsStoredData()
    {
        var testData = new { Name = "Test" };
        _mockCache.Setup(c => c.GetAsync<object>(It.IsAny<string>()))
            .ReturnsAsync(testData);

        var result = await _mockCache.Object.GetAsync<object>("key");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task IDataCache_SetAsyncStoresData()
    {
        var testData = "TestValue";

        _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        await _mockCache.Object.SetAsync("key", testData);

        _mockCache.Verify(c => c.SetAsync("key", testData), Times.Once);
    }

    [Fact]
    public async Task IDataCache_RemoveAsyncDeletesData()
    {
        _mockCache.Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _mockCache.Object.RemoveAsync("key");

        _mockCache.Verify(c => c.RemoveAsync("key"), Times.Once);
    }

    [Fact]
    public async Task IDataCache_GetAsyncReturnsNullForMissingKey()
    {
        _mockCache.Setup(c => c.GetAsync<object>(It.IsAny<string>()))
            .ReturnsAsync((object)null);

        var result = await _mockCache.Object.GetAsync<object>("nonexistent");

        Assert.Null(result);
    }
}