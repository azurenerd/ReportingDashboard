using Moq;
using Xunit;
using System.Text.Json;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Tests.Services
{
    public class DataProviderTests
    {
        private readonly Mock<IDataCache> _mockDataCache;
        private readonly Mock<ILogger<DataProvider>> _mockLogger;

        public DataProviderTests()
        {
            _mockDataCache = new Mock<IDataCache>();
            _mockLogger = new Mock<ILogger<DataProvider>>();
        }

        private Project CreateValidProject()
        {
            return new Project
            {
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.Now,
                TargetEndDate = DateTime.Now.AddMonths(6),
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 10,
                Milestones = new List<Milestone>
                {
                    new Milestone { Name = "M1", TargetDate = DateTime.Now, Status = MilestoneStatus.Completed }
                },
                WorkItems = new List<WorkItem>()
            };
        }

        [Fact]
        public async Task LoadProjectDataAsync_ReturnsProjectFromCache_WhenCached()
        {
            var cachedProject = CreateValidProject();
            _mockDataCache.Setup(x => x.GetAsync<Project>("project_data"))
                .ReturnsAsync(cachedProject);

            var provider = new DataProvider(_mockDataCache.Object, _mockLogger.Object);
            var result = await provider.LoadProjectDataAsync();

            Assert.NotNull(result);
            Assert.Equal("Test Project", result.Name);
            _mockDataCache.Verify(x => x.GetAsync<Project>("project_data"), Times.Once);
        }

        [Fact]
        public async Task LoadProjectDataAsync_ReadsFromFile_WhenNotCached()
        {
            _mockDataCache.Setup(x => x.GetAsync<Project>("project_data"))
                .ReturnsAsync((Project)null);

            var testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data.json");
            Directory.CreateDirectory(Path.GetDirectoryName(testDataPath));

            var projectJson = JsonSerializer.Serialize(CreateValidProject());
            File.WriteAllText(testDataPath, projectJson);

            try
            {
                var provider = new DataProvider(_mockDataCache.Object, _mockLogger.Object);
                var result = await provider.LoadProjectDataAsync();

                Assert.NotNull(result);
                _mockDataCache.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan?>()), Times.Once);
            }
            finally
            {
                if (File.Exists(testDataPath))
                    File.Delete(testDataPath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_ThrowsFileNotFoundException_WhenFileDoesNotExist()
        {
            _mockDataCache.Setup(x => x.GetAsync<Project>("project_data"))
                .ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockDataCache.Object, _mockLogger.Object);

            // Assume data.json doesn't exist at the test path
            await Assert.ThrowsAsync<FileNotFoundException>(async () => await provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_ThrowsJsonException_WhenJsonIsInvalid()
        {
            _mockDataCache.Setup(x => x.GetAsync<Project>("project_data"))
                .ReturnsAsync((Project)null);

            var testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data.json");
            Directory.CreateDirectory(Path.GetDirectoryName(testDataPath));
            File.WriteAllText(testDataPath, "{ invalid json }");

            try
            {
                var provider = new DataProvider(_mockDataCache.Object, _mockLogger.Object);
                await Assert.ThrowsAsync<JsonException>(async () => await provider.LoadProjectDataAsync());
            }
            finally
            {
                if (File.Exists(testDataPath))
                    File.Delete(testDataPath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_ThrowsInvalidOperationException_WhenProjectIsNull()
        {
            _mockDataCache.Setup(x => x.GetAsync<Project>("project_data"))
                .ReturnsAsync((Project)null);

            var testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data.json");
            Directory.CreateDirectory(Path.GetDirectoryName(testDataPath));
            File.WriteAllText(testDataPath, "null");

            try
            {
                var provider = new DataProvider(_mockDataCache.Object, _mockLogger.Object);
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.LoadProjectDataAsync());
            }
            finally
            {
                if (File.Exists(testDataPath))
                    File.Delete(testDataPath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_ThrowsInvalidOperationException_WhenNameIsEmpty()
        {
            _mockDataCache.Setup(x => x.GetAsync<Project>("project_data"))
                .ReturnsAsync((Project)null);

            var project = CreateValidProject();
            project.Name = string.Empty;

            var testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data.json");
            Directory.CreateDirectory(Path.GetDirectoryName(testDataPath));
            File.WriteAllText(testDataPath, JsonSerializer.Serialize(project));

            try
            {
                var provider = new DataProvider(_mockDataCache.Object, _mockLogger.Object);
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.LoadProjectDataAsync());
            }
            finally
            {
                if (File.Exists(testDataPath))
                    File.Delete(testDataPath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_ThrowsInvalidOperationException_WhenNoMilestones()
        {
            _mockDataCache.Setup(x => x.GetAsync<Project>("project_data"))
                .ReturnsAsync((Project)null);

            var project = CreateValidProject();
            project.Milestones = new List<Milestone>();

            var testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data.json");
            Directory.CreateDirectory(Path.GetDirectoryName(testDataPath));
            File.WriteAllText(testDataPath, JsonSerializer.Serialize(project));

            try
            {
                var provider = new DataProvider(_mockDataCache.Object, _mockLogger.Object);
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await provider.LoadProjectDataAsync());
            }
            finally
            {
                if (File.Exists(testDataPath))
                    File.Delete(testDataPath);
            }
        }

        [Fact]
        public void InvalidateCache_CallsRemoveOnCache()
        {
            var provider = new DataProvider(_mockDataCache.Object, _mockLogger.Object);
            provider.InvalidateCache();

            _mockDataCache.Verify(x => x.Remove("project_data"), Times.Once);
        }

        [Fact]
        public async Task LoadProjectDataAsync_CachesResultWithOneHourExpiration()
        {
            _mockDataCache.Setup(x => x.GetAsync<Project>("project_data"))
                .ReturnsAsync((Project)null);

            var testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data.json");
            Directory.CreateDirectory(Path.GetDirectoryName(testDataPath));

            var projectJson = JsonSerializer.Serialize(CreateValidProject());
            File.WriteAllText(testDataPath, projectJson);

            try
            {
                var provider = new DataProvider(_mockDataCache.Object, _mockLogger.Object);
                await provider.LoadProjectDataAsync();

                _mockDataCache.Verify(x => x.SetAsync(
                    "project_data",
                    It.IsAny<Project>(),
                    It.Is<TimeSpan?>(ts => ts.HasValue && ts.Value.TotalHours == 1)
                ), Times.Once);
            }
            finally
            {
                if (File.Exists(testDataPath))
                    File.Delete(testDataPath);
            }
        }
    }
}