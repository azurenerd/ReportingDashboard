using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
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
        Directory.CreateDirectory(Path.Combine(_tempDir, "Data"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private DashboardDataService CreateService()
    {
        var env = new TestWebHostEnvironment(_tempDir);
        return new DashboardDataService(env);
    }

    private void WriteJsonFile(string json)
    {
        var filePath = Path.Combine(_tempDir, "Data", "data.json");
        File.WriteAllText(filePath, json);
    }

    private static string BuildValidJson(
        string projectName = "Test Project",
        string overallStatus = "OnTrack",
        int milestoneCount = 1,
        int workItemCount = 1)
    {
        var milestones = Enumerable.Range(1, milestoneCount)
            .Select(i => $@"{{""id"":{i},""title"":""M{i}"",""targetDate"":""2026-01-01"",""completionDate"":null,""status"":""InProgress"",""description"":""Milestone {i}""}}")
            .ToList();

        var workItems = Enumerable.Range(1, workItemCount)
            .Select(i => $@"{{""id"":{100 + i},""title"":""WI{i}"",""description"":""Work item {i}"",""category"":""Shipped"",""milestoneId"":1,""owner"":""Owner {i}"",""priority"":""High"",""notes"":null,""statusIndicator"":""Done""}}")
            .ToList();

        return $@"{{
            ""project"": {{
                ""projectName"": ""{projectName}"",
                ""executiveSponsor"": ""Sponsor"",
                ""reportingPeriod"": ""March 2026"",
                ""lastUpdated"": ""2026-03-28"",
                ""overallStatus"": ""{overallStatus}"",
                ""summary"": ""Summary text""
            }},
            ""milestones"": [{string.Join(",", milestones)}],
            ""workItems"": [{string.Join(",", workItems)}]
        }}";
    }

    [Fact]
    public async Task GetDashboardDataAsync_ValidJson_ReturnsPopulatedData()
    {
        var json = BuildValidJson("Project Atlas", "OnTrack", milestoneCount: 3, workItemCount: 2);
        WriteJsonFile(json);
        var service = CreateService();

        var result = await service.GetDashboardDataAsync();

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
        // Don't write any file
        var service = CreateService();

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(
            () => service.GetDashboardDataAsync());
        Assert.Contains("data.json", ex.Message);
    }

    [Fact]
    public async Task GetDashboardDataAsync_MalformedJson_ThrowsJsonException()
    {
        WriteJsonFile("{ this is not valid json }}}");
        var service = CreateService();

        await Assert.ThrowsAsync<JsonException>(
            () => service.GetDashboardDataAsync());
    }

    [Fact]
    public async Task GetDashboardDataAsync_NoCaching_ReflectsFileChanges()
    {
        WriteJsonFile(BuildValidJson("Version1"));
        var service = CreateService();

        var result1 = await service.GetDashboardDataAsync();
        Assert.Equal("Version1", result1.Project.ProjectName);

        WriteJsonFile(BuildValidJson("Version2"));

        var result2 = await service.GetDashboardDataAsync();
        Assert.Equal("Version2", result2.Project.ProjectName);
    }

    private class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
        }

        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ApplicationName { get; set; } = "TestApp";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Development";
    }
}