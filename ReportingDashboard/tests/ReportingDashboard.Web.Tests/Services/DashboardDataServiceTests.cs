using Microsoft.AspNetCore.Hosting;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests.Services;

public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_test_{Guid.NewGuid():N}");
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

    private void WriteTestJson(string json)
    {
        var filePath = Path.Combine(_tempDir, "Data", "data.json");
        File.WriteAllText(filePath, json);
    }

    [Fact]
    public async Task GetDashboardDataAsync_ValidFile_ReturnsDeserializedData()
    {
        WriteTestJson("""
        {
            "project": {
                "projectName": "Test Atlas",
                "executiveSponsor": "Sponsor",
                "reportingPeriod": "Q1 2026",
                "lastUpdated": "2026-03-28",
                "overallStatus": "OnTrack",
                "summary": "All good."
            },
            "milestones": [
                { "id": 1, "title": "First", "targetDate": "2026-01-15", "completionDate": "2026-01-14", "status": "Completed", "description": "Desc" }
            ],
            "workItems": [
                { "id": 101, "title": "Task 1", "description": "Desc", "category": "Shipped", "milestoneId": 1, "owner": "Dev", "priority": "High", "notes": null, "statusIndicator": "Done" }
            ]
        }
        """);

        var service = CreateService();
        var data = await service.GetDashboardDataAsync();

        Assert.Equal("Test Atlas", data.Project.ProjectName);
        Assert.Single(data.Milestones);
        Assert.Single(data.WorkItems);
        Assert.Equal("Shipped", data.WorkItems[0].Category);
    }

    [Fact]
    public async Task GetDashboardDataAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        // Don't write any file
        var service = CreateService();

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => service.GetDashboardDataAsync());
    }

    [Fact]
    public async Task GetDashboardDataAsync_ReReadsFileEachCall()
    {
        WriteTestJson("""
        {
            "project": { "projectName": "Version 1" },
            "milestones": [],
            "workItems": []
        }
        """);

        var service = CreateService();
        var first = await service.GetDashboardDataAsync();
        Assert.Equal("Version 1", first.Project.ProjectName);

        WriteTestJson("""
        {
            "project": { "projectName": "Version 2" },
            "milestones": [],
            "workItems": []
        }
        """);

        var second = await service.GetDashboardDataAsync();
        Assert.Equal("Version 2", second.Project.ProjectName);
    }

    [Fact]
    public async Task GetDashboardDataAsync_WithTrailingCommasAndComments_Succeeds()
    {
        WriteTestJson("""
        {
            // Project info
            "project": { "projectName": "Lenient Parse", },
            "milestones": [],
            "workItems": [],
        }
        """);

        var service = CreateService();
        var data = await service.GetDashboardDataAsync();

        Assert.Equal("Lenient Parse", data.Project.ProjectName);
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