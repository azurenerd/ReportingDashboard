using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests.Services
{
    public class MemoryCacheAdapterTests
    {
        [Fact]
        public async Task SetAsync_StoresValue_GetAsyncReturnsValue()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            var adapter = new MemoryCacheAdapter(cache);
            var key = "test_key";
            var value = "test_value";

            // Act
            await adapter.SetAsync(key, value, null);
            var result = await adapter.GetAsync<string>(key);

            // Assert
            Assert.Equal(value, result);
        }

        [Fact]
        public async Task GetAsync_WithNonexistentKey_ReturnsNull()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            var adapter = new MemoryCacheAdapter(cache);

            // Act
            var result = await adapter.GetAsync<string>("nonexistent");

            // Assert
            Assert.Null(result);
        }
    }
}