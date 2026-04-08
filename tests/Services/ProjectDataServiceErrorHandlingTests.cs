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
    /// Unit tests for ProjectDataService error handling scenarios.
    /// Verifies that JSON parsing errors, file not found, and null validation errors
    /// are caught and converted to user-friendly DataLoadException messages.
    /// </summary>
    public class ProjectDataServiceErrorHandlingTests
    {
        private readonly Mock<ILogger<ProjectDataService>> _mockLogger;

        public ProjectDataServiceErrorHandlingTests()
        {
            _mockLogger = new Mock<ILogger<ProjectDataService>>();
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsDataLoadException_WithCorrectMessage()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var nonExistentFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "data.json");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                service.LoadProjectDataAsync(nonExistentFilePath));

            Assert.Equal("data.json not found in wwwroot directory", exception.Message);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMalformedJson_ThrowsDataLoadException_WithJsonExceptionDetails()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var malformedJson = "{ invalid json }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, malformedJson);

                // Act & Assert
                var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                    service.LoadProjectDataAsync(tempFilePath));

                Assert.StartsWith("Invalid JSON format:", exception.Message);
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNullDeserialization_ThrowsDataLoadException_WithNullMessage()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var emptyJson = "{}";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, emptyJson);

                // Act & Assert
                var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                    service.LoadProjectDataAsync(tempFilePath));

                Assert.Equal("JSON deserialization resulted in null", exception.Message);
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public void ValidateJsonSchema_WithMalformedJson_ReturnsFalse()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var malformedJson = "{ invalid json structure }";

            // Act
            var result = service.ValidateJsonSchema(malformedJson);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateJsonSchema_WithNullInput_ReturnsFalse()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);

            // Act
            var result = service.ValidateJsonSchema(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateJsonSchema_WithEmptyStringInput_ReturnsFalse()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);

            // Act
            var result = service.ValidateJsonSchema(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateJsonSchema_WithMissingRequiredProperties_ReturnsFalse()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var jsonMissingProject = @"{
                ""milestones"": [],
                ""tasks"": [],
                ""metrics"": {}
            }";

            // Act
            var result = service.ValidateJsonSchema(jsonMissingProject);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateJsonSchema_WithAllRequiredProperties_ReturnsTrue()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var validJson = @"{
                ""project"": { ""name"": ""Test"" },
                ""milestones"": [],
                ""tasks"": [],
                ""metrics"": {}
            }";

            // Act
            var result = service.ValidateJsonSchema(validJson);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ErrorMessages_AreUserFriendly_NoStackTraces()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var malformedJson = "{ invalid }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, malformedJson);

                // Act
                var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                    service.LoadProjectDataAsync(tempFilePath));

                // Assert - Verify no technical details exposed
                Assert.DoesNotContain("at ", exception.Message);
                Assert.DoesNotContain("System.", exception.Message);
                Assert.DoesNotContain("AgentSquad.", exception.Message);
                Assert.DoesNotContain(tempFilePath, exception.Message);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_LogsErrorBeforeThrowing()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var nonExistentFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "data.json");

            // Act
            await Assert.ThrowsAsync<DataLoadException>(() =>
                service.LoadProjectDataAsync(nonExistentFilePath));

            // Assert - Verify logging was called
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void ValidateJsonSchema_DoesNotThrowOnMalformedJson()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var malformedJson = "this is not json at all }{";

            // Act & Assert - Should not throw, only return false
            var result = service.ValidateJsonSchema(malformedJson);
            Assert.False(result);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidJson_CachesData()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var validJson = @"{
                ""project"": {
                    ""name"": ""Test Project"",
                    ""description"": ""Test Description"",
                    ""startDate"": ""2026-04-01"",
                    ""endDate"": ""2026-06-30"",
                    ""status"": ""OnTrack"",
                    ""sponsor"": ""Test Sponsor"",
                    ""projectManager"": ""Test PM""
                },
                ""milestones"": [],
                ""tasks"": [],
                ""metrics"": {
                    ""totalTasks"": 0,
                    ""completedTasks"": 0,
                    ""inProgressTasks"": 0,
                    ""carriedOverTasks"": 0,
                    ""completionPercentage"": 0,
                    ""estimatedBurndownRate"": 0.0,
                    ""projectStartDate"": ""2026-04-01"",
                    ""projectEndDate"": ""2026-06-30"",
                    ""daysRemaining"": 83
                }
            }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, validJson);

                // Act
                var data = await service.LoadProjectDataAsync(tempFilePath);
                var cachedData = service.GetCachedData();

                // Assert
                Assert.NotNull(data);
                Assert.NotNull(cachedData);
                Assert.Equal("Test Project", data.Project.Name);
                Assert.Equal("Test Project", cachedData.Project.Name);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public void RefreshData_ClearsCache()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);

            // Act
            service.RefreshData();
            var cachedData = service.GetCachedData();

            // Assert
            Assert.Null(cachedData);
        }
    }
}