using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Hosting;

namespace AgentSquad.Runner.Tests
{
    public class DataProviderTests
    {
        [Fact]
        public async Task LoadProjectDataAsync_WithValidJsonFile_DeserializesSuccessfully()
        {
            var mockLogger = new Mock<ILogger<DataProvider>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var dataCache = new MemoryCacheDataProvider(memoryCache);
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var jsonData = new
                {
                    name = "Test Project",
                    description = "Test Description",
                    startDate = "2024-01-15",
                    milestones = new[]
                    {
                        new { name = "M1", targetDate = "2024-02-15", status = "Completed", description = "Milestone 1" }
                    },
                    workItems = new object[] { },
                    metrics = new { completionPercentage = 72, healthStatus = "OnTrack", velocityCount = 12 }
                };

                string jsonPath = Path.Combine(tempDir, "data.json");
                string jsonContent = JsonSerializer.Serialize(jsonData);
                await File.WriteAllTextAsync(jsonPath, jsonContent);

                var mockHostEnvironment = new Mock<IWebHostEnvironment>();
                mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(tempDir);

                var provider = new DataProvider(mockLogger.Object, dataCache, mockHostEnvironment.Object);

                var project = await provider.LoadProjectDataAsync();

                Assert.NotNull(project);
                Assert.Equal("Test Project", project.Name);
                Assert.Single(project.Milestones);
                Assert.Empty(project.WorkItems);
                Assert.Equal(72, project.Metrics.CompletionPercentage);
                Assert.Equal(HealthStatus.OnTrack, project.Metrics.HealthStatus);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsFileNotFoundException()
        {
            var mockLogger = new Mock<ILogger<DataProvider>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var dataCache = new MemoryCacheDataProvider(memoryCache);
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var mockHostEnvironment = new Mock<IWebHostEnvironment>();
            mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(tempDir);

            var provider = new DataProvider(mockLogger.Object, dataCache, mockHostEnvironment.Object);

            await Assert.ThrowsAsync<FileNotFoundException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidJson_ThrowsInvalidOperationException()
        {
            var mockLogger = new Mock<ILogger<DataProvider>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var dataCache = new MemoryCacheDataProvider(memoryCache);
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                string jsonPath = Path.Combine(tempDir, "data.json");
                await File.WriteAllTextAsync(jsonPath, "{ invalid json }");

                var mockHostEnvironment = new Mock<IWebHostEnvironment>();
                mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(tempDir);

                var provider = new DataProvider(mockLogger.Object, dataCache, mockHostEnvironment.Object);

                await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_CachesProjectData()
        {
            var mockLogger = new Mock<ILogger<DataProvider>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var dataCache = new MemoryCacheDataProvider(memoryCache);
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var jsonData = new
                {
                    name = "Cached Project",
                    description = "Test",
                    startDate = "2024-01-15",
                    milestones = new[] { new { name = "M1", targetDate = "2024-02-15", status = "Completed", description = "M1" } },
                    workItems = new object[] { },
                    metrics = new { completionPercentage = 50, healthStatus = "OnTrack", velocityCount = 5 }
                };

                string jsonPath = Path.Combine(tempDir, "data.json");
                await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(jsonData));

                var mockHostEnvironment = new Mock<IWebHostEnvironment>();
                mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(tempDir);

                var provider = new DataProvider(mockLogger.Object, dataCache, mockHostEnvironment.Object);

                var project1 = await provider.LoadProjectDataAsync();
                var project2 = await provider.LoadProjectDataAsync();

                Assert.Same(project1, project2);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }
    }
}