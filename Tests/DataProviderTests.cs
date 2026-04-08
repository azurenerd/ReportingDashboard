using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests
{
    public class DataProviderTests
    {
        [Fact]
        public async Task LoadProjectDataAsync_WithValidJsonFile_DeserializesSuccessfully()
        {
            var mockLogger = new Mock<ILogger<DataProvider>>();
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
                        new { name = "M1", targetDate = "2024-02-15", status = "Completed", description = "Milestone 1" },
                        new { name = "M2", targetDate = "2024-03-15", status = "InProgress", description = "Milestone 2" },
                        new { name = "M3", targetDate = "2024-04-15", status = "AtRisk", description = "Milestone 3" },
                        new { name = "M4", targetDate = "2024-05-15", status = "Future", description = "Milestone 4" }
                    },
                    workItems = new[]
                    {
                        new { title = "Item 1", description = "Desc 1", status = "Shipped", category = "Backend" },
                        new { title = "Item 2", description = "Desc 2", status = "Shipped", category = "Frontend" },
                        new { title = "Item 3", description = "Desc 3", status = "InProgress", category = "Backend" },
                        new { title = "Item 4", description = "Desc 4", status = "InProgress", category = "Frontend" },
                        new { title = "Item 5", description = "Desc 5", status = "CarriedOver", category = "QA" },
                        new { title = "Item 6", description = "Desc 6", status = "CarriedOver", category = "Backend" },
                        new { title = "Item 7", description = "Desc 7", status = "Shipped", category = "Frontend" },
                        new { title = "Item 8", description = "Desc 8", status = "CarriedOver", category = "Documentation" }
                    },
                    metrics = new { completionPercentage = 72, healthStatus = "OnTrack", velocityCount = 12 }
                };

                string jsonPath = Path.Combine(tempDir, "data.json");
                string jsonContent = JsonSerializer.Serialize(jsonData);
                await File.WriteAllTextAsync(jsonPath, jsonContent);

                var originalBaseDirectory = AppContext.BaseDirectory;
                var provider = new TestDataProvider(mockLogger.Object, tempDir);

                var project = await provider.LoadProjectDataAsync();

                Assert.NotNull(project);
                Assert.Equal("Test Project", project.Name);
                Assert.Equal(4, project.Milestones.Count);
                Assert.Equal(8, project.WorkItems.Count);
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
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var provider = new TestDataProvider(mockLogger.Object, tempDir);

            await Assert.ThrowsAsync<FileNotFoundException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidJson_ThrowsInvalidOperationException()
        {
            var mockLogger = new Mock<ILogger<DataProvider>>();
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                string jsonPath = Path.Combine(tempDir, "data.json");
                await File.WriteAllTextAsync(jsonPath, "{ invalid json }");

                var provider = new TestDataProvider(mockLogger.Object, tempDir);

                await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        private class TestDataProvider : DataProvider
        {
            private readonly string _testDir;

            public TestDataProvider(ILogger<DataProvider> logger, string testDir)
                : base(logger)
            {
                _testDir = testDir;
            }

            protected override string GetDataFilePath() => Path.Combine(_testDir, "data.json");
        }
    }
}