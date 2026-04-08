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
                        new { name = "M1", targetDate = "2024-02-15", status = "Completed", description = "Milestone 1" }
                    },
                    workItems = new object[] { },
                    metrics = new { completionPercentage = 72, healthStatus = "OnTrack", velocityCount = 12, totalMilestones = 1, completedMilestones = 1 }
                };

                string jsonPath = Path.Combine(tempDir, "data.json");
                string jsonContent = JsonSerializer.Serialize(jsonData);
                await File.WriteAllTextAsync(jsonPath, jsonContent);

                var provider = new TestDataProvider(mockLogger.Object, tempDir);

                var project = await provider.LoadProjectDataAsync();

                Assert.NotNull(project);
                Assert.Equal("Test Project", project.Name);
                Assert.Single(project.Milestones);
                Assert.Empty(project.WorkItems);
                Assert.Equal(72, project.Metrics.CompletionPercentage);
                Assert.Equal(HealthStatus.OnTrack, project.Metrics.HealthStatus);
                Assert.Equal(1, project.Metrics.TotalMilestones);
                Assert.Equal(1, project.Metrics.CompletedMilestones);
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

        [Fact]
        public async Task LoadProjectDataAsync_WithZeroMilestones_ThrowsInvalidOperationException()
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
                    milestones = new object[] { },
                    workItems = new object[] { },
                    metrics = new { completionPercentage = 0, healthStatus = "OnTrack", velocityCount = 0, totalMilestones = 0, completedMilestones = 0 }
                };

                string jsonPath = Path.Combine(tempDir, "data.json");
                string jsonContent = JsonSerializer.Serialize(jsonData);
                await File.WriteAllTextAsync(jsonPath, jsonContent);

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