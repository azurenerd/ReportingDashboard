using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests.Services;

public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteTestJson(string json)
    {
        var filePath = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(filePath, json);
        return filePath;
    }

    [Fact]
    public async Task GetDashboardDataAsync_ValidFile_ReturnsDeserializedData()
    {
        var path = WriteTestJson("""
        {
            "projectInfo": {
                "projectName": "Test Atlas",
                "executiveSponsor": "Sponsor",
                "reportingPeriod": "Q1 2026",
                "overallStatus": "OnTrack",
                "summary": "All good."
            },
            "milestones": [
                { "id": "MS-1", "title": "First", "targetDate": "2026-01-15", "completionDate": "2026-01-14", "status": "Completed" }
            ],
            "workItems": [
                { "id": "WI-1", "title": "Task 1", "description": "Desc", "category": "Shipped", "milestoneId": "MS-1", "owner": "Dev", "priority": "High" }
            ]
        }
        """);

        var service = new DashboardDataService(path);
        var data = await service.GetDashboardDataAsync();

        Assert.Equal("Test Atlas", data.ProjectInfo.ProjectName);
        Assert.Single(data.Milestones);
        Assert.Single(data.WorkItems);
        Assert.Equal("Shipped", data.WorkItems[0].Category);
    }

    [Fact]
    public void GetDashboardData_Sync_ReturnsDeserializedData()
    {
        var path = WriteTestJson("""
        {
            "projectInfo": { "projectName": "Sync Test" },
            "milestones": [],
            "workItems": []
        }
        """);

        var service = new DashboardDataService(path);
        var data = service.GetDashboardData();

        Assert.Equal("Sync Test", data.ProjectInfo.ProjectName);
    }

    [Fact]
    public async Task GetDashboardDataAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        var service = new DashboardDataService(Path.Combine(_tempDir, "missing.json"));

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => service.GetDashboardDataAsync());
    }

    [Fact]
    public void GetDashboardData_FileNotFound_ThrowsFileNotFoundException()
    {
        var service = new DashboardDataService(Path.Combine(_tempDir, "missing.json"));

        Assert.Throws<FileNotFoundException>(() => service.GetDashboardData());
    }

    [Fact]
    public async Task GetDashboardDataAsync_ReReadsFileEachCall()
    {
        var path = WriteTestJson("""
        {
            "projectInfo": { "projectName": "Version 1" },
            "milestones": [],
            "workItems": []
        }
        """);

        var service = new DashboardDataService(path);
        var first = await service.GetDashboardDataAsync();
        Assert.Equal("Version 1", first.ProjectInfo.ProjectName);

        // Overwrite the file to simulate a PM edit
        File.WriteAllText(path, """
        {
            "projectInfo": { "projectName": "Version 2" },
            "milestones": [],
            "workItems": []
        }
        """);

        var second = await service.GetDashboardDataAsync();
        Assert.Equal("Version 2", second.ProjectInfo.ProjectName);
    }

    [Fact]
    public async Task GetDashboardDataAsync_WithTrailingCommasAndComments_Succeeds()
    {
        var path = WriteTestJson("""
        {
            // Project info
            "projectInfo": { "projectName": "Lenient Parse", },
            "milestones": [],
            "workItems": [],
        }
        """);

        var service = new DashboardDataService(path);
        var data = await service.GetDashboardDataAsync();

        Assert.Equal("Lenient Parse", data.ProjectInfo.ProjectName);
    }
}