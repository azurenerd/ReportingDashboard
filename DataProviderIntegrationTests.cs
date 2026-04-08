using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Xunit;

namespace AgentSquad.Runner.Tests
{
    public class DataProviderIntegrationTests
    {
        private string GetProjectRootPath()
        {
            var assemblyLocation = typeof(DataProviderIntegrationTests).Assembly.Location;
            var directory = new DirectoryInfo(Path.GetDirectoryName(assemblyLocation));
            
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "AgentSquad.Runner.csproj")))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }

            throw new InvalidOperationException("Could not find project root");
        }

        [Fact]
        public async Task GetProjectDataAsync_LoadsDataFromJsonFile()
        {
            var projectRoot = GetProjectRootPath();
            var cache = new DataCache();
            var provider = new DataProvider(cache);

            var result = await provider.GetProjectDataAsync();

            Assert.NotNull(result);
            Assert.NotEmpty(result.ProjectName);
            Assert.True(provider.IsLoaded);
            Assert.Empty(provider.ErrorMessage);
        }

        [Fact]
        public async Task GetProjectDataAsync_ContainsValidMilestones()
        {
            var cache = new DataCache();
            var provider = new DataProvider(cache);

            var result = await provider.GetProjectDataAsync();

            Assert.NotNull(result.Milestones);
            Assert.True(result.Milestones.Count >= 5);
            
            foreach (var milestone in result.Milestones)
            {
                Assert.NotEmpty(milestone.Name);
                Assert.NotEmpty(milestone.Description);
                Assert.True(Enum.IsDefined(typeof(MilestoneStatus), milestone.Status));
            }
        }

        [Fact]
        public async Task GetProjectDataAsync_ContainsValidWorkItems()
        {
            var cache = new DataCache();
            var provider = new DataProvider(cache);

            var result = await provider.GetProjectDataAsync();

            Assert.NotNull(result.WorkItems);
            Assert.True(result.WorkItems.Count >= 12);
            
            foreach (var item in result.WorkItems)
            {
                Assert.NotEmpty(item.Title);
                Assert.NotEmpty(item.Description);
                Assert.NotEmpty(item.AssignedTo);
                Assert.True(Enum.IsDefined(typeof(WorkItemStatus), item.Status));
            }
        }

        [Fact]
        public async Task GetProjectDataAsync_ContainsValidMetrics()
        {
            var cache = new DataCache();
            var provider = new DataProvider(cache);

            var result = await provider.GetProjectDataAsync();

            Assert.NotNull(result.Metrics);
            Assert.InRange(result.Metrics.CompletionPercentage, 0, 100);
            Assert.True(Enum.IsDefined(typeof(HealthStatus), result.Metrics.HealthStatus));
            Assert.True(result.Metrics.VelocityCount >= 0);
            Assert.True(result.Metrics.TotalMilestones > 0);
            Assert.True(result.Metrics.CompletedMilestones >= 0);
        }

        [Fact]
        public async Task GetProjectDataAsync_CachesDataOnSecondCall()
        {
            var cache = new DataCache();
            var provider = new DataProvider(cache);

            var first = await provider.GetProjectDataAsync();
            var second = await provider.GetProjectDataAsync();

            Assert.Same(first, second);
        }

        [Fact]
        public async Task GetProjectDataAsync_SetsErrorMessageOnMissingFile()
        {
            var cache = new DataCache();
            var providerWithBadPath = new DataProvider(cache);
            
            var result = await providerWithBadPath.GetProjectDataAsync();

            Assert.Null(result);
            Assert.False(providerWithBadPath.IsLoaded);
            Assert.NotEmpty(providerWithBadPath.ErrorMessage);
        }
    }
}