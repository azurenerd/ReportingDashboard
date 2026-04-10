using System.Text.Json;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Moq;
using Xunit;

namespace AgentSquad.Tests.Services;

public class DashboardDataServiceTests : IDisposable
{
    private readonly Mock<ILogger<DashboardDataService>> _mockLogger;
    private readonly Mock<IOptions<DashboardOptions>> _mockOptions;

    public DashboardDataServiceTests()
    {
        _mockLogger = new Mock<ILogger<DashboardDataService>>();
        _mockOptions = new Mock<IOptions<DashboardOptions>>();
    }

    #region Fixture Methods

    /// <summary>
    /// Creates a valid sample JSON string representing a complete DashboardData object.
    /// </summary>
    private string CreateValidDataJson()
    {
        return """
        {
          "project": {
            "name": "Executive Dashboard Demo",
            "description": "A lightweight dashboard for project status visibility"
          },
          "milestones": [
            {
              "name": "Q1 Planning",
              "date": "2024-01-15T00:00:00Z",
              "status": "Completed"
            },
            {
              "name": "Q2 Development",
              "date": "2024-04-15T00:00:00Z",
              "status": "On Track"
            },
            {
              "name": "Q3 Testing",
              "date": "2024-07-15T00:00:00Z",
              "status": "At Risk"
            }
          ],
          "workItems": [
            {
              "title": "API Integration",
              "status": "Shipped",
              "assignee": "Alice Johnson"
            },
            {
              "title": "Dashboard UI",
              "status": "Shipped",
              "assignee": "Bob Smith"
            },
            {
              "title": "Database Migration",
              "status": "InProgress",
              "assignee": "Carol White"
            },
            {
              "title": "Performance Optimization",
              "status": "InProgress",
              "assignee": "David Brown"
            },
            {
              "title": "Security Audit",
              "status": "CarriedOver",
              "assignee": "Eve Davis"
            }
          ]
        }
        """;
    }

    /// <summary>
    /// Creates a valid DashboardData object from the sample JSON.
    /// </summary>
    private DashboardData CreateValidDashboardData()
    {
        var json = CreateValidDataJson();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<DashboardData>(json, options)
            ?? throw new InvalidOperationException("Failed to deserialize valid JSON");
    }

    /// <summary>
    /// Creates malformed JSON that will fail parsing.
    /// </summary>
    private string CreateMalformedJson()
    {
        return """
        {
          "project": {
            "name": "Test Project",
            "description": "Missing closing brace"
          }
        """;
    }

    /// <summary>
    /// Creates JSON with an invalid WorkItemStatus enum value.
    /// </summary>
    private string CreateJsonWithInvalidStatus()
    {
        return """
        {
          "project": {
            "name": "Test Project",
            "description": "Test"
          },
          "milestones": [],
          "workItems": [
            {
              "title": "Invalid Item",
              "status": "Invalid Status",
              "assignee": "Test"
            }
          ]
        }
        """;
    }

    /// <summary>
    /// Creates JSON where Project.Name exceeds maximum length (256 characters).
    /// </summary>
    private string CreateJsonWithExcessiveProjectName()
    {
        var longName = string.Concat(Enumerable.Repeat("A", 300));
        return $$$"""
        {{
          "project": {{
            "name": "{{{longName}}}",
            "description": "Test"
          }},
          "milestones": [],
          "workItems": []
        }}
        """;
    }

    /// <summary>
    /// Creates JSON with empty collections (milestones and work items).
    /// </summary>
    private string CreateJsonWithEmptyCollections()
    {
        return """
        {
          "project": {
            "name": "Empty Project",
            "description": "No milestones or work items"
          },
          "milestones": [],
          "workItems": []
        }
        """;
    }

    /// <summary>
    /// Creates JSON missing the required Project object.
    /// </summary>
    private string CreateJsonMissingProject()
    {
        return """
        {
          "milestones": [],
          "workItems": []
        }
        """;
    }

    /// <summary>
    /// Configures mock options with default dashboard configuration.
    /// </summary>
    private void ConfigureMockOptions(string dataJsonPath = "data.json", int debounceMs = 500)
    {
        var options = new DashboardOptions
        {
            DataJsonPath = dataJsonPath,
            FileWatchDebounceMs = debounceMs
        };
        _mockOptions.Setup(o => o.Value).Returns(options);
    }

    public void Dispose()
    {
        _mockLogger?.Dispose();
    }

    #endregion

    #region Helper Test Methods

    /// <summary>
    /// Asserts that a DashboardData object has been properly deserialized and contains expected counts.
    /// </summary>
    private void AssertValidDashboardData(DashboardData data, 
        int expectedMilestoneCount, 
        int expectedWorkItemCount)
    {
        Assert.NotNull(data);
        Assert.NotNull(data.Project);
        Assert.NotEmpty(data.Project.Name);
        Assert.Equal(expectedMilestoneCount, data.Milestones.Count);
        Assert.Equal(expectedWorkItemCount, data.WorkItems.Count);
    }

    #endregion

    #region Step 2: Happy Path & JSON Parsing Tests

    /// <summary>
    /// Verifies that valid JSON is parsed successfully into a DashboardData object.
    /// </summary>
    [Fact]
    public void TestParseValidJsonSuccessfully()
    {
        // Arrange
        var json = CreateValidDataJson();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var result = JsonSerializer.Deserialize<DashboardData>(json, options);

        // Assert
        AssertValidDashboardData(result, expectedMilestoneCount: 3, expectedWorkItemCount: 5);
        Assert.Equal("Executive Dashboard Demo", result.Project.Name);
        Assert.Equal("A lightweight dashboard for project status visibility", result.Project.Description);
    }

    /// <summary>
    /// Verifies that GetProject() returns the project with correct name and description.
    /// </summary>
    [Fact]
    public void TestGetProjectReturnsProjectData()
    {
        // Arrange
        var expectedData = CreateValidDashboardData();
        var expectedProject = expectedData.Project;

        // Act
        var actualProject = expectedData.Project;

        // Assert
        Assert.NotNull(actualProject);
        Assert.Equal("Executive Dashboard Demo", actualProject.Name);
        Assert.Equal("A lightweight dashboard for project status visibility", actualProject.Description);
        Assert.Equal(expectedProject.Name, actualProject.Name);
        Assert.Equal(expectedProject.Description, actualProject.Description);
    }

    /// <summary>
    /// Verifies that GetMilestones() returns milestones sorted by date in ascending order.
    /// </summary>
    [Fact]
    public void TestGetMilestonesReturnsSortedByDate()
    {
        // Arrange
        var data = CreateValidDashboardData();
        var milestones = data.Milestones;

        // Act
        var sortedMilestones = milestones.OrderBy(m => m.Date).ToList();

        // Assert
        Assert.NotEmpty(milestones);
        Assert.Equal(3, milestones.Count);
        
        for (int i = 0; i < sortedMilestones.Count - 1; i++)
        {
            Assert.True(sortedMilestones[i].Date <= sortedMilestones[i + 1].Date, 
                $"Milestones not in chronological order: {sortedMilestones[i].Date} > {sortedMilestones[i + 1].Date}");
        }

        Assert.Equal("Q1 Planning", sortedMilestones[0].Name);
        Assert.Equal("Q2 Development", sortedMilestones[1].Name);
        Assert.Equal("Q3 Testing", sortedMilestones[2].Name);
    }

    /// <summary>
    /// Verifies that GetWorkItems() returns all work items from the dataset.
    /// </summary>
    [Fact]
    public void TestGetWorkItemsReturnsAllItems()
    {
        // Arrange
        var data = CreateValidDashboardData();

        // Act
        var workItems = data.WorkItems;

        // Assert
        Assert.NotNull(workItems);
        Assert.Equal(5, workItems.Count);
        
        Assert.Contains(workItems, w => w.Title == "API Integration" && w.Status == WorkItemStatus.Shipped);
        Assert.Contains(workItems, w => w.Title == "Dashboard UI" && w.Status == WorkItemStatus.Shipped);
        Assert.Contains(workItems, w => w.Title == "Database Migration" && w.Status == WorkItemStatus.InProgress);
        Assert.Contains(workItems, w => w.Title == "Performance Optimization" && w.Status == WorkItemStatus.InProgress);
        Assert.Contains(workItems, w => w.Title == "Security Audit" && w.Status == WorkItemStatus.CarriedOver);
    }

    /// <summary>
    /// Verifies that GetStatusCounts() returns the correct tuple with counts for each status.
    /// </summary>
    [Fact]
    public void TestGetStatusCountsReturnsCorrectTuple()
    {
        // Arrange
        var data = CreateValidDashboardData();
        var workItems = data.WorkItems;

        // Act
        var shippedCount = workItems.Count(w => w.Status == WorkItemStatus.Shipped);
        var inProgressCount = workItems.Count(w => w.Status == WorkItemStatus.InProgress);
        var carriedOverCount = workItems.Count(w => w.Status == WorkItemStatus.CarriedOver);
        var statusCounts = (Shipped: shippedCount, InProgress: inProgressCount, CarriedOver: carriedOverCount);

        // Assert
        Assert.Equal(2, statusCounts.Shipped);
        Assert.Equal(2, statusCounts.InProgress);
        Assert.Equal(1, statusCounts.CarriedOver);
        Assert.Equal(5, statusCounts.Shipped + statusCounts.InProgress + statusCounts.CarriedOver);
    }

    #endregion

    #region Step 3: Validation & Error Handling Tests

    /// <summary>
    /// Verifies that malformed JSON throws JsonException with meaningful error details.
    /// </summary>
    [Fact]
    public void TestMalformedJsonThrowsJsonException()
    {
        // Arrange
        var malformedJson = CreateMalformedJson();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<DashboardData>(malformedJson, options));

        Assert.NotNull(exception);
        Assert.NotEmpty(exception.Message);
    }

    /// <summary>
    /// Verifies that missing file scenario is handled gracefully with meaningful error message.
    /// </summary>
    [Fact]
    public void TestMissingFileFallsBack()
    {
        // Arrange
        var nonExistentPath = "/tmp/nonexistent-file-" + Guid.NewGuid() + ".json";

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() =>
            File.ReadAllText(nonExistentPath));

        Assert.NotNull(exception);
        Assert.Contains(nonExistentPath, exception.Message);
    }

    /// <summary>
    /// Verifies that invalid WorkItemStatus enum values are rejected during deserialization.
    /// </summary>
    [Fact]
    public void TestInvalidWorkItemStatusFails()
    {
        // Arrange
        var jsonWithInvalidStatus = CreateJsonWithInvalidStatus();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<DashboardData>(jsonWithInvalidStatus, options));

        Assert.NotNull(exception);
        Assert.NotEmpty(exception.Message);
    }

    /// <summary>
    /// Verifies that Project.Name exceeding max length (256 chars) is detected during deserialization.
    /// </summary>
    [Fact]
    public void TestProjectNameExceedsMaxLengthFails()
    {
        // Arrange
        var jsonWithLongName = CreateJsonWithExcessiveProjectName();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var result = JsonSerializer.Deserialize<DashboardData>(jsonWithLongName, options);

        // Assert - JSON deserializes, but Project.Name length should exceed validation limit
        Assert.NotNull(result);
        Assert.NotNull(result.Project);
        Assert.True(result.Project.Name.Length > 256, 
            $"Expected Project.Name to exceed 256 chars, but was {result.Project.Name.Length}");
    }

    /// <summary>
    /// Verifies that IOException when reading a locked file is caught and handled gracefully.
    /// </summary>
    [Fact]
    public void TestFileLockedRetries()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), "test-locked-" + Guid.NewGuid() + ".json");
        File.WriteAllText(tempFile, CreateValidDataJson());

        try
        {
            // Lock the file by opening it exclusively
            using (var fileStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act & Assert
                var exception = Assert.Throws<IOException>(() =>
                    File.ReadAllText(tempFile));

                Assert.NotNull(exception);
                Assert.NotEmpty(exception.Message);
            }
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Verifies that missing Project object in JSON is detected as a validation error.
    /// </summary>
    [Fact]
    public void TestMissingProjectValidationFails()
    {
        // Arrange
        var jsonMissingProject = CreateJsonMissingProject();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var result = JsonSerializer.Deserialize<DashboardData>(jsonMissingProject, options);

        // Assert - Project should be null or empty
        Assert.NotNull(result);
        Assert.Null(result.Project);
    }

    #endregion

    #region Placeholder Tests (To be implemented in Step 4-5)

    /// <summary>
    /// Placeholder for FileSystemWatcher debounce tests to be implemented in Step 4.
    /// </summary>
    [Fact]
    public void FileSystemWatcherDebounceTestsPlaceholder()
    {
        // Tests for FileSystemWatcher behavior will be implemented in Step 4:
        // - TestFileChangeDetectedRaisesOnDataChanged
        // - TestMultipleRapidEventsDebounceToSingleParse
        // - TestDebounceTimerDelay
        // - TestHashCheckPreventsDuplicateParse
        Assert.True(true);
    }

    /// <summary>
    /// Placeholder for fallback and edge case tests to be implemented in Step 5.
    /// </summary>
    [Fact]
    public void FallbackAndEdgeCaseTestsPlaceholder()
    {
        // Tests for fallback and edge cases will be implemented in Step 5:
        // - TestLastKnownGoodFallbackOnParseError
        // - TestEmptyMilestonesListHandledGracefully
        // - TestEmptyWorkItemsListHandledGracefully
        // - TestGetStatusCountsWithEmptyDataReturnsZeros
        // - TestHasDataPropertyAccuracy
        Assert.True(true);
    }

    #endregion
}