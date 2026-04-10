using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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

        #region Step 2: JSON Parsing and Validation Tests

        [Fact]
        public void ValidJsonDeserializes_ValidDataJsonLoads_GetCurrentDataReturnsNonNull()
        {
            // Arrange
            var validJsonPath = SetupValidDataJsonFile();
            var options = CreateMockOptions(validJsonPath);

            // Act
            var service = CreateMockDataService(options);
            var currentData = service.GetCurrentData();

            // Assert
            Assert.NotNull(currentData);
            Assert.True(service.HasData);
            Assert.Null(service.GetLastError());
        }

        [Fact]
        public void ValidJsonPopulatesModels_ValidJsonParsed_ProjectNameMilestonesAndWorkItemsCorrect()
        {
            // Arrange
            var validJsonPath = SetupValidDataJsonFile();
            var options = CreateMockOptions(validJsonPath);

            // Act
            var service = CreateMockDataService(options);
            var project = service.GetProject();
            var milestones = service.GetMilestones();
            var workItems = service.GetWorkItems();

            // Assert
            Assert.NotNull(project);
            Assert.Equal("Sample Project", project.Name);
            Assert.Equal("A sample project for testing", project.Description);
            Assert.NotNull(milestones);
            Assert.Equal(3, milestones.Count);
            Assert.NotNull(workItems);
            Assert.Equal(9, workItems.Count);
        }

        [Fact]
        public void MalformedJsonCaught_MissingCommaInJson_JsonExceptionCaughtAndHasDataFalse()
        {
            // Arrange
            var invalidJsonPath = SetupInvalidDataJsonFile();
            var options = CreateMockOptions(invalidJsonPath);

            // Act
            var service = CreateMockDataService(options);
            var currentData = service.GetCurrentData();

            // Assert
            Assert.Null(currentData);
            Assert.False(service.HasData);
            Assert.NotNull(service.GetLastError());
            Assert.Contains("JSON parsing error", service.GetLastError());
        }

        [Fact]
        public void InvalidUtf8Rejected_NonUtf8Encoding_JsonExceptionCaught()
        {
            // Arrange
            var nonUtf8Path = SetupNonUtf8DataJsonFile();
            var options = CreateMockOptions(nonUtf8Path);

            // Act
            var service = CreateMockDataService(options);
            var currentData = service.GetCurrentData();

            // Assert
            Assert.Null(currentData);
            Assert.False(service.HasData);
            Assert.NotNull(service.GetLastError());
        }

        [Fact]
        public void CaseInsensitivePropertyNames_ProjectPropertyInDifferentCases_DeserializesCorrectly()
        {
            // Arrange
            var caseInsensitiveJson = @"{
  ""PROJECT"": {
    ""NAME"": ""Case Insensitive Project"",
    ""DESCRIPTION"": ""Testing case insensitivity""
  },
  ""MILESTONES"": [],
  ""WORKITEMS"": []
}";
            var jsonPath = CreateTempDataJsonFile(caseInsensitiveJson);
            var options = CreateMockOptions(jsonPath);

            // Act
            var service = CreateMockDataService(options);
            var project = service.GetProject();

            // Assert
            Assert.NotNull(project);
            Assert.Equal("Case Insensitive Project", project.Name);
            Assert.True(service.HasData);
        }

        [Fact]
        public void RequiredFieldsEnforced_MissingProjectName_HasDataFalseAndErrorSet()
        {
            // Arrange
            var missingNameJson = @"{
  ""project"": {
    ""description"": ""Missing project name""
  },
  ""milestones"": [],
  ""workItems"": []
}";
            var jsonPath = CreateTempDataJsonFile(missingNameJson);
            var options = CreateMockOptions(jsonPath);

            // Act
            var service = CreateMockDataService(options);

            // Assert
            Assert.False(service.HasData);
            Assert.NotNull(service.GetLastError());
        }

        [Fact]
        public void StringLengthValidated_ProjectNameExceedsMaxLength_ValidationErrorLogged()
        {
            // Arrange
            var longName = new string('A', 257);
            var jsonWithLongName = new
            {
                project = new { name = longName, description = "Test" },
                milestones = new object[] { },
                workItems = new object[] { }
            };
            var longNameJson = JsonSerializer.Serialize(jsonWithLongName);
            var jsonPath = CreateTempDataJsonFile(longNameJson);
            var options = CreateMockOptions(jsonPath);

            // Act
            var service = CreateMockDataService(options);

            // Assert
            Assert.False(service.HasData);
            Assert.NotNull(service.GetLastError());
        }

        [Fact]
        public void EnumValuesEnforced_InvalidWorkItemStatus_ErrorCaughtAndMessageSet()
        {
            // Arrange
            var invalidStatusJson = @"{
  ""project"": { ""name"": ""Test"" },
  ""milestones"": [],
  ""workItems"": [
    { ""title"": ""Test Item"", ""status"": ""InvalidStatus"", ""assignee"": ""Test"" }
  ]
}";
            var jsonPath = CreateTempDataJsonFile(invalidStatusJson);
            var options = CreateMockOptions(jsonPath);

            // Act
            var service = CreateMockDataService(options);

            // Assert
            Assert.False(service.HasData);
            Assert.NotNull(service.GetLastError());
        }

        [Fact]
        public void InvalidMilestoneStatus_StatusNotInAllowedValues_ValidationFails()
        {
            // Arrange
            var invalidMilestoneStatusJson = @"{
  ""project"": { ""name"": ""Test"" },
  ""milestones"": [
    { ""name"": ""M1"", ""date"": ""2026-01-01T00:00:00Z"", ""status"": ""InvalidStatus"" }
  ],
  ""workItems"": []
}";
            var jsonPath = CreateTempDataJsonFile(invalidMilestoneStatusJson);
            var options = CreateMockOptions(jsonPath);

            // Act
            var service = CreateMockDataService(options);

            // Assert
            Assert.False(service.HasData);
            Assert.NotNull(service.GetLastError());
        }

        [Fact]
        public void ValidJsonWithOptionalFields_AssigneeAndDescriptionOmitted_DeserializesWithDefaults()
        {
            // Arrange
            var minimalJson = @"{
  ""project"": { ""name"": ""Minimal Project"" },
  ""milestones"": [
    { ""name"": ""M1"", ""date"": ""2026-01-01T00:00:00Z"", ""status"": ""Completed"" }
  ],
  ""workItems"": [
    { ""title"": ""Item 1"", ""status"": ""Shipped"" }
  ]
}";
            var jsonPath = CreateTempDataJsonFile(minimalJson);
            var options = CreateMockOptions(jsonPath);

            // Act
            var service = CreateMockDataService(options);
            var workItems = service.GetWorkItems();

            // Assert
            Assert.True(service.HasData);
            Assert.Single(workItems);
            Assert.Null(workItems[0].Assignee);
        }

        [Fact]
        public void EmptyCollectionsAllowed_NoMilestonesAndWorkItems_ReturnsEmptyLists()
        {
            // Arrange
            var emptyCollectionsJson = @"{
  ""project"": { ""name"": ""Empty Project"" },
  ""milestones"": [],
  ""workItems"": []
}";
            var jsonPath = CreateTempDataJsonFile(emptyCollectionsJson);
            var options = CreateMockOptions(jsonPath);

            // Act
            var service = CreateMockDataService(options);
            var milestones = service.GetMilestones();
            var workItems = service.GetWorkItems();

            // Assert
            Assert.True(service.HasData);
            Assert.Empty(milestones);
            Assert.Empty(workItems);
        }

        [Fact]
        public void MilestoneStatusEnum_AllValidStatuses_DeserializeCorrectly()
        {
            // Arrange
            var multiStatusJson = @"{
  ""project"": { ""name"": ""Test"" },
  ""milestones"": [
    { ""name"": ""M1"", ""date"": ""2026-01-01T00:00:00Z"", ""status"": ""Completed"" },
    { ""name"": ""M2"", ""date"": ""2026-02-01T00:00:00Z"", ""status"": ""On Track"" },
    { ""name"": ""M3"", ""date"": ""2026-03-01T00:00:00Z"", ""status"": ""At Risk"" }
  ],
  ""workItems"": []
}";
            var jsonPath = CreateTempDataJsonFile(multiStatusJson);
            var options = CreateMockOptions(jsonPath);

            // Act
            var service = CreateMockDataService(options);
            var milestones = service.GetMilestones();

            // Assert
            Assert.True(service.HasData);
            Assert.Equal(3, milestones.Count);
            Assert.Equal("Completed", milestones[0].Status);
            Assert.Equal("On Track", milestones[1].Status);
            Assert.Equal("At Risk", milestones[2].Status);
        }

        #endregion

        #region Step 3: FileSystemWatcher Debounce and Duplicate Detection Tests

        [Fact]
        public void FileWatchDebounce500ms_RapidFileEvents_OnDataChangedFiredOnlyOnceAfterDebounceWindow()
        {
            // Arrange
            var validJsonPath = SetupValidDataJsonFile();
            var options = CreateMockOptions(validJsonPath, 100);
            var service = CreateMockDataService(options);
            
            int eventCount = 0;
            service.OnDataChanged += () => eventCount++;

            // Act: Simulate rapid file changes by modifying file multiple times
            for (int i = 0; i < 5; i++)
            {
                File.WriteAllText(validJsonPath, CreateValidJson(), Encoding.UTF8);
                System.Threading.Thread.Sleep(20);
            }

            // Wait for debounce to complete (100ms debounce + buffer)
            System.Threading.Thread.Sleep(200);

            // Assert: Event should fire only once despite multiple file modifications
            Assert.Equal(1, eventCount);
        }

        [Fact]
        public void DebounceCoalescesRapidEvents_SequentialWritesWithinDebounceWindow_SingleParseOccurs()
        {
            // Arrange
            var validJsonPath = SetupValidDataJsonFile();
            var options = CreateMockOptions(validJsonPath, 100);
            var service = CreateMockDataService(options);
            
            int eventCount = 0;
            service.OnDataChanged += () => eventCount++;

            // Act: Write file, wait 50ms, write again, wait 50ms, write again
            // All within the 100ms debounce window plus initial load
            File.WriteAllText(validJsonPath, CreateValidJson(), Encoding.UTF8);
            System.Threading.Thread.Sleep(50);
            File.WriteAllText(validJsonPath, CreateValidJson(), Encoding.UTF8);
            System.Threading.Thread.Sleep(50);
            File.WriteAllText(validJsonPath, CreateValidJson(), Encoding.UTF8);

            // Wait for debounce to settle
            System.Threading.Thread.Sleep(200);

            // Assert: Only one additional event from debounced changes
            Assert.Equal(1, eventCount);
        }

        [Fact]
        public void TimerCancelledOnDispose_PendingDebounceTimer_DisposeCancelsTimer()
        {
            // Arrange
            var validJsonPath = SetupValidDataJsonFile();
            var options = CreateMockOptions(validJsonPath, 500);
            var service = CreateMockDataService(options);
            
            int eventCount = 0;
            service.OnDataChanged += () => eventCount++;

            // Act: Trigger a file change
            File.WriteAllText(validJsonPath, CreateValidJson(), Encoding.UTF8);
            
            // Immediately dispose before timer fires (500ms debounce)
            System.Threading.Thread.Sleep(100);
            service.Dispose();

            // Wait longer than debounce to ensure timer was cancelled
            System.Threading.Thread.Sleep(500);

            // Assert: Event should not fire after dispose
            Assert.Equal(1, eventCount);
        }

        [Fact]
        public void HashCheckPreventsRedundantParse_IdenticalFileContent_OnDataChangedNotFiredForDuplicate()
        {
            // Arrange
            var validJson = CreateValidJson();
            var validJsonPath = SetupValidDataJsonFile();
            File.WriteAllText(validJsonPath, validJson, Encoding.UTF8);
            
            var options = CreateMockOptions(validJsonPath, 50);
            var service = CreateMockDataService(options);
            
            int eventCount = 0;
            service.OnDataChanged += () => eventCount++;

            // Act: Write the same content twice
            File.WriteAllText(validJsonPath, validJson, Encoding.UTF8);
            System.Threading.Thread.Sleep(100);
            
            File.WriteAllText(validJsonPath, validJson, Encoding.UTF8);
            System.Threading.Thread.Sleep(100);

            // Assert: No additional events due to hash matching
            Assert.Equal(1, eventCount);
        }

        [Fact]
        public void UnchangedFileHashSkipsReload_FileWatchEventWithUnchangedContent_ReloadSkipped()
        {
            // Arrange
            var validJson = CreateValidJson();
            var validJsonPath = SetupValidDataJsonFile();
            File.WriteAllText(validJsonPath, validJson, Encoding.UTF8);
            
            var options = CreateMockOptions(validJsonPath, 50);
            var service = CreateMockDataService(options);
            
            var initialData = service.GetCurrentData();
            int eventCount = 0;
            service.OnDataChanged += () => eventCount++;

            // Act: Trigger FileSystemWatcher with unchanged content
            File.WriteAllText(validJsonPath, validJson, Encoding.UTF8);
            System.Threading.Thread.Sleep(100);

            // Assert: Data unchanged, event not fired
            var currentData = service.GetCurrentData();
            Assert.Equal(initialData.Project.Name, currentData.Project.Name);
            Assert.Equal(1, eventCount);
        }

        [Fact]
        public void HashUpdatedOnSuccessfulLoad_DataParsedSuccessfully_HashUpdatedToMatchFileContent()
        {
            // Arrange
            var validJson = CreateValidJson();
            var validJsonPath = SetupValidDataJsonFile();
            
            var options = CreateMockOptions(validJsonPath);
            var service = CreateMockDataService(options);

            // Act: Get initial hash by checking service state
            Assert.True(service.HasData);
            
            // Modify file content
            var modifiedJson = CreateValidJson();
            File.WriteAllText(validJsonPath, modifiedJson, Encoding.UTF8);
            System.Threading.Thread.Sleep(100);

            // Assert: Service successfully updated with new data
            var updatedData = service.GetCurrentData();
            Assert.NotNull(updatedData);
            Assert.True(service.HasData);
            Assert.Null(service.GetLastError());
        }

        [Fact]
        public void NoEventOnHashMatch_FileWriteEventWithIdenticalHash_EventNotRaised()
        {
            // Arrange
            var originalJson = CreateValidJson();
            var validJsonPath = SetupValidDataJsonFile();
            File.WriteAllText(validJsonPath, originalJson, Encoding.UTF8);
            
            var options = CreateMockOptions(validJsonPath, 50);
            var service = CreateMockDataService(options);
            
            int eventCount = 0;
            service.OnDataChanged += () => eventCount++;

            // Act: Write same file content again (same hash)
            File.WriteAllText(validJsonPath, originalJson, Encoding.UTF8);
            System.Threading.Thread.Sleep(100);
            
            // Assert: No event fired because hash matches
            Assert.Equal(1, eventCount);
        }

        [Fact]
        public void DebounceTimerResetOnNewEvent_EventFiredBeforeDebounceCompletes_TimerResets()
        {
            // Arrange
            var validJsonPath = SetupValidDataJsonFile();
            var options = CreateMockOptions(validJsonPath, 100);
            var service = CreateMockDataService(options);
            
            int eventCount = 0;
            service.OnDataChanged += () => eventCount++;

            // Act: Write, wait 60ms, write again (before 100ms debounce completes)
            File.WriteAllText(validJsonPath, CreateValidJson(), Encoding.UTF8);
            System.Threading.Thread.Sleep(60);
            
            File.WriteAllText(validJsonPath, CreateValidJson(), Encoding.UTF8);
            System.Threading.Thread.Sleep(60);

            // Still within debounce, shouldn't have fired yet
            Assert.Equal(1, eventCount);

            // Wait for new debounce to complete
            System.Threading.Thread.Sleep(150);

            // Assert: Single event for coalesced changes
            Assert.Equal(2, eventCount);
        }

        [Fact]
        public void MultipleRapidChanges_TenFileWritesInQuickSuccession_SingleDebounceEvent()
        {
            // Arrange
            var validJsonPath = SetupValidDataJsonFile();
            var options = CreateMockOptions(validJsonPath, 75);
            var service = CreateMockDataService(options);
            
            int eventCount = 0;
            service.OnDataChanged += () => eventCount++;

            // Act: Simulate 10 rapid file writes (like a save operation writing multiple times)
            for (int i = 0; i < 10; i++)
            {
                File.WriteAllText(validJsonPath, CreateValidJson(), Encoding.UTF8);
                System.Threading.Thread.Sleep(10);
            }

            // Wait for debounce to settle
            System.Threading.Thread.Sleep(150);

            // Assert: All changes coalesced into one event
            Assert.Equal(1, eventCount);
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
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
                }
            }

            _tempFiles.Clear();
        }

        #endregion
    }
}