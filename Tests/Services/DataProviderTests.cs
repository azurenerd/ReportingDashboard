using System;
using System.Collections.Generic;
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

namespace AgentSquad.Runner.Tests.Services
{
    public class DataProviderTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly Mock<ILogger<DataProvider>> _mockLogger;
        private readonly IDataCache _dataCache;
        private readonly Mock<IWebHostEnvironment> _mockHostEnvironment;

        public DataProviderTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            _mockLogger = new Mock<ILogger<DataProvider>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            _dataCache = new MemoryCacheDataProvider(memoryCache);
            _mockHostEnvironment = new Mock<IWebHostEnvironment>();
            _mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        private async Task<string> CreateValidDataJson()
        {
            var jsonData = new
            {
                name = "Executive Dashboard",
                description = "Project dashboard",
                startDate = "2024-01-15",
                milestones = new[]
                {
                    new { name = "Alpha", targetDate = "2024-02-15", status = "Completed", description = "Phase 1" },
                    new { name = "Beta", targetDate = "2024-03-15", status = "InProgress", description = "Phase 2" },
                    new { name = "Release", targetDate = "2024-04-15", status = "Future", description = "Launch" }
                },
                workItems = new[]
                {
                    new { title = "Auth module", description = "JWT auth", status = "Shipped", assignedTo = "Alice" },
                    new { title = "Dashboard UI", description = "UI components", status = "InProgress", assignedTo = "Bob" },
                    new { title = "Reporting", description = "Export reports", status = "CarriedOver", assignedTo = null }
                },
                metrics = new { completionPercentage = 72, healthStatus = "OnTrack", velocityCount = 12 }
            };

            string jsonPath = Path.Combine(_tempDir, "data.json");
            string jsonContent = JsonSerializer.Serialize(jsonData);
            await File.WriteAllTextAsync(jsonPath, jsonContent);
            return jsonPath;
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidJsonFile_DeserializesSuccessfully()
        {
            await CreateValidDataJson();
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);

            var project = await provider.LoadProjectDataAsync();

            Assert.NotNull(project);
            Assert.Equal("Executive Dashboard", project.Name);
            Assert.Equal("Project dashboard", project.Description);
            Assert.Equal(3, project.Milestones.Count);
            Assert.Equal(3, project.WorkItems.Count);
            Assert.NotNull(project.Metrics);
            Assert.Equal(72, project.Metrics.CompletionPercentage);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsFileNotFoundException()
        {
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);

            await Assert.ThrowsAsync<FileNotFoundException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidJson_ThrowsInvalidOperationException()
        {
            string jsonPath = Path.Combine(_tempDir, "data.json");
            await File.WriteAllTextAsync(jsonPath, "{ invalid json }");
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNullJson_ThrowsInvalidOperationException()
        {
            string jsonPath = Path.Combine(_tempDir, "data.json");
            await File.WriteAllTextAsync(jsonPath, "null");
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_CachesProjectData()
        {
            await CreateValidDataJson();
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);

            var project1 = await provider.LoadProjectDataAsync();
            var project2 = await provider.LoadProjectDataAsync();

            Assert.Same(project1, project2);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingProjectName_ThrowsInvalidOperationException()
        {
            var jsonData = new
            {
                description = "No name",
                startDate = "2024-01-15",
                milestones = new[] { new { name = "M1", targetDate = "2024-02-15", status = "Future", description = "M1" } },
                workItems = new object[] { },
                metrics = new { completionPercentage = 50, healthStatus = "OnTrack", velocityCount = 5 }
            };

            string jsonPath = Path.Combine(_tempDir, "data.json");
            await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(jsonData));
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNoMilestones_ThrowsInvalidOperationException()
        {
            var jsonData = new
            {
                name = "Test",
                description = "No milestones",
                startDate = "2024-01-15",
                milestones = new object[] { },
                workItems = new object[] { },
                metrics = new { completionPercentage = 50, healthStatus = "OnTrack", velocityCount = 5 }
            };

            string jsonPath = Path.Combine(_tempDir, "data.json");
            await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(jsonData));
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidCompletionPercentage_ThrowsInvalidOperationException()
        {
            var jsonData = new
            {
                name = "Test",
                description = "Invalid percentage",
                startDate = "2024-01-15",
                milestones = new[] { new { name = "M1", targetDate = "2024-02-15", status = "Future", description = "M1" } },
                workItems = new object[] { },
                metrics = new { completionPercentage = 150, healthStatus = "OnTrack", velocityCount = 5 }
            };

            string jsonPath = Path.Combine(_tempDir, "data.json");
            await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(jsonData));
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNegativeVelocity_ThrowsInvalidOperationException()
        {
            var jsonData = new
            {
                name = "Test",
                description = "Negative velocity",
                startDate = "2024-01-15",
                milestones = new[] { new { name = "M1", targetDate = "2024-02-15", status = "Future", description = "M1" } },
                workItems = new object[] { },
                metrics = new { completionPercentage = 50, healthStatus = "OnTrack", velocityCount = -5 }
            };

            string jsonPath = Path.Combine(_tempDir, "data.json");
            await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(jsonData));
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task GetCachedProjectData_BeforeLoading_ReturnsNull()
        {
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);

            var result = provider.GetCachedProjectData();

            Assert.Null(result);
        }

        [Fact]
        public async Task GetCachedProjectData_AfterLoading_ReturnsCachedProject()
        {
            await CreateValidDataJson();
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);
            await provider.LoadProjectDataAsync();

            var result = provider.GetCachedProjectData();

            Assert.NotNull(result);
            Assert.Equal("Executive Dashboard", result.Name);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithCaseInsensitiveJson_DeserializesSuccessfully()
        {
            var jsonContent = @"{
                ""NAME"": ""Dashboard"",
                ""DESCRIPTION"": ""Test"",
                ""STARTDATE"": ""2024-01-15"",
                ""MILESTONES"": [{ ""name"": ""M1"", ""targetDate"": ""2024-02-15"", ""status"": ""Future"", ""description"": ""M1"" }],
                ""WORKITEMS"": [],
                ""METRICS"": { ""completionPercentage"": 50, ""healthStatus"": ""OnTrack"", ""velocityCount"": 5 }
            }";

            string jsonPath = Path.Combine(_tempDir, "data.json");
            await File.WriteAllTextAsync(jsonPath, jsonContent);
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);

            var project = await provider.LoadProjectDataAsync();

            Assert.NotNull(project);
            Assert.Equal("Dashboard", project.Name);
        }

        [Theory]
        [InlineData("Completed")]
        [InlineData("InProgress")]
        [InlineData("AtRisk")]
        [InlineData("Future")]
        public async Task LoadProjectDataAsync_WithAllMilestoneStatuses_DeserializesSuccessfully(string status)
        {
            var jsonData = new
            {
                name = "Test",
                description = "Test",
                startDate = "2024-01-15",
                milestones = new[] { new { name = "M1", targetDate = "2024-02-15", status, description = "M1" } },
                workItems = new object[] { },
                metrics = new { completionPercentage = 50, healthStatus = "OnTrack", velocityCount = 5 }
            };

            string jsonPath = Path.Combine(_tempDir, "data.json");
            await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(jsonData));
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);

            var project = await provider.LoadProjectDataAsync();

            Assert.NotNull(project);
        }

        [Theory]
        [InlineData("Shipped")]
        [InlineData("InProgress")]
        [InlineData("CarriedOver")]
        public async Task LoadProjectDataAsync_WithAllWorkItemStatuses_DeserializesSuccessfully(string status)
        {
            var jsonData = new
            {
                name = "Test",
                description = "Test",
                startDate = "2024-01-15",
                milestones = new[] { new { name = "M1", targetDate = "2024-02-15", status = "Future", description = "M1" } },
                workItems = new[] { new { title = "WI1", description = "WI1", status, assignedTo = "User" } },
                metrics = new { completionPercentage = 50, healthStatus = "OnTrack", velocityCount = 5 }
            };

            string jsonPath = Path.Combine(_tempDir, "data.json");
            await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(jsonData));
            var provider = new DataProvider(_mockLogger.Object, _dataCache, _mockHostEnvironment.Object);

            var project = await provider.LoadProjectDataAsync();

            Assert.NotNull(project);
            Assert.Single(project.WorkItems);
        }
    }
}