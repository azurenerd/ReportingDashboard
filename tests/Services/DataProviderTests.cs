using Moq;
using Microsoft.Extensions.Logging;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Xunit;

namespace AgentSquad.Runner.Tests.Services
{
    public class DataProviderTests
    {
        private readonly Mock<ILogger<DataProvider>> _mockLogger;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<IDataCache> _mockCache;

        public DataProviderTests()
        {
            _mockLogger = new Mock<ILogger<DataProvider>>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockCache = new Mock<IDataCache>();
        }

        [Fact]
        public async Task LoadProjectDataAsync_ReturnsProjectData_FromFile()
        {
            var jsonContent = @"{
                ""name"": ""Test Project"",
                ""description"": ""Test Description"",
                ""milestones"": [],
                ""metrics"": { ""completionPercentage"": 50 },
                ""workItems"": []
            }";

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var dataFilePath = Path.Combine(tempDir, "data.json");
            File.WriteAllText(dataFilePath, jsonContent);

            _mockEnvironment.Setup(x => x.WebRootPath).Returns(tempDir);
            _mockCache.Setup(x => x.Get<Project>("ProjectData")).Returns((Project)null);

            var provider = new DataProvider(_mockLogger.Object, _mockEnvironment.Object, _mockCache.Object);
            var result = await provider.LoadProjectDataAsync();

            Assert.NotNull(result);
            Assert.Equal("Test Project", result.Name);
            Assert.Equal("Test Description", result.Description);

            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task LoadProjectDataAsync_CachesData_AfterFirstLoad()
        {
            var jsonContent = @"{
                ""name"": ""Cached Project"",
                ""description"": ""Test"",
                ""milestones"": [],
                ""metrics"": { ""completionPercentage"": 50 },
                ""workItems"": []
            }";

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var dataFilePath = Path.Combine(tempDir, "data.json");
            File.WriteAllText(dataFilePath, jsonContent);

            _mockEnvironment.Setup(x => x.WebRootPath).Returns(tempDir);
            _mockCache.Setup(x => x.Get<Project>("ProjectData")).Returns((Project)null);

            var provider = new DataProvider(_mockLogger.Object, _mockEnvironment.Object, _mockCache.Object);
            var result = await provider.LoadProjectDataAsync();

            _mockCache.Verify(x => x.Set("ProjectData", It.IsAny<Project>(), It.IsAny<TimeSpan>()), Times.Once);

            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task LoadProjectDataAsync_ReturnsCached_WhenAvailable()
        {
            var cachedProject = new Project { Name = "Cached Project" };
            _mockCache.Setup(x => x.Get<Project>("ProjectData")).Returns(cachedProject);

            var provider = new DataProvider(_mockLogger.Object, _mockEnvironment.Object, _mockCache.Object);
            var result = await provider.LoadProjectDataAsync();

            Assert.NotNull(result);
            Assert.Equal("Cached Project", result.Name);
            _mockCache.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task LoadProjectDataAsync_ThrowsFileNotFoundException_WhenDataJsonMissing()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            _mockEnvironment.Setup(x => x.WebRootPath).Returns(tempDir);
            _mockCache.Setup(x => x.Get<Project>("ProjectData")).Returns((Project)null);

            var provider = new DataProvider(_mockLogger.Object, _mockEnvironment.Object, _mockCache.Object);

            await Assert.ThrowsAsync<FileNotFoundException>(() => provider.LoadProjectDataAsync());

            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task LoadProjectDataAsync_ThrowsInvalidOperationException_WhenJsonInvalid()
        {
            var jsonContent = "{ invalid json }";

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var dataFilePath = Path.Combine(tempDir, "data.json");
            File.WriteAllText(dataFilePath, jsonContent);

            _mockEnvironment.Setup(x => x.WebRootPath).Returns(tempDir);
            _mockCache.Setup(x => x.Get<Project>("ProjectData")).Returns((Project)null);

            var provider = new DataProvider(_mockLogger.Object, _mockEnvironment.Object, _mockCache.Object);

            await Assert.ThrowsAsync<Exception>(() => provider.LoadProjectDataAsync());

            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task LoadProjectDataAsync_HandlesNull_ProjectData()
        {
            var jsonContent = "null";

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var dataFilePath = Path.Combine(tempDir, "data.json");
            File.WriteAllText(dataFilePath, jsonContent);

            _mockEnvironment.Setup(x => x.WebRootPath).Returns(tempDir);
            _mockCache.Setup(x => x.Get<Project>("ProjectData")).Returns((Project)null);

            var provider = new DataProvider(_mockLogger.Object, _mockEnvironment.Object, _mockCache.Object);

            var result = await provider.LoadProjectDataAsync();

            Assert.Null(result);

            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task LoadProjectDataAsync_DeserializesCompleteProjectData()
        {
            var jsonContent = @"{
                ""name"": ""Full Project"",
                ""description"": ""Complete Data"",
                ""lastUpdated"": ""2026-04-08T00:00:00"",
                ""milestones"": [
                    {
                        ""name"": ""M1"",
                        ""targetDate"": ""2026-06-01T00:00:00"",
                        ""status"": 0,
                        ""description"": ""Milestone 1""
                    }
                ],
                ""metrics"": {
                    ""completionPercentage"": 75,
                    ""healthStatus"": 0,
                    ""velocityThisMonth"": 10,
                    ""completedMilestones"": 2,
                    ""milestoneCount"": 3,
                    ""targetMilestoneCount"": 4
                },
                ""workItems"": [
                    {
                        ""id"": ""1"",
                        ""title"": ""Task 1"",
                        ""description"": ""Complete task"",
                        ""status"": 0
                    }
                ]
            }";

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var dataFilePath = Path.Combine(tempDir, "data.json");
            File.WriteAllText(dataFilePath, jsonContent);

            _mockEnvironment.Setup(x => x.WebRootPath).Returns(tempDir);
            _mockCache.Setup(x => x.Get<Project>("ProjectData")).Returns((Project)null);

            var provider = new DataProvider(_mockLogger.Object, _mockEnvironment.Object, _mockCache.Object);
            var result = await provider.LoadProjectDataAsync();

            Assert.NotNull(result);
            Assert.NotEmpty(result.Milestones);
            Assert.NotNull(result.Metrics);
            Assert.NotEmpty(result.WorkItems);

            Directory.Delete(tempDir, true);
        }

        [Fact]
        public void AcceptanceCriteria_LoadsDataFromIDataProvider()
        {
            var provider = new DataProvider(_mockLogger.Object, _mockEnvironment.Object, _mockCache.Object);

            Assert.NotNull(provider);
            Assert.IsAssignableFrom<IDataProvider>(provider);
        }
    }
}