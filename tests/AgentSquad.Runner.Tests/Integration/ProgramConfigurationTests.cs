using AgentSquad.Runner.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AgentSquad.Runner.Tests.Integration
{
    public class ProgramConfigurationTests
    {
        [Fact]
        public void ProgramConfiguration_RegistersDataCacheAsScoped()
        {
            var services = new ServiceCollection();
            services.AddScoped<IDataCache, MemoryCacheAdapter>();

            var provider = services.BuildServiceProvider();
            var cache1 = provider.GetRequiredService<IDataCache>();
            var cache2 = provider.GetRequiredService<IDataCache>();

            Assert.NotNull(cache1);
            Assert.NotSame(cache1, cache2);
        }

        [Fact]
        public void ProgramConfiguration_RegistersDataProvider_WithConstructorParameters()
        {
            var services = new ServiceCollection();
            services.AddScoped<IDataCache, MemoryCacheAdapter>();
            
            var dataPath = Path.Combine(Path.GetTempPath(), "data.json");
            var testJson = @"{ ""name"": ""Test"", ""milestones"": [{ ""name"": ""M1"", ""completionPercentage"": 50 }], ""workItems"": [] }";
            File.WriteAllText(dataPath, testJson);

            try
            {
                services.AddScoped<DataProvider>(sp =>
                    new DataProvider(sp.GetRequiredService<IDataCache>(), dataPath)
                );

                var provider = services.BuildServiceProvider();
                var dataProvider = provider.GetRequiredService<DataProvider>();

                Assert.NotNull(dataProvider);
                var project = dataProvider.LoadProject();
                Assert.Equal("Test", project.Name);
            }
            finally
            {
                if (File.Exists(dataPath)) File.Delete(dataPath);
            }
        }

        [Fact]
        public void ProgramConfiguration_DataProvider_InjectsIDataCache()
        {
            var services = new ServiceCollection();
            var mockCache = new MemoryCacheAdapter();
            services.AddScoped<IDataCache>(_ => mockCache);

            var dataPath = Path.Combine(Path.GetTempPath(), "config_test.json");
            var json = @"{ ""name"": ""Config"", ""milestones"": [{ ""name"": ""M1"", ""completionPercentage"": 50 }], ""workItems"": [] }";
            File.WriteAllText(dataPath, json);

            try
            {
                services.AddScoped<DataProvider>(sp =>
                    new DataProvider(sp.GetRequiredService<IDataCache>(), dataPath)
                );

                var provider = services.BuildServiceProvider();
                var dataProvider = provider.GetRequiredService<DataProvider>();

                Assert.NotNull(dataProvider);
            }
            finally
            {
                if (File.Exists(dataPath)) File.Delete(dataPath);
            }
        }

        [Fact]
        public void ProgramConfiguration_DataProvider_AcceptsFilePath()
        {
            var cache = new MemoryCacheAdapter();
            var filePath = Path.Combine(Path.GetTempPath(), "filepath_test.json");
            var json = @"{ ""name"": ""PathTest"", ""milestones"": [{ ""name"": ""M1"", ""completionPercentage"": 50 }], ""workItems"": [] }";
            File.WriteAllText(filePath, json);

            try
            {
                var provider = new DataProvider(cache, filePath);
                var project = provider.LoadProject();

                Assert.Equal("PathTest", project.Name);
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }
    }
}