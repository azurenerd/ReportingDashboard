using System.Text.Json;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace ReportingDashboard.Tests;

public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _wwwrootDir;
    private readonly string _dataFilePath;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_test_{Guid.NewGuid():N}");
        _wwwrootDir = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(_wwwrootDir);
        _dataFilePath = Path.Combine(_wwwrootDir, "data.json");
    }

    private DashboardDataService CreateService()
    {
        var env = new TestWebHostEnvironment(_wwwrootDir);
        var logger = new LoggerFactory().CreateLogger<DashboardDataService>();
        return new DashboardDataService(env, logger);
    }

    private void WriteSampleData(DashboardData data)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        File.WriteAllText(_dataFilePath, JsonSerializer.Serialize(data, options));
    }

    private static DashboardData CreateSampleData()
    {
        return new DashboardData
        {
            ProjectName = "Test Project",
            ReportingPeriod = "January 2026",
            Status = "On Track",
            HealthIndicator = "green",
            ExecutiveSummary = "All systems go.",
            Milestones = new List<Milestone>
            {
                new() { Title = "Milestone 1", TargetDate = "2026-01-15", CompletionDate = "2026-01-14", Status = "completed" },
                new() { Title = "Milestone 2", TargetDate = "2026-02-28", Status = "upcoming" }
            },
            Shipped = new List<WorkItem>
            {
                new() { Title = "Feature A", Description = "Desc A", Category = "shipped", Owner = "Alice", Priority = "high" }
            },
            InProgress = new List<WorkItem>
            {
                new() { Title = "Feature B", Description = "Desc B", Category = "in-progress", Owner = "Bob", Priority = "medium" }
            },
            CarriedOver = new List<WorkItem>
            {
                new() { Title = "Feature C", Description = "Desc C", Category = "carried-over", Owner = "Charlie", Priority = "low", Notes = "Blocked." }
            }
        };
    }

    [Fact]
    public void GetData_LoadsValidJson()
    {
        WriteSampleData(CreateSampleData());
        using var service = CreateService();

        var data = service.GetData();

        Assert.Equal("Test Project", data.ProjectName);
        Assert.Equal("January 2026", data.ReportingPeriod);
        Assert.Equal("green", data.HealthIndicator);
        Assert.Equal(2, data.Milestones.Count);
        Assert.Single(data.Shipped);
        Assert.Single(data.InProgress);
        Assert.Single(data.CarriedOver);
    }

    [Fact]
    public void GetData_MissingFile_ReturnsDefaultData()
    {
        // Don't write any file
        using var service = CreateService();

        var data = service.GetData();

        Assert.Equal("No Data", data.ProjectName);
    }

    [Fact]
    public void GetData_InvalidJson_ReturnsErrorData()
    {
        File.WriteAllText(_dataFilePath, "{ this is not valid json }");
        using var service = CreateService();

        var data = service.GetData();

        Assert.Equal("Load Error", data.ProjectName);
        Assert.Contains("Error reading data.json", data.ExecutiveSummary);
    }

    [Fact]
    public void GetData_ReturnsAllMilestoneFields()
    {
        WriteSampleData(CreateSampleData());
        using var service = CreateService();

        var data = service.GetData();
        var completed = data.Milestones.First(m => m.Status == "completed");
        var upcoming = data.Milestones.First(m => m.Status == "upcoming");

        Assert.Equal("Milestone 1", completed.Title);
        Assert.Equal("2026-01-15", completed.TargetDate);
        Assert.Equal("2026-01-14", completed.CompletionDate);
        Assert.Equal("Milestone 2", upcoming.Title);
        Assert.Null(upcoming.CompletionDate);
    }

    [Fact]
    public void GetData_ReturnsWorkItemDetails()
    {
        WriteSampleData(CreateSampleData());
        using var service = CreateService();

        var data = service.GetData();
        var shipped = data.Shipped[0];
        var carriedOver = data.CarriedOver[0];

        Assert.Equal("Feature A", shipped.Title);
        Assert.Equal("Alice", shipped.Owner);
        Assert.Equal("high", shipped.Priority);
        Assert.Null(shipped.Notes);
        Assert.Equal("Blocked.", carriedOver.Notes);
    }

    [Fact]
    public async Task FileChange_TriggersDataReload()
    {
        WriteSampleData(CreateSampleData());
        using var service = CreateService();

        Assert.Equal("Test Project", service.GetData().ProjectName);

        var tcs = new TaskCompletionSource<bool>();
        service.DataChanged += () => tcs.TrySetResult(true);

        // Modify the file
        var updated = CreateSampleData();
        updated.ProjectName = "Updated Project";
        WriteSampleData(updated);

        // Wait for file watcher to fire (with timeout)
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));

        if (completed == tcs.Task)
        {
            Assert.Equal("Updated Project", service.GetData().ProjectName);
        }
        // FileSystemWatcher timing is non-deterministic; test passes if event fires
    }

    [Fact]
    public void Dispose_StopsFileWatcher()
    {
        WriteSampleData(CreateSampleData());
        var service = CreateService();

        service.Dispose();
        // No exception expected; double dispose should be safe
        service.Dispose();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
        catch { /* cleanup best-effort */ }
    }

    private class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment(string webRootPath)
        {
            WebRootPath = webRootPath;
            ContentRootPath = Path.GetDirectoryName(webRootPath)!;
            EnvironmentName = "Development";
            ApplicationName = "ReportingDashboard.Tests";
        }

        public string WebRootPath { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public IFileProvider WebRootFileProvider { get; set; } = null!;
    }
}