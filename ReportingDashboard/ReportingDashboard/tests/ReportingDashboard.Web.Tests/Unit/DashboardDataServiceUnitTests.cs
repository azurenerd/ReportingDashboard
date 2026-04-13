using System.Text.Json;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceUnitTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardDataServiceUnitTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteJsonFile(string json, string fileName = "data.json")
    {
        var filePath = Path.Combine(_tempDir, fileName);
        File.WriteAllText(filePath, json);
        return filePath;
    }

    /// <summary>
    /// Builds valid JSON matching the architecture's data model with integer IDs,
    /// camelCase property names, and all fields including notes, statusIndicator,
    /// and lastUpdated as defined in the T1 models and PR description Step 1.
    /// </summary>
    private static string BuildValidJson(
        string projectName = "Test Project",
        string overallStatus = "OnTrack",
        int milestoneCount = 1,
        int workItemCount = 1)
    {
        var milestones = Enumerable.Range(1, milestoneCount)
            .Select(i => $@"{{
                ""id"": {i},
                ""title"": ""Milestone {i}"",
                ""targetDate"": ""2026-01-{i:D2}"",
                ""completionDate"": null,
                ""status"": ""InProgress"",
                ""description"": ""Description for milestone {i}""
            }}")
            .ToList();

        var workItems = Enumerable.Range(1, workItemCount)
            .Select(i => $@"{{
                ""id"": {100 + i},
                ""title"": ""Work Item {i}"",
                ""description"": ""Description for work item {i}"",
                ""category"": ""Shipped"",
                ""milestoneId"": 1,
                ""owner"": ""Owner {i}"",
                ""priority"": ""High"",
                ""notes"": ""Note for item {i}"",
                ""statusIndicator"": ""Done""
            }}")
            .ToList();

        return $@"{{
            ""project"": {{
                ""projectName"": ""{projectName}"",
                ""executiveSponsor"": ""Test Sponsor"",
                ""reportingPeriod"": ""March 2026"",
                ""lastUpdated"": ""2026-03-28"",
                ""overallStatus"": ""{overallStatus}"",
                ""summary"": ""Summary text for testing""
            }},
            ""milestones"": [{string.Join(",", milestones)}],
            ""workItems"": [{string.Join(",", workItems)}]
        }}";
    }

    [Fact]
    public void DashboardDataService_ImplementsIDashboardDataService()
    {
        // Validates feedback #1/#3: the concrete class must implement the interface
        var filePath = WriteJsonFile(BuildValidJson());
        var service = new DashboardDataService(filePath);

        Assert.IsAssignableFrom<IDashboardDataService>(service);
    }

    [Fact]
    public async Task GetDashboardDataAsync_ValidJson_ReturnsPopulatedData()
    {
        // Arrange
        var json = BuildValidJson("Project Atlas", "OnTrack", milestoneCount: 3, workItemCount: 2);
        var filePath = WriteJsonFile(json);
        var service = new DashboardDataService(filePath);

        // Act
        var result = await service.GetDashboardDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Project);
        Assert.Equal("Project Atlas", result.Project.ProjectName);
        Assert.Equal("OnTrack", result.Project.OverallStatus);
        Assert.Equal(3, result.Milestones.Count);
        Assert.Equal(2, result.WorkItems.Count);
    }

    [Fact]
    public async Task GetDashboardDataAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDir, "nonexistent.json");
        var service = new DashboardDataService(nonExistentPath);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<FileNotFoundException>(
            () => service.GetDashboardDataAsync());
        Assert.Contains(nonExistentPath, ex.Message);
    }

    [Fact]
    public async Task GetDashboardDataAsync_MalformedJson_ThrowsJsonException()
    {
        // Arrange
        var filePath = WriteJsonFile("{ this is not valid json }}}");
        var service = new DashboardDataService(filePath);

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(
            () => service.GetDashboardDataAsync());
    }

    [Fact]
    public async Task GetDashboardDataAsync_NoCaching_ReflectsFileChanges()
    {
        // Arrange — validates acceptance criteria: no in-memory caching
        var filePath = WriteJsonFile(BuildValidJson("Version1"));
        var service = new DashboardDataService(filePath);

        // Act - first read
        var result1 = await service.GetDashboardDataAsync();
        Assert.Equal("Version1", result1.Project.ProjectName);

        // Overwrite the file with different data
        File.WriteAllText(filePath, BuildValidJson("Version2"));

        // Act - second read should reflect new content (no caching)
        var result2 = await service.GetDashboardDataAsync();
        Assert.Equal("Version2", result2.Project.ProjectName);
    }

    [Fact]
    public void GetDashboardData_Sync_ReturnsPopulatedData()
    {
        // Arrange
        var json = BuildValidJson("Sync Test", "AtRisk", milestoneCount: 2, workItemCount: 3);
        var filePath = WriteJsonFile(json);
        var service = new DashboardDataService(filePath);

        // Act
        var result = service.GetDashboardData();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Sync Test", result.Project.ProjectName);
        Assert.Equal("AtRisk", result.Project.OverallStatus);
        Assert.Equal(2, result.Milestones.Count);
        Assert.Equal(3, result.WorkItems.Count);
    }
}