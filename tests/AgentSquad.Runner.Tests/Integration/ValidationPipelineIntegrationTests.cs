using Xunit;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Validators;

namespace AgentSquad.Runner.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class ValidationPipelineIntegrationTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<ILogger<ProjectDataService>> _mockLogger;

        public ValidationPipelineIntegrationTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"validation_test_{Guid.NewGuid()}");
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
        public async Task Service_ValidatesData_OnInitialization()
        {
            var json = """
            {
              "projectName": "Test",
              "milestones": [{"id":"m1","name":"M1","targetDate":"2026-02-01","status":"InvalidStatus"}],
              "shipped": [],
              "inProgress": [],
              "carriedOver": []
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            var ex = await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => service.InitializeAsync());

            Assert.NotNull(service.LastError);
            Assert.Contains("validation", service.LastError.ToLower());
        }

        [Fact]
        public async Task Service_DetectsDuplicateIds_Across_AllColumns()
        {
            var json = """
            {
              "projectName": "Test",
              "milestones": [],
              "shipped": [{"id":"w001","title":"Item 1"}],
              "inProgress": [{"id":"w001","title":"Item 1 Duplicate"}],
              "carriedOver": []
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            var ex = await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => service.InitializeAsync());

            Assert.NotNull(service.LastError);
            Assert.Contains("duplicate", service.LastError.ToLower());
            Assert.Contains("w001", service.LastError);
        }

        [Fact]
        public async Task Service_ValidatesMilestoneStatuses_OnAllMilestones()
        {
            var json = """
            {
              "projectName": "Test",
              "milestones": [
                {"id":"m1","name":"M1","targetDate":"2026-02-01","status":"Completed"},
                {"id":"m2","name":"M2","targetDate":"2026-03-01","status":"OnTrack"},
                {"id":"m3","name":"M3","targetDate":"2026-04-01","status":"BadStatus"}
              ],
              "shipped": [],
              "inProgress": [],
              "carriedOver": []
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            var ex = await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => service.InitializeAsync());

            Assert.NotNull(service.LastError);
            Assert.Contains("index 2", service.LastError);
        }

        [Fact]
        public async Task Service_AllowsExtraJsonFields_WithoutValidationError()
        {
            var json = """
            {
              "projectName": "Test",
              "milestones": [],
              "shipped": [],
              "inProgress": [],
              "carriedOver": [],
              "extraField1": "value",
              "futureFeature": {"nested": "object"},
              "unknownArray": [1, 2, 3]
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            await service.InitializeAsync();

            Assert.True(service.IsInitialized);
            Assert.Null(service.LastError);
        }

        [Fact]
        public async Task Service_Logs_ValidationErrors_ToConsole()
        {
            var json = """
            {
              "projectName": "Test",
              "milestones": [{"id":"m1","name":"M1","targetDate":"2026-02-01","status":"BadStatus"}],
              "shipped": [],
              "inProgress": [],
              "carriedOver": []
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            var ex = await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => service.InitializeAsync());

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task Service_Provides_UserFriendly_ErrorMessages()
        {
            var json = """
            {
              "projectName": "Test",
              "milestones": [{"id":"m1","name":"M1","targetDate":"2026-02-01","status":"InvalidStatus"}],
              "shipped": [],
              "inProgress": [],
              "carriedOver": []
            }
            """;
            CreateDataFile(json);

            var service = new ProjectDataService(_mockEnv.Object, _mockLogger.Object);
            var ex = await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => service.InitializeAsync());

            Assert.NotNull(service.LastError);
            Assert.DoesNotContain("at System", service.LastError);
            Assert.DoesNotContain("stack trace", service.LastError.ToLower());
        }

        [Fact]
        public async Task Service_Validates_45Items_Distribution_Successfully()
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

            Assert.True(service.IsInitialized);
            var dashboard = service.GetDashboard();
            Assert.Equal(45, dashboard.Metrics.TotalPlanned);
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