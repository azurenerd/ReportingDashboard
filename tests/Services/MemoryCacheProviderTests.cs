using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Models;
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
        public void Set_AddsItemToCache()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var project = new Project { Name = "Test" };

            cache.Set("key", project);

            Assert.True(cache.Exists("key"));
        }

        [Fact]
        public void Get_ReturnsItem_WhenExists()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var project = new Project { Name = "Test Project" };

            cache.Set("testKey", project);
            var result = cache.Get<Project>("testKey");

            Assert.NotNull(result);
            Assert.Equal("Test Project", result.Name);
        }

        [Fact]
        public void Get_ReturnsNull_WhenNotExists()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            var result = cache.Get<Project>("nonexistentKey");

            Assert.Null(result);
        }

        [Fact]
        public void Set_WithExpiration_RemovesItem_AfterTTL()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var project = new Project { Name = "Expiring" };

            cache.Set("expiringKey", project, TimeSpan.FromMilliseconds(100));
            Assert.True(cache.Exists("expiringKey"));

            Thread.Sleep(150);
            Assert.False(cache.Exists("expiringKey"));
        }

        [Fact]
        public void Remove_DeletesItemFromCache()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var project = new Project { Name = "ToRemove" };

            cache.Set("removeKey", project);
            Assert.True(cache.Exists("removeKey"));

            cache.Remove("removeKey");
            Assert.False(cache.Exists("removeKey"));
        }

        [Fact]
        public void Exists_ReturnsFalse_WhenKeyNotInCache()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            Assert.False(cache.Exists("nonexistent"));
        }

        [Fact]
        public void Exists_ReturnsTrue_WhenKeyInCache()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var project = new Project { Name = "Test" };

            cache.Set("existsKey", project);
            Assert.True(cache.Exists("existsKey"));
        }

        [Fact]
        public void Set_ThrowsArgumentException_WhenKeyNull()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var project = new Project();

            Assert.Throws<ArgumentException>(() => cache.Set(null, project));
        }

        [Fact]
        public void Set_ThrowsArgumentNullException_WhenValueNull()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            Assert.Throws<ArgumentNullException>(() => cache.Set("key", (Project)null));
        }

        [Fact]
        public void Get_ThrowsArgumentException_WhenKeyNull()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            Assert.Throws<ArgumentException>(() => cache.Get<Project>(null));
        }

        [Fact]
        public void Remove_ThrowsArgumentException_WhenKeyNull()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            Assert.Throws<ArgumentException>(() => cache.Remove(null));
        }

        [Fact]
        public void Exists_ThrowsArgumentException_WhenKeyNull()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            Assert.Throws<ArgumentException>(() => cache.Exists(null));
        }

        [Fact]
        public void Set_WithEmptyExpirationTimespan_StoresIndefinitely()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);
            var project = new Project { Name = "Persistent" };

            cache.Set("persistKey", project, null);
            Assert.True(cache.Exists("persistKey"));
        }

        [Fact]
        public void AcceptanceCriteria_ImplementsIDataCache()
        {
            var cache = new MemoryCacheProvider(_memoryCache, _mockLogger.Object);

            Assert.IsAssignableFrom<IDataCache>(cache);
        }
    }
}