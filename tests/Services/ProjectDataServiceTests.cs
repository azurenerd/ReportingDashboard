using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Data;

namespace AgentSquad.Runner.Tests.Services
{
    public class ProjectDataServiceTests
    {
        private readonly Mock<ILogger<ProjectDataService>> _loggerMock;
        private readonly ProjectDataService _service;

        public ProjectDataServiceTests()
        {
            _loggerMock = new Mock<ILogger<ProjectDataService>>();
            _service = new ProjectDataService(_loggerMock.Object);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidJson_ReturnsProjectData()
        {
            // Arrange
            var validJson = @"{
                ""project"": {
                    ""name"": ""Q2 Mobile App Release"",
                    ""description"": ""iOS and Android mobile app v2.0"",
                    ""startDate"": ""2024-01-01T00:00:00"",
                    ""endDate"": ""2024-06-30T00:00:00"",
                    ""status"": ""OnTrack"",
                    ""sponsor"": ""VP Engineering"",
                    ""projectManager"": ""John Doe""
                },
                ""milestones"": [],
                ""tasks"": [],
                ""metrics"": null
            }";
            var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
            await File.WriteAllTextAsync(tempFile, validJson);

            try
            {
                // Act
                var result = await _service.LoadProjectDataAsync(tempFile);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Project);
                Assert.Equal("Q2 Mobile App Release", result.Project.Name);
                Assert.Equal("iOS and Android mobile app v2.0", result.Project.Description);
                Assert.Empty(result.Milestones);
                Assert.Empty(result.Tasks);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMalformedJson_ThrowsDataLoadException()
        {
            // Arrange
            var malformedJson = @"{ invalid json }";
            var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
            await File.WriteAllTextAsync(tempFile, malformedJson);

            try
            {
                // Act & Assert
                await Assert.ThrowsAsync<DataLoadException>(() => _service.LoadProjectDataAsync(tempFile));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNonexistentFile_ThrowsDataLoadException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<DataLoadException>(() => 
                _service.LoadProjectDataAsync("/nonexistent/path/data.json"));
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithComplexProjectData_DeserializesAllFields()
        {
            // Arrange
            var complexJson = @"{
                ""project"": {
                    ""name"": ""Platform Upgrade"",
                    ""description"": ""Backend infrastructure v3.0"",
                    ""startDate"": ""2024-01-15T00:00:00"",
                    ""endDate"": ""2024-09-30T00:00:00"",
                    ""status"": ""AtRisk"",
                    ""sponsor"": ""CTO"",
                    ""projectManager"": ""Jane Smith""
                },
                ""milestones"": [
                    {
                        ""id"": ""m1"",
                        ""name"": ""Architecture Review"",
                        ""targetDate"": ""2024-02-28T00:00:00"",
                        ""actualDate"": ""2024-02-25T00:00:00"",
                        ""status"": 0,
                        ""completionPercentage"": 100
                    },
                    {
                        ""id"": ""m2"",
                        ""name"": ""Development Phase"",
                        ""targetDate"": ""2024-06-30T00:00:00"",
                        ""actualDate"": null,
                        ""status"": 1,
                        ""completionPercentage"": 45
                    }
                ],
                ""tasks"": [
                    {
                        ""id"": ""t1"",
                        ""name"": ""Database Migration"",
                        ""status"": 0,
                        ""assignedTo"": ""Dev Team A"",
                        ""dueDate"": ""2024-03-15T00:00:00"",
                        ""estimatedDays"": 10,
                        ""relatedMilestone"": ""m1""
                    }
                ],
                ""metrics"": {
                    ""totalTasks"": 15,
                    ""completedTasks"": 5,
                    ""inProgressTasks"": 8,
                    ""carriedOverTasks"": 2,
                    ""completionPercentage"": 33,
                    ""estimatedBurndownRate"": 0.5,
                    ""projectStartDate"": ""2024-01-15T00:00:00"",
                    ""projectEndDate"": ""2024-09-30T00:00:00"",
                    ""daysRemaining"": 200
                }
            }";
            var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
            await File.WriteAllTextAsync(tempFile, complexJson);

            try
            {
                // Act
                var result = await _service.LoadProjectDataAsync(tempFile);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Milestones.Count);
                Assert.Equal(1, result.Tasks.Count);
                Assert.NotNull(result.Metrics);
                Assert.Equal(15, result.Metrics.TotalTasks);
                Assert.Equal(5, result.Metrics.CompletedTasks);
                Assert.Equal(8, result.Metrics.InProgressTasks);
                Assert.Equal(2, result.Metrics.CarriedOverTasks);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ValidateJsonSchema_WithValidJson_ReturnsTrue()
        {
            // Arrange
            var validJson = @"{
                ""project"": null,
                ""milestones"": [],
                ""tasks"": [],
                ""metrics"": null
            }";

            // Act
            var result = _service.ValidateJsonSchema(validJson);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateJsonSchema_WithMalformedJson_ReturnsFalse()
        {
            // Arrange
            var malformedJson = @"{ broken: json ]";

            // Act
            var result = _service.ValidateJsonSchema(malformedJson);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateJsonSchema_WithEmptyString_ReturnsFalse()
        {
            // Act
            var result = _service.ValidateJsonSchema(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateJsonSchema_WithNullString_ReturnsFalse()
        {
            // Act
            var result = _service.ValidateJsonSchema(null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetCachedData_WhenNothingLoaded_ReturnsNull()
        {
            // Act
            var result = _service.GetCachedData();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoadProjectDataAsync_CachesDataForRetrieval()
        {
            // Arrange
            var json = @"{
                ""project"": {
                    ""name"": ""Test Project"",
                    ""description"": ""Test"",
                    ""startDate"": ""2024-01-01T00:00:00"",
                    ""endDate"": ""2024-12-31T00:00:00"",
                    ""status"": ""OnTrack"",
                    ""sponsor"": ""Sponsor"",
                    ""projectManager"": ""PM""
                },
                ""milestones"": [],
                ""tasks"": [],
                ""metrics"": null
            }";
            var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
            await File.WriteAllTextAsync(tempFile, json);

            try
            {
                // Act
                await _service.LoadProjectDataAsync(tempFile);
                var cached = _service.GetCachedData();

                // Assert
                Assert.NotNull(cached);
                Assert.Equal("Test Project", cached.Project?.Name);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNullProject_DeserializesSuccessfully()
        {
            // Arrange
            var json = @"{
                ""project"": null,
                ""milestones"": [],
                ""tasks"": [],
                ""metrics"": null
            }";
            var tempFile = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}.json");
            await File.WriteAllTextAsync(tempFile, json);

            try
            {
                // Act
                var result = await _service.LoadProjectDataAsync(tempFile);

                // Assert
                Assert.NotNull(result);
                Assert.Null(result.Project);
                Assert.Empty(result.Milestones);
                Assert.Empty(result.Tasks);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ValidateJsonSchema_WithCaseInsensitiveProperties_ReturnsTrue()
        {
            // Arrange - JSON with different casing
            var json = @"{
                ""Project"": null,
                ""MILESTONES"": [],
                ""Tasks"": [],
                ""Metrics"": null
            }";

            // Act
            var result = _service.ValidateJsonSchema(json);

            // Assert
            Assert.True(result);
        }
    }
}