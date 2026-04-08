using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AgentSquad.Runner.Data;
using AgentSquad.Runner.Data.Exceptions;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Services
{
    /// <summary>
    /// Integration tests for ProjectDataService.
    /// Verifies end-to-end data loading, error handling, and caching behavior.
    /// Tests realistic scenarios with actual JSON files and external dependencies.
    /// </summary>
    public class ProjectDataServiceIntegrationTests
    {
        private readonly Mock<ILogger<ProjectDataService>> _mockLogger;

        public ProjectDataServiceIntegrationTests()
        {
            _mockLogger = new Mock<ILogger<ProjectDataService>>();
        }

        private string CreateValidProjectDataJson()
        {
            return @"{
                ""project"": {
                    ""name"": ""Q2 Mobile App Release"",
                    ""description"": ""Mobile app release for Q2"",
                    ""startDate"": ""2024-04-01"",
                    ""endDate"": ""2024-06-30"",
                    ""status"": ""OnTrack"",
                    ""sponsor"": ""John Doe"",
                    ""projectManager"": ""Jane Smith""
                },
                ""milestones"": [
                    {
                        ""id"": ""m1"",
                        ""name"": ""Design Phase Complete"",
                        ""targetDate"": ""2024-04-30"",
                        ""actualDate"": ""2024-04-28"",
                        ""status"": 0,
                        ""completionPercentage"": 100
                    },
                    {
                        ""id"": ""m2"",
                        ""name"": ""Development Phase"",
                        ""targetDate"": ""2024-05-31"",
                        ""status"": 1,
                        ""completionPercentage"": 65
                    }
                ],
                ""tasks"": [
                    {
                        ""id"": ""t1"",
                        ""name"": ""API Development"",
                        ""status"": 0,
                        ""assignedTo"": ""Alice"",
                        ""dueDate"": ""2024-05-15"",
                        ""estimatedDays"": 10,
                        ""relatedMilestone"": ""m2""
                    },
                    {
                        ""id"": ""t2"",
                        ""name"": ""UI Implementation"",
                        ""status"": 1,
                        ""assignedTo"": ""Bob"",
                        ""dueDate"": ""2024-05-20"",
                        ""estimatedDays"": 12,
                        ""relatedMilestone"": ""m2""
                    }
                ],
                ""metrics"": {
                    ""totalTasks"": 2,
                    ""completedTasks"": 1,
                    ""inProgressTasks"": 1,
                    ""carriedOverTasks"": 0,
                    ""completionPercentage"": 50,
                    ""estimatedBurndownRate"": 0.5,
                    ""projectStartDate"": ""2024-04-01"",
                    ""projectEndDate"": ""2024-06-30"",
                    ""daysRemaining"": 83
                }
            }";
        }

        #region Full Load Scenarios

        [Fact]
        public async Task LoadProjectDataAsync_WithCompleteValidJson_ReturnsFullProjectData()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var jsonContent = CreateValidProjectDataJson();
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, jsonContent);

                // Act
                var result = await service.LoadProjectDataAsync(tempFilePath);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Project);
                Assert.NotNull(result.Milestones);
                Assert.NotNull(result.Tasks);
                Assert.NotNull(result.Metrics);
                Assert.Equal("Q2 Mobile App Release", result.Project.Name);
                Assert.Equal(2, result.Milestones.Count);
                Assert.Equal(2, result.Tasks.Count);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_DeserializesAllProperties_Correctly()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var jsonContent = CreateValidProjectDataJson();
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, jsonContent);

                // Act
                var result = await service.LoadProjectDataAsync(tempFilePath);

                // Assert - Verify all properties deserialized
                Assert.Equal("Jane Smith", result.Project.ProjectManager);
                Assert.Equal("OnTrack", result.Project.Status);
                Assert.Equal(2, result.Metrics.TotalTasks);
                Assert.Equal(1, result.Metrics.CompletedTasks);
                Assert.Equal(50, result.Metrics.CompletionPercentage);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_IsCaseInsensitive_ForJsonProperties()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var jsonContent = @"{
                ""project"": { ""name"": ""Test"" },
                ""milestones"": [],
                ""tasks"": [],
                ""metrics"": {}
            }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, jsonContent);

                // Act
                var result = await service.LoadProjectDataAsync(tempFilePath);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Project);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        #endregion

        #region Caching Behavior

        [Fact]
        public async Task GetCachedData_ReturnsLoadedData_AfterLoad()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var jsonContent = CreateValidProjectDataJson();
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, jsonContent);
                await service.LoadProjectDataAsync(tempFilePath);

                // Act
                var cachedData = service.GetCachedData();

                // Assert
                Assert.NotNull(cachedData);
                Assert.Equal("Q2 Mobile App Release", cachedData.Project.Name);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public void GetCachedData_ReturnsNull_BeforeLoad()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);

            // Act
            var cachedData = service.GetCachedData();

            // Assert
            Assert.Null(cachedData);
        }

        [Fact]
        public async Task RefreshData_ClearsCache()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var jsonContent = CreateValidProjectDataJson();
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, jsonContent);
                await service.LoadProjectDataAsync(tempFilePath);
                Assert.NotNull(service.GetCachedData());

                // Act
                service.RefreshData();

                // Assert
                Assert.Null(service.GetCachedData());
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        #endregion

        #region Error Recovery

        [Fact]
        public async Task LoadProjectDataAsync_CanRetry_AfterFailure()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var invalidJson = "{ invalid }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                // First attempt with invalid JSON
                await File.WriteAllTextAsync(tempFilePath, invalidJson);
                await Assert.ThrowsAsync<DataLoadException>(() =>
                    service.LoadProjectDataAsync(tempFilePath));

                // Now write valid JSON
                var validJson = CreateValidProjectDataJson();
                await File.WriteAllTextAsync(tempFilePath, validJson);

                // Act - Retry should succeed
                var result = await service.LoadProjectDataAsync(tempFilePath);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Q2 Mobile App Release", result.Project.Name);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_MultipleSuccessfulLoads_OverwriteCache()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var tempFilePath1 = Path.GetTempFileName();
            var tempFilePath2 = Path.GetTempFileName();

            try
            {
                var json1 = @"{
                    ""project"": { ""name"": ""Project One"" },
                    ""milestones"": [],
                    ""tasks"": [],
                    ""metrics"": {}
                }";

                var json2 = @"{
                    ""project"": { ""name"": ""Project Two"" },
                    ""milestones"": [],
                    ""tasks"": [],
                    ""metrics"": {}
                }";

                await File.WriteAllTextAsync(tempFilePath1, json1);
                await File.WriteAllTextAsync(tempFilePath2, json2);

                // Act
                await service.LoadProjectDataAsync(tempFilePath1);
                var cachedAfterFirst = service.GetCachedData();

                await service.LoadProjectDataAsync(tempFilePath2);
                var cachedAfterSecond = service.GetCachedData();

                // Assert
                Assert.Equal("Project One", cachedAfterFirst.Project.Name);
                Assert.Equal("Project Two", cachedAfterSecond.Project.Name);
            }
            finally
            {
                File.Delete(tempFilePath1);
                File.Delete(tempFilePath2);
            }
        }

        #endregion

        #region Validation Integration

        [Fact]
        public void ValidateJsonSchema_WithValidProjectData_ReturnsTrue()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var jsonContent = CreateValidProjectDataJson();

            // Act
            var isValid = service.ValidateJsonSchema(jsonContent);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateJsonSchema_ReturnsFalseBeforeLoad_ForInvalidData()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var invalidJson = "{ missing properties }";

            // Act
            var isValid = service.ValidateJsonSchema(invalidJson);

            // Assert
            Assert.False(isValid);
        }

        #endregion

        #region Performance & Boundary Tests

        [Fact]
        public async Task LoadProjectDataAsync_WithLargeJson_Succeeds()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var milestonesJson = string.Join(",", new string[100].Select((_, i) =>
                $@"{{ ""id"": ""m{i}"", ""name"": ""Milestone {i}"", ""targetDate"": ""2024-05-01"", ""status"": 0, ""completionPercentage"": 0 }}"));

            var tasksJson = string.Join(",", new string[100].Select((_, i) =>
                $@"{{ ""id"": ""t{i}"", ""name"": ""Task {i}"", ""status"": 0, ""assignedTo"": ""User"", ""dueDate"": ""2024-05-01"", ""estimatedDays"": 5 }}"));

            var largeJson = @"{
                ""project"": { ""name"": ""Large Project"" },
                ""milestones"": [" + milestonesJson + @"],
                ""tasks"": [" + tasksJson + @"],
                ""metrics"": {}
            }";

            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, largeJson);

                // Act
                var result = await service.LoadProjectDataAsync(tempFilePath);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(100, result.Milestones.Count);
                Assert.Equal(100, result.Tasks.Count);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMinimalValidJson_Succeeds()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var minimalJson = @"{
                ""project"": {},
                ""milestones"": [],
                ""tasks"": [],
                ""metrics"": {}
            }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, minimalJson);

                // Act
                var result = await service.LoadProjectDataAsync(tempFilePath);

                // Assert
                Assert.NotNull(result);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public async Task Dashboard_CanLoadAndDisplayProjectData_AfterLoad()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var jsonContent = CreateValidProjectDataJson();
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, jsonContent);

                // Act
                var projectData = await service.LoadProjectDataAsync(tempFilePath);

                // Assert - Verify Dashboard can access all required data
                Assert.NotNull(projectData.Project);
                Assert.NotNull(projectData.Milestones);
                Assert.NotNull(projectData.Tasks);
                Assert.NotNull(projectData.Metrics);

                // Verify data is complete for rendering
                Assert.NotEmpty(projectData.Project.Name);
                Assert.NotEmpty(projectData.Milestones);
                Assert.NotEmpty(projectData.Tasks);
                Assert.NotNull(projectData.Metrics.CompletionPercentage);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task Dashboard_ReceivesUserFriendlyError_OnDataLoadException()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "missing", "data.json");

            // Act
            var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                service.LoadProjectDataAsync(nonExistentPath));

            // Assert - Verify error is user-friendly (suitable for ErrorBoundary display)
            Assert.Equal("data.json not found in wwwroot directory", exception.Message);
            Assert.NotEmpty(exception.Message);
            Assert.DoesNotContain("Exception", exception.Message);
            Assert.DoesNotContain(".cs:", exception.Message);
        }

        #endregion
    }
}