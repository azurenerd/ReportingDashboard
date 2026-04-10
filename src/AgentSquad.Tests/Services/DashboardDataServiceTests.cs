using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AgentSquad.Core.Models;
using AgentSquad.Core.Services;

namespace AgentSquad.Tests.Services
{
    public class DashboardDataServiceTests : IDisposable
    {
        private readonly List<string> _tempFiles = new();
        private readonly Mock<ILogger<DashboardDataService>> _mockLogger;
        private readonly Mock<IOptions<DashboardOptions>> _mockOptions;

        public DashboardDataServiceTests()
        {
            _mockLogger = new Mock<ILogger<DashboardDataService>>();
            _mockOptions = new Mock<IOptions<DashboardOptions>>();
            
            _mockOptions.Setup(o => o.Value).Returns(new DashboardOptions
            {
                DataJsonPath = Path.Combine(Path.GetTempPath(), "data.json"),
                FileWatchDebounceMs = 500
            });
        }

        #region Helper Methods

        /// <summary>
        /// Creates valid JSON string representing DashboardData with sample project, milestones, and work items.
        /// </summary>
        protected string CreateValidJson()
        {
            var data = new
            {
                project = new
                {
                    name = "Sample Project",
                    description = "A sample project for testing"
                },
                milestones = new object[]
                {
                    new
                    {
                        name = "Phase 1 Complete",
                        date = "2026-03-01T00:00:00Z",
                        status = "Completed"
                    },
                    new
                    {
                        name = "Phase 2 In Progress",
                        date = "2026-04-15T00:00:00Z",
                        status = "On Track"
                    },
                    new
                    {
                        name = "Phase 3 Planned",
                        date = "2026-05-30T00:00:00Z",
                        status = "At Risk"
                    }
                },
                workItems = new object[]
                {
                    new { title = "Feature 1", status = "Shipped", assignee = "Alice" },
                    new { title = "Feature 2", status = "Shipped", assignee = "Bob" },
                    new { title = "Feature 3", status = "Shipped", assignee = "Charlie" },
                    new { title = "Feature 4", status = "Shipped", assignee = "Diana" },
                    new { title = "Bug Fix 1", status = "InProgress", assignee = "Eve" },
                    new { title = "Bug Fix 2", status = "InProgress", assignee = "Frank" },
                    new { title = "Bug Fix 3", status = "InProgress", assignee = "Grace" },
                    new { title = "Debt 1", status = "CarriedOver", assignee = "Henry" },
                    new { title = "Debt 2", status = "CarriedOver", assignee = "Iris" }
                }
            };

            return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Creates malformed JSON string with syntax errors (missing comma, unclosed brace).
        /// </summary>
        protected string CreateInvalidJson()
        {
            return @"{
  ""project"": {
    ""name"": ""Sample Project""
    ""description"": ""Missing comma above""
  }
  ""milestones"": []
}";
        }

        /// <summary>
        /// Creates a temporary file with the given content and tracks it for cleanup.
        /// Returns the full file path.
        /// </summary>
        protected string CreateTempDataJsonFile(string content)
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"data_{Guid.NewGuid()}.json");
            File.WriteAllText(tempFilePath, content, Encoding.UTF8);
            _tempFiles.Add(tempFilePath);
            return tempFilePath;
        }

        /// <summary>
        /// Creates a mocked FileSystemWatcher for testing file change events.
        /// Returns a Mock instance that can have OnChanged/OnCreated events triggered manually.
        /// </summary>
        protected Mock<FileSystemWatcher> CreateMockFileSystemWatcher()
        {
            var mockWatcher = new Mock<FileSystemWatcher>();
            mockWatcher.Setup(w => w.EnableRaisingEvents).Returns(true);
            mockWatcher.Setup(w => w.Dispose());
            return mockWatcher;
        }

        /// <summary>
        /// Creates a DashboardDataService instance with mocked dependencies.
        /// Configures the service to use the provided IOptions<DashboardOptions>.
        /// </summary>
        protected DashboardDataService CreateMockDataService(IOptions<DashboardOptions> options)
        {
            var service = new DashboardDataService(
                _mockLogger.Object,
                options ?? _mockOptions.Object
            );
            return service;
        }

        /// <summary>
        /// Creates a mocked IOptions<DashboardOptions> with custom path and debounce settings.
        /// </summary>
        protected IOptions<DashboardOptions> CreateMockOptions(string dataJsonPath, int debounceMs = 500)
        {
            var mock = new Mock<IOptions<DashboardOptions>>();
            mock.Setup(o => o.Value).Returns(new DashboardOptions
            {
                DataJsonPath = dataJsonPath,
                FileWatchDebounceMs = debounceMs
            });
            return mock.Object;
        }

        #endregion

        #region Fixtures and Setup/Teardown

        /// <summary>
        /// Creates a valid data.json file in a temporary location for integration testing.
        /// </summary>
        protected string SetupValidDataJsonFile()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"data_{Guid.NewGuid()}.json");
            File.WriteAllText(tempPath, CreateValidJson(), Encoding.UTF8);
            _tempFiles.Add(tempPath);
            return tempPath;
        }

        /// <summary>
        /// Creates an invalid data.json file with malformed JSON for error testing.
        /// </summary>
        protected string SetupInvalidDataJsonFile()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"data_{Guid.NewGuid()}.json");
            File.WriteAllText(tempPath, CreateInvalidJson(), Encoding.UTF8);
            _tempFiles.Add(tempPath);
            return tempPath;
        }

        /// <summary>
        /// Creates a non-UTF-8 encoded JSON file to test encoding validation.
        /// </summary>
        protected string SetupNonUtf8DataJsonFile()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"data_{Guid.NewGuid()}.json");
            var validJson = CreateValidJson();
            var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(validJson);
            File.WriteAllBytes(tempPath, bytes);
            _tempFiles.Add(tempPath);
            return tempPath;
        }

        #endregion

        #region Placeholder Tests (Ready for Implementation in Step 2+)

        [Fact]
        public void DashboardDataServiceTests_SetupComplete()
        {
            // Arrange: Use helper method to create valid JSON
            var validJson = CreateValidJson();

            // Assert: Verify helper produces valid JSON structure
            Assert.NotNull(validJson);
            Assert.Contains("\"project\"", validJson);
            Assert.Contains("\"milestones\"", validJson);
            Assert.Contains("\"workItems\"", validJson);
        }

        [Fact]
        public void DashboardDataServiceTests_InvalidJsonDetected()
        {
            // Arrange: Use helper method to create invalid JSON
            var invalidJson = CreateInvalidJson();

            // Assert: Verify invalid JSON contains syntax errors
            Assert.NotNull(invalidJson);
            Assert.Contains("Missing comma", invalidJson);
        }

        [Fact]
        public void DashboardDataServiceTests_TempFileCreation()
        {
            // Arrange: Create a temporary data.json file
            var tempPath = CreateTempDataJsonFile(CreateValidJson());

            // Assert: Verify file exists and is tracked for cleanup
            Assert.True(File.Exists(tempPath));
            Assert.Contains(tempPath, _tempFiles);
        }

        [Fact]
        public void DashboardDataServiceTests_MockOptionsConfiguration()
        {
            // Arrange: Create mock options with custom path
            var customPath = Path.Combine(Path.GetTempPath(), "custom_data.json");
            var options = CreateMockOptions(customPath, 600);

            // Assert: Verify options are configured correctly
            Assert.Equal(customPath, options.Value.DataJsonPath);
            Assert.Equal(600, options.Value.FileWatchDebounceMs);
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            // Cleanup all temporary files created during tests
            foreach (var tempFile in _tempFiles)
            {
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch (IOException)
                {
                    // File may be locked by FileSystemWatcher; ignore cleanup errors
                }
            }

            _tempFiles.Clear();
        }

        #endregion
    }
}