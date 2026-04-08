using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace AgentSquad.Runner.Tests.Services
{
    public class DataProviderTests : IDisposable
    {
        private readonly string _testDataDirectory;
        private readonly string _testDataFilePath;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<IDataCache> _mockCache;
        private readonly Mock<ILogger<DataProvider>> _mockLogger;

        public DataProviderTests()
        {
            _testDataDirectory = Path.Combine(Path.GetTempPath(), $"DataProviderTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDataDirectory);
            _testDataFilePath = Path.Combine(_testDataDirectory, "data.json");

            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testDataDirectory);

            _mockCache = new Mock<IDataCache>();
            _mockLogger = new Mock<ILogger<DataProvider>>();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDataDirectory))
            {
                Directory.Delete(_testDataDirectory, true);
            }
        }

        private void WriteTestDataJson(object data)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(_testDataFilePath, json);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidJson_DeserializesSuccessfully()
        {
            // Arrange
            var testProject = new Project
            {
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.Parse("2024-01-01"),
                TargetEndDate = DateTime.Parse("2024-12-31"),
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 10,
                Milestones = new List<Milestone>
                {
                    new Milestone
                    {
                        Name = "Phase 1",
                        TargetDate = DateTime.Parse("2024-03-31"),
                        Status = MilestoneStatus.Completed,
                        Description = "Phase 1 complete"
                    }
                },
                WorkItems = new List<WorkItem>()
            };

            WriteTestDataJson(testProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            var result = await provider.LoadProjectDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Project", result.Name);
            Assert.Equal(50, result.CompletionPercentage);
            Assert.Single(result.Milestones);
            _mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithCachedData_ReturnsCachedValueWithoutFileRead()
        {
            // Arrange
            var cachedProject = new Project
            {
                Name = "Cached Project",
                Milestones = new List<Milestone>
                {
                    new Milestone { Name = "Milestone 1", Status = MilestoneStatus.Completed, TargetDate = DateTime.Now }
                },
                WorkItems = new List<WorkItem>()
            };

            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
                .ReturnsAsync(cachedProject);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            var result = await provider.LoadProjectDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Cached Project", result.Name);
            _mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan?>()), Times.Never);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsFileNotFoundException()
        {
            // Arrange
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidJson_ThrowsJsonException()
        {
            // Arrange
            File.WriteAllText(_testDataFilePath, "{ invalid json content");
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<JsonException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("Invalid JSON", exception.Message);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNullProject_ThrowsInvalidOperationException()
        {
            // Arrange
            File.WriteAllText(_testDataFilePath, "null");
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("null", exception.Message);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingName_ThrowsInvalidOperationException()
        {
            // Arrange
            var invalidProject = new
            {
                description = "No name",
                startDate = "2024-01-01",
                targetEndDate = "2024-12-31",
                completionPercentage = 50,
                healthStatus = "OnTrack",
                velocityThisMonth = 10,
                milestones = new[] { new { name = "Phase 1", targetDate = "2024-03-31", status = "Completed", description = "Done" } },
                workItems = new object[] { }
            };

            WriteTestDataJson(invalidProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("name", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNoMilestones_ThrowsInvalidOperationException()
        {
            // Arrange
            var projectWithoutMilestones = new Project
            {
                Name = "No Milestones",
                Milestones = new List<Milestone>(),
                WorkItems = new List<WorkItem>()
            };

            WriteTestDataJson(projectWithoutMilestones);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("At least one milestone", exception.Message);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidCompletionPercentage_ThrowsInvalidOperationException()
        {
            // Arrange
            var invalidProject = new
            {
                name = "Test",
                completionPercentage = 150,
                healthStatus = "OnTrack",
                velocityThisMonth = 10,
                milestones = new[] { new { name = "Phase 1", targetDate = "2024-03-31", status = "Completed", description = "Done" } },
                workItems = new object[] { }
            };

            WriteTestDataJson(invalidProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("Completion percentage", exception.Message);
        }

        [Fact]
        public void InvalidateCache_CallsRemoveOnCache()
        {
            // Arrange
            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            provider.InvalidateCache();

            // Assert
            _mockCache.Verify(c => c.Remove(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LoadProjectDataAsync_CachesDataWithCorrectTTL()
        {
            // Arrange
            var testProject = new Project
            {
                Name = "Test",
                Milestones = new List<Milestone>
                {
                    new Milestone { Name = "M1", Status = MilestoneStatus.Completed, TargetDate = DateTime.Now }
                },
                WorkItems = new List<WorkItem>()
            };

            WriteTestDataJson(testProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            await provider.LoadProjectDataAsync();

            // Assert
            _mockCache.Verify(
                c => c.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Project>(),
                    It.Is<TimeSpan?>(ts => ts.HasValue && ts.Value.TotalHours == 1)),
                Times.Once);
        }
    }
}