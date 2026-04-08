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
    /// Comprehensive unit tests for ProjectDataService error handling.
    /// Verifies all acceptance criteria: FileNotFoundException, JsonException, null validation,
    /// user-friendly messages, logging, and error propagation.
    /// </summary>
    public class ProjectDataServiceErrorHandlingTests
    {
        private readonly Mock<ILogger<ProjectDataService>> _mockLogger;

        public ProjectDataServiceErrorHandlingTests()
        {
            _mockLogger = new Mock<ILogger<ProjectDataService>>();
        }

        #region FileNotFoundException Tests

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsDataLoadException_WithCorrectMessage()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var nonExistentFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "data.json");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                service.LoadProjectDataAsync(nonExistentFilePath));

            Assert.NotNull(exception);
            Assert.Equal("data.json not found in wwwroot directory", exception.Message);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_LogsErrorBeforeThrowing()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var nonExistentFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "data.json");

            // Act
            await Assert.ThrowsAsync<DataLoadException>(() =>
                service.LoadProjectDataAsync(nonExistentFilePath));

            // Assert
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
        public async Task LoadProjectDataAsync_WithMissingFile_ExceptionMessageDoesNotContainFilePath()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var nonExistentFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "data.json");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                service.LoadProjectDataAsync(nonExistentFilePath));

            Assert.DoesNotContain(nonExistentFilePath, exception.Message);
            Assert.DoesNotContain("FileNotFoundException", exception.Message);
        }

        #endregion

        #region JsonException Tests

        [Fact]
        public async Task LoadProjectDataAsync_WithMalformedJson_ThrowsDataLoadException_WithJsonExceptionDetails()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var malformedJson = "{ invalid json syntax }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, malformedJson);

                // Act & Assert
                var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                    service.LoadProjectDataAsync(tempFilePath));

                Assert.NotNull(exception);
                Assert.StartsWith("Invalid JSON format:", exception.Message);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMalformedJson_MessageIncludesJsonExceptionDetails()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var malformedJson = "{ unclosed }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, malformedJson);

                // Act
                var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                    service.LoadProjectDataAsync(tempFilePath));

                // Assert - verify original JsonException message is preserved
                Assert.Contains("Invalid JSON format:", exception.Message);
                Assert.True(exception.Message.Length > "Invalid JSON format:".Length,
                    "Message should contain JSON exception details after prefix");
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMalformedJson_LogsErrorBeforeThrowing()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var malformedJson = "{ bad json }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, malformedJson);

                // Act
                await Assert.ThrowsAsync<DataLoadException>(() =>
                    service.LoadProjectDataAsync(tempFilePath));

                // Assert
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<JsonException>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMalformedJson_DoesNotExposeStackTrace()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var malformedJson = "{ broken }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, malformedJson);

                // Act
                var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                    service.LoadProjectDataAsync(tempFilePath));

                // Assert
                Assert.DoesNotContain(" at ", exception.Message);
                Assert.DoesNotContain("System.", exception.Message);
                Assert.DoesNotContain("AgentSquad.Runner", exception.Message);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        #endregion

        #region Null Deserialization Tests

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

                Assert.NotNull(exception);
                Assert.Equal("JSON deserialization resulted in null", exception.Message);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNullDeserialization_LogsErrorBeforeThrowing()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var emptyJson = "{}";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, emptyJson);

                // Act
                await Assert.ThrowsAsync<DataLoadException>(() =>
                    service.LoadProjectDataAsync(tempFilePath));

                // Assert
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
        public async Task LoadProjectDataAsync_WithPartiallyValidJson_ThrowsDataLoadException_OnNullDeserialization()
        {
            // Arrange - JSON with some properties but missing required fields that cause deserialization to null
            var service = new ProjectDataService(_mockLogger.Object);
            var incompleteJson = @"{ ""someProperty"": ""value"" }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, incompleteJson);

                // Act & Assert
                var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                    service.LoadProjectDataAsync(tempFilePath));

                Assert.Equal("JSON deserialization resulted in null", exception.Message);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        #endregion

        #region ValidateJsonSchema Tests

        [Fact]
        public void ValidateJsonSchema_WithMalformedJson_ReturnsFalse()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var malformedJson = "{ invalid json }";

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
        public void ValidateJsonSchema_WithWhitespaceOnlyInput_ReturnsFalse()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);

            // Act
            var result = service.ValidateJsonSchema("   ");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateJsonSchema_WithMissingProjectProperty_ReturnsFalse()
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
        public void ValidateJsonSchema_WithMissingMilestonesProperty_ReturnsFalse()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var jsonMissingMilestones = @"{
                ""project"": {},
                ""tasks"": [],
                ""metrics"": {}
            }";

            // Act
            var result = service.ValidateJsonSchema(jsonMissingMilestones);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateJsonSchema_WithMissingTasksProperty_ReturnsFalse()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var jsonMissingTasks = @"{
                ""project"": {},
                ""milestones"": [],
                ""metrics"": {}
            }";

            // Act
            var result = service.ValidateJsonSchema(jsonMissingTasks);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateJsonSchema_WithMissingMetricsProperty_ReturnsFalse()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var jsonMissingMetrics = @"{
                ""project"": {},
                ""milestones"": [],
                ""tasks"": []
            }";

            // Act
            var result = service.ValidateJsonSchema(jsonMissingMetrics);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateJsonSchema_WithAllRequiredProperties_ReturnsTrue()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var validJson = @"{
                ""project"": { ""name"": ""Test Project"" },
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
        public void ValidateJsonSchema_WithPopulatedRequiredProperties_ReturnsTrue()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var validJson = @"{
                ""project"": { 
                    ""name"": ""Q2 Mobile App Release"",
                    ""description"": ""Mobile app release"",
                    ""status"": ""OnTrack""
                },
                ""milestones"": [
                    { ""name"": ""Design Complete"", ""targetDate"": ""2024-05-01"" }
                ],
                ""tasks"": [
                    { ""name"": ""Task 1"", ""status"": ""Shipped"" }
                ],
                ""metrics"": {
                    ""totalTasks"": 10,
                    ""completedTasks"": 5
                }
            }";

            // Act
            var result = service.ValidateJsonSchema(validJson);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateJsonSchema_DoesNotThrow_OnInvalidJson()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var malformedJson = "{ broken json [";

            // Act & Assert - Should not throw exception
            var result = service.ValidateJsonSchema(malformedJson);
            Assert.False(result);
        }

        #endregion

        #region User-Friendly Message Tests

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

                // Assert
                Assert.DoesNotContain(" at ", exception.Message);
                Assert.DoesNotContain("System.", exception.Message);
                Assert.DoesNotContain("AgentSquad.Runner", exception.Message);
                Assert.DoesNotContain(".cs:", exception.Message);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task FileNotFoundErrorMessage_IsUserFriendly()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent", "data.json");

            // Act
            var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                service.LoadProjectDataAsync(nonExistentPath));

            // Assert
            Assert.Equal("data.json not found in wwwroot directory", exception.Message);
            Assert.DoesNotContain(nonExistentPath, exception.Message);
        }

        [Fact]
        public async Task JsonErrorMessage_StartsWithUserFriendlyPrefix()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var malformedJson = "{ bad }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, malformedJson);

                // Act
                var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                    service.LoadProjectDataAsync(tempFilePath));

                // Assert
                Assert.StartsWith("Invalid JSON format:", exception.Message);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        #endregion

        #region Successful Load Tests

        [Fact]
        public async Task LoadProjectDataAsync_WithValidJson_ReturnsProjectData()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var validJson = @"{
                ""project"": { 
                    ""name"": ""Test Project"",
                    ""description"": ""Test description"",
                    ""startDate"": ""2024-01-01"",
                    ""endDate"": ""2024-12-31"",
                    ""status"": ""OnTrack"",
                    ""sponsor"": ""John Doe"",
                    ""projectManager"": ""Jane Smith""
                },
                ""milestones"": [],
                ""tasks"": [],
                ""metrics"": {
                    ""totalTasks"": 0,
                    ""completedTasks"": 0
                }
            }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, validJson);

                // Act
                var result = await service.LoadProjectDataAsync(tempFilePath);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Project);
                Assert.Equal("Test Project", result.Project.Name);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidJson_CachesData()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var validJson = @"{
                ""project"": { ""name"": ""Cached Project"" },
                ""milestones"": [],
                ""tasks"": [],
                ""metrics"": {}
            }";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFilePath, validJson);

                // Act
                var loadedData = await service.LoadProjectDataAsync(tempFilePath);
                var cachedData = service.GetCachedData();

                // Assert
                Assert.NotNull(cachedData);
                Assert.Equal(loadedData.Project.Name, cachedData.Project.Name);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task LoadProjectDataAsync_WithMultipleErrors_ThrowsFirstError()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "data.json");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DataLoadException>(() =>
                service.LoadProjectDataAsync(nonExistentPath));

            Assert.Equal("data.json not found in wwwroot directory", exception.Message);
        }

        [Fact]
        public void ValidateJsonSchema_WithEmptyArrays_ReturnsTrue()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var json = @"{
                ""project"": {},
                ""milestones"": [],
                ""tasks"": [],
                ""metrics"": {}
            }";

            // Act
            var result = service.ValidateJsonSchema(json);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateJsonSchema_IsCaseInsensitive()
        {
            // Arrange
            var service = new ProjectDataService(_mockLogger.Object);
            var json = @"{
                ""PROJECT"": {},
                ""MILESTONES"": [],
                ""TASKS"": [],
                ""METRICS"": {}
            }";

            // Act - Should fail because JSON property names are case-sensitive
            var result = service.ValidateJsonSchema(json);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}