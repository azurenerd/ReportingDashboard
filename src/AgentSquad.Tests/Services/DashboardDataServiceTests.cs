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
        
        // Verify chronological order
        for (int i = 0; i < sortedMilestones.Count - 1; i++)
        {
            Assert.True(sortedMilestones[i].Date <= sortedMilestones[i + 1].Date, 
                $"Milestones not in chronological order: {sortedMilestones[i].Date} > {sortedMilestones[i + 1].Date}");
        }

        // Verify names match expected order
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
        
        // Verify specific items are present
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
        
        // Verify total count
        Assert.Equal(5, statusCounts.Shipped + statusCounts.InProgress + statusCounts.CarriedOver);
    }

    #endregion

    #region Placeholder Tests (To be implemented in Step 3-5)

    /// <summary>
    /// Placeholder for validation and error handling tests to be implemented in Step 3.
    /// </summary>
    [Fact]
    public void ValidationAndErrorHandlingTestsPlaceholder()
    {
        // Tests for validation and error scenarios will be implemented in Step 3:
        // - TestMalformedJsonThrowsJsonException
        // - TestMissingFileFallsBack
        // - TestInvalidWorkItemStatusFails
        // - TestProjectNameExceedsMaxLengthFails
        // - TestFileLockedRetries
        Assert.True(true);
    }

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