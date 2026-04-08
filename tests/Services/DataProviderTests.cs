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

        private Project CreateValidTestProject()
        {
            return new Project
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
                    },
                    new Milestone
                    {
                        Name = "Phase 2",
                        TargetDate = DateTime.Parse("2024-06-30"),
                        Status = MilestoneStatus.InProgress,
                        Description = "Phase 2 in progress"
                    },
                    new Milestone
                    {
                        Name = "Phase 3",
                        TargetDate = DateTime.Parse("2024-09-30"),
                        Status = MilestoneStatus.Future,
                        Description = "Phase 3 upcoming"
                    }
                },
                WorkItems = new List<WorkItem>
                {
                    new WorkItem { Title = "Task 1", Description = "First task", Status = WorkItemStatus.Shipped, AssignedTo = "Dev Team" },
                    new WorkItem { Title = "Task 2", Description = "Second task", Status = WorkItemStatus.InProgress, AssignedTo = "Dev Team" },
                    new WorkItem { Title = "Task 3", Description = "Third task", Status = WorkItemStatus.CarriedOver, AssignedTo = "Dev Team" }
                }
            };
        }

        #region Acceptance Criteria Tests

        [Fact]
        public async Task LoadProjectDataAsync_WithValidJsonFile_LoadsWithoutErrors()
        {
            // Arrange - AC: DataProvider loads wwwroot/data.json without errors when file exists and contains valid JSON
            var testProject = CreateValidTestProject();
            WriteTestDataJson(testProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            var result = await provider.LoadProjectDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Project", result.Name);
        }

        [Fact]
        public async Task LoadProjectDataAsync_DeserializesJsonToStronglyTypedProject()
        {
            // Arrange - AC: DataProvider deserializes JSON into strongly-typed Project model with all required properties
            var testProject = CreateValidTestProject();
            WriteTestDataJson(testProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            var result = await provider.LoadProjectDataAsync();

            // Assert - Verify all required properties are deserialized
            Assert.NotNull(result);
            Assert.NotNull(result.Name);
            Assert.NotNull(result.Description);
            Assert.NotNull(result.Milestones);
            Assert.NotNull(result.WorkItems);
            Assert.True(result.CompletionPercentage >= 0);
            Assert.True(Enum.IsDefined(typeof(HealthStatus), result.HealthStatus));
            Assert.True(result.VelocityThisMonth >= 0);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingRequiredFields_ThrowsInvalidOperationException()
        {
            // Arrange - AC: DataProvider validates JSON structure and throws descriptive InvalidOperationException when required fields missing
            var invalidProject = new
            {
                description = "Missing name field",
                startDate = "2024-01-01",
                targetEndDate = "2024-12-31",
                completionPercentage = 50,
                healthStatus = "OnTrack",
                velocityThisMonth = 10,
                milestones = new[] { new { name = "Milestone", targetDate = "2024-03-31", status = "Completed", description = "" } },
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
        public async Task LoadProjectDataAsync_CachesDataInMemoryForOneHour()
        {
            // Arrange - AC: DataProvider caches parsed Project data in-memory for 1 hour via IDataCache service
            var testProject = CreateValidTestProject();
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

        [Fact]
        public async Task LoadProjectDataAsync_ReturnsCachedDataOnSubsequentCalls()
        {
            // Arrange - AC: DataProvider returns cached data on subsequent calls within TTL window
            var cachedProject = new Project
            {
                Name = "Cached Project",
                Milestones = new List<Milestone>
                {
                    new Milestone { Name = "M1", Status = MilestoneStatus.Completed, TargetDate = DateTime.Now }
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
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsFileNotFoundExceptionWithHelpfulMessage()
        {
            // Arrange - AC: DataProvider throws FileNotFoundException with helpful message when data.json not found
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("not found", exception.Message);
            Assert.Contains("data.json", exception.Message);
            Assert.Contains("wwwroot", exception.Message);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidJsonSyntax_ThrowsJsonExceptionWithLineColumnInfo()
        {
            // Arrange - AC: DataProvider throws JsonException with line/column info when JSON syntax invalid
            File.WriteAllText(_testDataFilePath, "{ invalid json: content");
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<JsonException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("Invalid JSON", exception.Message);
        }

        [Fact]
        public void InvalidateCache_RemovesCachedData()
        {
            // Arrange
            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            provider.InvalidateCache();

            // Assert
            _mockCache.Verify(c => c.Remove(It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region Happy Path Tests

        [Fact]
        public async Task LoadProjectDataAsync_WithCompleteValidProject_ReturnsFullyPopulatedModel()
        {
            // Arrange
            var testProject = CreateValidTestProject();
            WriteTestDataJson(testProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            var result = await provider.LoadProjectDataAsync();

            // Assert
            Assert.Equal("Test Project", result.Name);
            Assert.Equal("Test Description", result.Description);
            Assert.Equal(50, result.CompletionPercentage);
            Assert.Equal(HealthStatus.OnTrack, result.HealthStatus);
            Assert.Equal(10, result.VelocityThisMonth);
            Assert.Equal(3, result.Milestones.Count);
            Assert.Equal(3, result.WorkItems.Count);
            Assert.Equal("Phase 1", result.Milestones[0].Name);
            Assert.Equal(MilestoneStatus.Completed, result.Milestones[0].Status);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMultipleMilestoneStatuses_DeserializesAllStatusesCorrectly()
        {
            // Arrange
            var testProject = new Project
            {
                Name = "Multi-Status Project",
                Milestones = new List<Milestone>
                {
                    new Milestone { Name = "Completed", Status = MilestoneStatus.Completed, TargetDate = DateTime.Now },
                    new Milestone { Name = "InProgress", Status = MilestoneStatus.InProgress, TargetDate = DateTime.Now },
                    new Milestone { Name = "AtRisk", Status = MilestoneStatus.AtRisk, TargetDate = DateTime.Now },
                    new Milestone { Name = "Future", Status = MilestoneStatus.Future, TargetDate = DateTime.Now }
                },
                WorkItems = new List<WorkItem>()
            };

            WriteTestDataJson(testProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            var result = await provider.LoadProjectDataAsync();

            // Assert
            Assert.Collection(result.Milestones,
                m => Assert.Equal(MilestoneStatus.Completed, m.Status),
                m => Assert.Equal(MilestoneStatus.InProgress, m.Status),
                m => Assert.Equal(MilestoneStatus.AtRisk, m.Status),
                m => Assert.Equal(MilestoneStatus.Future, m.Status));
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMultipleWorkItemStatuses_DeserializesAllStatusesCorrectly()
        {
            // Arrange
            var testProject = new Project
            {
                Name = "Multi-Status Work Items",
                Milestones = new List<Milestone>
                {
                    new Milestone { Name = "M1", Status = MilestoneStatus.Completed, TargetDate = DateTime.Now }
                },
                WorkItems = new List<WorkItem>
                {
                    new WorkItem { Title = "Shipped", Status = WorkItemStatus.Shipped, Description = "", AssignedTo = "" },
                    new WorkItem { Title = "InProgress", Status = WorkItemStatus.InProgress, Description = "", AssignedTo = "" },
                    new WorkItem { Title = "CarriedOver", Status = WorkItemStatus.CarriedOver, Description = "", AssignedTo = "" }
                }
            };

            WriteTestDataJson(testProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            var result = await provider.LoadProjectDataAsync();

            // Assert
            Assert.Collection(result.WorkItems,
                w => Assert.Equal(WorkItemStatus.Shipped, w.Status),
                w => Assert.Equal(WorkItemStatus.InProgress, w.Status),
                w => Assert.Equal(WorkItemStatus.CarriedOver, w.Status));
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithVariousHealthStatuses_DeserializesAllStatusesCorrectly()
        {
            // Arrange
            var healthStatuses = new[] { HealthStatus.OnTrack, HealthStatus.AtRisk, HealthStatus.Blocked };

            foreach (var status in healthStatuses)
            {
                var testProject = new Project
                {
                    Name = "Health Status Test",
                    HealthStatus = status,
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
                var result = await provider.LoadProjectDataAsync();

                // Assert
                Assert.Equal(status, result.HealthStatus);
            }
        }

        #endregion

        #region Edge Cases & Validation Tests

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
        public async Task LoadProjectDataAsync_WithEmptyProjectName_ThrowsInvalidOperationException()
        {
            // Arrange
            var projectWithEmptyName = CreateValidTestProject();
            projectWithEmptyName.Name = "";

            WriteTestDataJson(projectWithEmptyName);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("name", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithWhitespaceOnlyProjectName_ThrowsInvalidOperationException()
        {
            // Arrange
            var projectWithWhitespaceName = CreateValidTestProject();
            projectWithWhitespaceName.Name = "   ";

            WriteTestDataJson(projectWithWhitespaceName);
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
            var projectWithoutMilestones = CreateValidTestProject();
            projectWithoutMilestones.Milestones = new List<Milestone>();

            WriteTestDataJson(projectWithoutMilestones);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("milestone", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNullMilestonesList_ThrowsInvalidOperationException()
        {
            // Arrange
            var projectWithNullMilestones = CreateValidTestProject();
            projectWithNullMilestones.Milestones = null;

            WriteTestDataJson(projectWithNullMilestones);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("milestone", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithCompletionPercentageNegative_ThrowsInvalidOperationException()
        {
            // Arrange
            var projectWithNegativeCompletion = CreateValidTestProject();
            projectWithNegativeCompletion.CompletionPercentage = -1;

            WriteTestDataJson(projectWithNegativeCompletion);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("Completion percentage", exception.Message);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithCompletionPercentageOver100_ThrowsInvalidOperationException()
        {
            // Arrange
            var projectWithExcessCompletion = CreateValidTestProject();
            projectWithExcessCompletion.CompletionPercentage = 101;

            WriteTestDataJson(projectWithExcessCompletion);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("Completion percentage", exception.Message);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithCompletionPercentageBoundaryValues_Succeeds()
        {
            // Arrange - Test both valid boundary values
            foreach (var percentage in new[] { 0, 100 })
            {
                var testProject = CreateValidTestProject();
                testProject.CompletionPercentage = percentage;

                WriteTestDataJson(testProject);
                _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

                var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

                // Act
                var result = await provider.LoadProjectDataAsync();

                // Assert
                Assert.Equal(percentage, result.CompletionPercentage);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMilestoneEmptyName_ThrowsInvalidOperationException()
        {
            // Arrange
            var projectWithEmptyMilestoneName = CreateValidTestProject();
            projectWithEmptyMilestoneName.Milestones[0].Name = "";

            WriteTestDataJson(projectWithEmptyMilestoneName);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("Milestone name", exception.Message);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNullMilestoneName_ThrowsInvalidOperationException()
        {
            // Arrange
            var projectWithNullMilestoneName = CreateValidTestProject();
            projectWithNullMilestoneName.Milestones[0].Name = null;

            WriteTestDataJson(projectWithNullMilestoneName);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            Assert.Contains("Milestone name", exception.Message);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidMilestoneStatus_ThrowsInvalidOperationException()
        {
            // Arrange - Create JSON with invalid milestone status string
            var projectJson = @"{
                ""name"": ""Test"",
                ""description"": ""Test"",
                ""startDate"": ""2024-01-01"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 50,
                ""healthStatus"": ""OnTrack"",
                ""velocityThisMonth"": 10,
                ""milestones"": [
                    {
                        ""name"": ""Invalid Status"",
                        ""targetDate"": ""2024-03-31"",
                        ""status"": ""InvalidStatus"",
                        ""description"": ""Test""
                    }
                ],
                ""workItems"": []
            }";

            File.WriteAllText(_testDataFilePath, projectJson);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<JsonException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNullWorkItemsList_InitializesEmptyList()
        {
            // Arrange
            var testProject = CreateValidTestProject();
            testProject.WorkItems = null;

            WriteTestDataJson(testProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            var result = await provider.LoadProjectDataAsync();

            // Assert
            Assert.NotNull(result.WorkItems);
            Assert.Empty(result.WorkItems);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithEmptyWorkItemsList_Succeeds()
        {
            // Arrange
            var testProject = CreateValidTestProject();
            testProject.WorkItems = new List<WorkItem>();

            WriteTestDataJson(testProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            var result = await provider.LoadProjectDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.WorkItems);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithLargeProjectData_LoadsSuccessfully()
        {
            // Arrange - Create large project with many milestones and work items
            var largeProject = new Project
            {
                Name = "Large Project",
                Description = "Project with many items",
                StartDate = DateTime.Parse("2024-01-01"),
                TargetEndDate = DateTime.Parse("2024-12-31"),
                CompletionPercentage = 75,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 50,
                Milestones = Enumerable.Range(1, 20)
                    .Select(i => new Milestone
                    {
                        Name = $"Milestone {i}",
                        TargetDate = DateTime.Parse("2024-01-01").AddMonths(i),
                        Status = MilestoneStatus.Future,
                        Description = $"Description {i}"
                    })
                    .ToList(),
                WorkItems = Enumerable.Range(1, 50)
                    .Select(i => new WorkItem
                    {
                        Title = $"Work Item {i}",
                        Description = $"Description {i}",
                        Status = (WorkItemStatus)(i % 3),
                        AssignedTo = $"Team Member {i % 5}"
                    })
                    .ToList()
            };

            WriteTestDataJson(largeProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            var result = await provider.LoadProjectDataAsync();

            // Assert
            Assert.Equal(20, result.Milestones.Count);
            Assert.Equal(50, result.WorkItems.Count);
        }

        #endregion

        #region Cache Behavior Tests

        [Fact]
        public async Task LoadProjectDataAsync_WithCacheHit_DoesNotReadFile()
        {
            // Arrange
            var cachedProject = new Project
            {
                Name = "Cached",
                Milestones = new List<Milestone>
                {
                    new Milestone { Name = "M1", Status = MilestoneStatus.Completed, TargetDate = DateTime.Now }
                },
                WorkItems = new List<WorkItem>()
            };

            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
                .ReturnsAsync(cachedProject);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            var result = await provider.LoadProjectDataAsync();

            // Assert
            Assert.Equal("Cached", result.Name);
            Assert.False(File.Exists(_testDataFilePath), "File should not be read when cache hit occurs");
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithCacheMiss_WritesToCache()
        {
            // Arrange
            var testProject = CreateValidTestProject();
            WriteTestDataJson(testProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act
            await provider.LoadProjectDataAsync();

            // Assert
            _mockCache.Verify(
                c => c.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan?>()),
                Times.Once);
        }

        #endregion

        #region Error Handling & Logging Tests

        [Fact]
        public async Task LoadProjectDataAsync_LogsErrorOnFileNotFound()
        {
            // Arrange
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => provider.LoadProjectDataAsync());

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task LoadProjectDataAsync_LogsErrorOnValidationFailure()
        {
            // Arrange
            var invalidProject = CreateValidTestProject();
            invalidProject.Name = "";

            WriteTestDataJson(invalidProject);
            _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>())).ReturnsAsync((Project)null);

            var provider = new DataProvider(_mockCache.Object, _mockLogger.Object, _mockEnvironment.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        #endregion

        #region Dependency Tests

        [Fact]
        public void Constructor_WithNullCache_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new DataProvider(null, _mockLogger.Object, _mockEnvironment.Object));
            Assert.Equal("cache", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new DataProvider(_mockCache.Object, null, _mockEnvironment.Object));
            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullEnvironment_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new DataProvider(_mockCache.Object, _mockLogger.Object, null));
            Assert.Equal("webHostEnvironment", exception.ParamName);
        }

        #endregion
    }
}