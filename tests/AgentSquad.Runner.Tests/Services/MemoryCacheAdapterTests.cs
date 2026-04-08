using Xunit;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using System.Threading.Tasks;

namespace AgentSquad.Runner.Tests.Services
{
    public class MemoryCacheAdapterTests
    {
        [Fact]
        public async Task SetAsync_StoresValueInCache()
        {
            var cache = new MemoryCacheAdapter();
            var testData = new Project { Milestones = [] };

            await cache.SetAsync("project", testData, TimeSpan.FromMinutes(5));

            var result = await cache.GetAsync<Project>("project");
            Assert.NotNull(result);
            Assert.Equal(testData.Milestones.Count, result.Milestones.Count);
        }

        [Fact]
        public async Task GetAsync_ReturnsNullForMissingKey()
        {
            var cache = new MemoryCacheAdapter();
            var result = await cache.GetAsync<Project>("nonexistent");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_ReturnsNullAfterExpiration()
        {
            var cache = new MemoryCacheAdapter();
            var testData = new Project { Milestones = [] };

            await cache.SetAsync("project", testData, TimeSpan.FromMilliseconds(50));
            await Task.Delay(100);

            var result = await cache.GetAsync<Project>("project");
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_OverwritesPreviousValue()
        {
            var cache = new MemoryCacheAdapter();
            var data1 = new Project { Milestones = [] };
            var data2 = new Project { Milestones = new() { new Milestone { Name = "Q1" } } };

            await cache.SetAsync("project", data1, TimeSpan.FromMinutes(5));
            await cache.SetAsync("project", data2, TimeSpan.FromMinutes(5));

            var result = await cache.GetAsync<Project>("project");
            Assert.Single(result.Milestones);
        }
    }
}