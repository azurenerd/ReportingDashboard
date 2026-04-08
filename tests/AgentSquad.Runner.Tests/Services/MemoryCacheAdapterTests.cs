using AgentSquad.Runner.Services;
using Xunit;

namespace AgentSquad.Runner.Tests.Services
{
    public class MemoryCacheAdapterTests
    {
        [Fact]
        public void MemoryCacheAdapter_StoresAndRetrievesValue()
        {
            var cache = new MemoryCacheAdapter();
            var testValue = new { Name = "Test" };

            cache.Set("key1", testValue, TimeSpan.FromSeconds(60));
            var result = cache.Get<dynamic>("key1");

            Assert.NotNull(result);
        }

        [Fact]
        public void MemoryCacheAdapter_ReturnsNullForExpiredValue()
        {
            var cache = new MemoryCacheAdapter();
            cache.Set("expiring_key", "value", TimeSpan.FromMilliseconds(100));

            Thread.Sleep(150);
            var result = cache.Get<string>("expiring_key");

            Assert.Null(result);
        }

        [Fact]
        public void MemoryCacheAdapter_HonorsTimeSpanExpiration()
        {
            var cache = new MemoryCacheAdapter();
            var testData = "test_value";

            cache.Set("timed_key", testData, TimeSpan.FromSeconds(1));
            var immediate = cache.Get<string>("timed_key");
            Assert.NotNull(immediate);

            Thread.Sleep(1100);
            var expired = cache.Get<string>("timed_key");
            Assert.Null(expired);
        }

        [Fact]
        public void MemoryCacheAdapter_HandlesNullKey()
        {
            var cache = new MemoryCacheAdapter();
            Assert.Throws<ArgumentNullException>(() =>
                cache.Set(null, "value", TimeSpan.FromSeconds(60))
            );
        }

        [Fact]
        public void MemoryCacheAdapter_RemovesExpiredEntries()
        {
            var cache = new MemoryCacheAdapter();
            cache.Set("key_to_expire", "data", TimeSpan.FromMilliseconds(50));

            Thread.Sleep(100);
            cache.Set("another_key", "value", TimeSpan.FromSeconds(60));

            var expired = cache.Get<string>("key_to_expire");
            Assert.Null(expired);

            var valid = cache.Get<string>("another_key");
            Assert.NotNull(valid);
        }

        [Fact]
        public async Task MemoryCacheAdapter_CompletesAsyncOperations()
        {
            var cache = new MemoryCacheAdapter();
            cache.Set("async_key", "async_value", TimeSpan.FromSeconds(60));

            var task = Task.Run(() => cache.Get<string>("async_key"));
            var result = await task;

            Assert.NotNull(result);
            Assert.Equal("async_value", result);
        }
    }
}