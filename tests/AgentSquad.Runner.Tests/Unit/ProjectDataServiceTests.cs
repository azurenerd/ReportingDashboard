using Xunit;
using Moq;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class ProjectDataServiceTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<ILogger<ProjectDataService>> _mockLogger;

        public ProjectDataServiceTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDir);

            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.ContentRootPath).Returns(_tempDir);

            _mockLogger = new Mock<ILogger<ProjectDataService>>();
        }

        private void CreateDataFile(string content)
        {
            var dataDir = Path.Combine(_tempDir, "data");
            Directory.CreateDirectory(dataDir);
            File.WriteAllText(Path.Combine(dataDir, "data.json"), content);
        }

        [Fact]
        public async Task InitializeAsync_WithMissingDataFile_ThrowsFileNotFoundException()
        {
            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            
            var ex = await Assert.ThrowsAsync<FileNotFoundException>(() => service.InitializeAsync());
            
            Assert.NotNull(service.LastError);
            Assert.Contains("not found", service.LastError);
        }

        [Fact]
        public async Task InitializeAsync_WithValidJson_LoadsSuccessfully()
        {
            var json = """
            {
              "projectName": "Test",
              "milestones": [],
              "shipped": [],
              "inProgress": [],
              "carriedOver": []
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            await service.InitializeAsync();

            Assert.True(service.IsInitialized);
            Assert.Null(service.LastError);
        }

        [Fact]
        public async Task InitializeAsync_WithInvalidJson_ThrowsJsonException()
        {
            CreateDataFile("{ invalid json }");

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            
            var ex = await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => service.InitializeAsync());
            
            Assert.NotNull(service.LastError);
            Assert.Contains("JSON", service.LastError);
        }

        [Fact]
        public async Task InitializeAsync_WithValidData_CachesDashboard()
        {
            var json = """
            {
              "projectName": "Test",
              "milestones": [],
              "shipped": [{"id": "w1", "title": "Item 1"}],
              "inProgress": [],
              "carriedOver": []
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            await service.InitializeAsync();

            var dashboard1 = service.GetDashboard();
            var dashboard2 = service.GetDashboard();

            Assert.Same(dashboard1, dashboard2);
        }

        [Fact]
        public async Task GetDashboard_BeforeInitialize_ThrowsInvalidOperationException()
        {
            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            
            var ex = Assert.Throws<InvalidOperationException>(() => service.GetDashboard());
            
            Assert.Contains("not initialized", ex.Message.ToLower());
        }

        [Fact]
        public async Task InitializeAsync_WithValidData_CalculatesMetrics()
        {
            var json = """
            {
              "projectName": "Test",
              "milestones": [],
              "shipped": [{"id": "w1", "title": "Item 1"}, {"id": "w2", "title": "Item 2"}],
              "inProgress": [{"id": "w3", "title": "Item 3"}],
              "carriedOver": [{"id": "w4", "title": "Item 4"}]
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            await service.InitializeAsync();

            var dashboard = service.GetDashboard();

            Assert.Equal(4, dashboard.Metrics.TotalPlanned);
            Assert.Equal(2, dashboard.Metrics.Completed);
            Assert.Equal(2, dashboard.Metrics.InFlight);
            Assert.Equal(50m, dashboard.Metrics.HealthScore);
        }

        [Fact]
        public async Task InitializeAsync_With_45Items_CalculatesCorrectMetrics()
        {
            var shipped = string.Join(",", Enumerable.Range(1, 15)
                .Select(i => $@"{{"id":"w{i:D3}","title":"Item {i}""}}"));
            var inProgress = string.Join(",", Enumerable.Range(16, 15)
                .Select(i => $@"{{"id":"w{i:D3}","title":"Item {i}""}}"));
            var carriedOver = string.Join(",", Enumerable.Range(31, 15)
                .Select(i => $@"{{"id":"w{i:D3}","title":"Item {i}""}}"));

            var json = $"""
            {{
              "projectName": "Test",
              "milestones": [],
              "shipped": [{shipped}],
              "inProgress": [{inProgress}],
              "carriedOver": [{carriedOver}]
            }}
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            await service.InitializeAsync();

            var dashboard = service.GetDashboard();

            Assert.Equal(45, dashboard.Metrics.TotalPlanned);
            Assert.Equal(15, dashboard.Metrics.Completed);
            Assert.Equal(30, dashboard.Metrics.InFlight);
            Assert.Equal(33.333333m, dashboard.Metrics.HealthScore, 5);
        }

        [Fact]
        public async Task IsInitialized_ReturnsFalse_BeforeInitializeAsync()
        {
            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            
            Assert.False(service.IsInitialized);
        }

        [Fact]
        public async Task IsInitialized_ReturnsTrue_AfterSuccessfulInitialize()
        {
            var json = """
            {
              "projectName": "Test",
              "milestones": [],
              "shipped": [],
              "inProgress": [],
              "carriedOver": []
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            await service.InitializeAsync();

            Assert.True(service.IsInitialized);
        }

        [Fact]
        public async Task RefreshAsync_ReloadsData()
        {
            var json1 = """
            {
              "projectName": "Version1",
              "milestones": [],
              "shipped": [{"id": "w1", "title": "Item 1"}],
              "inProgress": [],
              "carriedOver": []
            }
            """;
            CreateDataFile(json1);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            await service.InitializeAsync();

            var dashboard1 = service.GetDashboard();
            Assert.Equal("Version1", dashboard1.ProjectName);
            Assert.Single(dashboard1.Shipped);

            var json2 = """
            {
              "projectName": "Version2",
              "milestones": [],
              "shipped": [{"id": "w1", "title": "Item 1"}, {"id": "w2", "title": "Item 2"}],
              "inProgress": [],
              "carriedOver": []
            }
            """;
            File.WriteAllText(Path.Combine(_tempDir, "data", "data.json"), json2);

            await service.RefreshAsync();

            var dashboard2 = service.GetDashboard();
            Assert.Equal("Version2", dashboard2.ProjectName);
            Assert.Equal(2, dashboard2.Shipped.Count);
        }

        [Fact]
        public async Task InitializeAsync_WithCaseInsensitiveProperties_DeserializesCorrectly()
        {
            var json = """
            {
              "PROJECTNAME": "TestProject",
              "Milestones": [],
              "Shipped": [],
              "INPROGRESS": [],
              "carriedover": []
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            await service.InitializeAsync();

            var dashboard = service.GetDashboard();
            Assert.Equal("TestProject", dashboard.ProjectName);
        }

        [Fact]
        public async Task InitializeAsync_WithExtraJsonFields_IgnoresSilently()
        {
            var json = """
            {
              "projectName": "Test",
              "milestones": [],
              "shipped": [],
              "inProgress": [],
              "carriedOver": [],
              "unknownField": "value",
              "futureFeature": {"nested": "object"}
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            await service.InitializeAsync();

            Assert.True(service.IsInitialized);
            Assert.Null(service.LastError);
        }

        [Fact]
        public async Task InitializeAsync_WithZeroItems_CalculatesHealthScoreAsZero()
        {
            var json = """
            {
              "projectName": "Test",
              "milestones": [],
              "shipped": [],
              "inProgress": [],
              "carriedOver": []
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            await service.InitializeAsync();

            var dashboard = service.GetDashboard();
            Assert.Equal(0, dashboard.Metrics.HealthScore);
        }

        [Fact]
        public async Task InitializeAsync_WithAllItemsShipped_CalculatesHealthScoreAs100()
        {
            var json = """
            {
              "projectName": "Test",
              "milestones": [],
              "shipped": [{"id": "w1", "title": "Item 1"}, {"id": "w2", "title": "Item 2"}],
              "inProgress": [],
              "carriedOver": []
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            await service.InitializeAsync();

            var dashboard = service.GetDashboard();
            Assert.Equal(100m, dashboard.Metrics.HealthScore);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
    }
}