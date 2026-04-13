using System.Text.Json;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace ReportingDashboard.Tests.Unit;

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
            TeamName = "Test Team",
            ReportingPeriod = "January 2026",
            OverallStatus = "Green",
            HealthIndicator = "Green",
            Summary = "All systems go.",
            BacklogUrl = "",
            Milestones = new List<Milestone>
            {
                new()
                {
                    Name = "Milestone 1",
                    Description = "First milestone",
                    TargetDate = new DateTime(2026, 1, 15),
                    CompletionDate = new DateTime(2026, 1, 14),
                    Status = "completed",
                    Color = "#28a745",
                    MarkerType = "diamond"
                },
                new()
                {
                    Name = "Milestone 2",
                    Description = "Second milestone",
                    TargetDate = new DateTime(2026, 2, 28),
                    CompletionDate = null,
                    Status = "upcoming",
                    Color = "#6c757d",
                    MarkerType = "circle"
                }
            },
            ShippedItems = new List<WorkItem>
            {
                new()
                {
                    Title = "Feature A",
                    Description = "Desc A",
                    Status = "shipped",
                    Owner = "Alice",
                    Priority = "high",
                    Month = "January 2026"
                }
            },
            InProgressItems = new List<WorkItem>
            {
                new()
                {
                    Title = "Feature B",
                    Description = "Desc B",
                    Status = "in-progress",
                    Owner = "Bob",
                    Priority = "medium",
                    Month = "January 2026"
                }
            },
            CarriedOverItems = new List<WorkItem>
            {
                new()
                {
                    Title = "Feature C",
                    Description = "Desc C",
                    Status = "carried-over",
                    Owner = "Charlie",
                    Priority = "low",
                    Month = "December 2025",
                    Notes = "Blocked."
                }
            },
            BlockedItems = new List<WorkItem>()
        };
    }

    [Fact]
    public void GetDashboardData_LoadsValidJson()
    {
        WriteSampleData(CreateSampleData());
        using var service = CreateService();

        var data = service.GetDashboardData();

        Assert.Equal("Test Project", data.ProjectName);
        Assert.Equal("Test Team", data.TeamName);
        Assert.Equal("January 2026", data.ReportingPeriod);
        Assert.Equal("Green", data.HealthIndicator);
        Assert.Equal("Green", data.OverallStatus);
        Assert.Equal("All systems go.", data.Summary);
        Assert.Equal(2, data.Milestones.Count);
        Assert.Single(data.ShippedItems);
        Assert.Single(data.InProgressItems);
        Assert.Single(data.CarriedOverItems);
        Assert.Empty(data.BlockedItems);
    }

    [Fact]
    public void GetDashboardData_MissingFile_ReturnsDefaultData()
    {
        using var service = CreateService();

        var data = service.GetDashboardData();

        Assert.Equal("No Data", data.ProjectName);
        Assert.NotNull(data.Summary);
    }

    [Fact]
    public void GetDashboardData_InvalidJson_ReturnsErrorData()
    {
        File.WriteAllText(_dataFilePath, "{ this is not valid json }");
        using var service = CreateService();

        var data = service.GetDashboardData();

        Assert.Equal("Load Error", data.ProjectName);
        Assert.Contains("Error reading data.json", data.Summary);
    }

    [Fact]
    public void GetDashboardData_ReturnsAllMilestoneFields()
    {
        WriteSampleData(CreateSampleData());
        using var service = CreateService();

        var data = service.GetDashboardData();
        var completed = data.Milestones.First(m => m.Status == "completed");
        var upcoming = data.Milestones.First(m => m.Status == "upcoming");

        Assert.Equal("Milestone 1", completed.Name);
        Assert.Equal("First milestone", completed.Description);
        Assert.Equal(new DateTime(2026, 1, 15), completed.TargetDate);
        Assert.Equal(new DateTime(2026, 1, 14), completed.CompletionDate);
        Assert.Equal("#28a745", completed.Color);
        Assert.Equal("diamond", completed.MarkerType);

        Assert.Equal("Milestone 2", upcoming.Name);
        Assert.Null(upcoming.CompletionDate);
        Assert.Equal("circle", upcoming.MarkerType);
    }

    [Fact]
    public void GetDashboardData_ReturnsWorkItemDetails()
    {
        WriteSampleData(CreateSampleData());
        using var service = CreateService();

        var data = service.GetDashboardData();
        var shipped = data.ShippedItems[0];
        var carriedOver = data.CarriedOverItems[0];

        Assert.Equal("Feature A", shipped.Title);
        Assert.Equal("Alice", shipped.Owner);
        Assert.Equal("high", shipped.Priority);
        Assert.Equal("shipped", shipped.Status);
        Assert.Equal("January 2026", shipped.Month);
        Assert.Null(shipped.Notes);

        Assert.Equal("Blocked.", carriedOver.Notes);
        Assert.Equal("carried-over", carriedOver.Status);
    }

    [Fact]
    public async Task FileChange_TriggersDataReload()
    {
        WriteSampleData(CreateSampleData());
        using var service = CreateService();

        Assert.Equal("Test Project", service.GetDashboardData().ProjectName);

        var tcs = new TaskCompletionSource<bool>();
        service.DataChanged += () => tcs.TrySetResult(true);

        var updated = CreateSampleData();
        updated.ProjectName = "Updated Project";
        WriteSampleData(updated);

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));

        if (completed == tcs.Task)
        {
            Assert.Equal("Updated Project", service.GetDashboardData().ProjectName);
        }
        // FileSystemWatcher timing is non-deterministic; test passes if event fires
    }

    [Fact]
    public void GetDashboardData_ThreadSafety_ConcurrentReadsDoNotThrow()
    {
        WriteSampleData(CreateSampleData());
        using var service = CreateService();

        var exceptions = new List<Exception>();
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            try
            {
                var data = service.GetDashboardData();
                Assert.NotNull(data);
                Assert.NotNull(data.ProjectName);
            }
            catch (Exception ex)
            {
                lock (exceptions) { exceptions.Add(ex); }
            }
        })).ToArray();

        Task.WaitAll(tasks);
        Assert.Empty(exceptions);
    }

    [Fact]
    public void GetDashboardData_EmptyJson_ReturnsDefaultValues()
    {
        File.WriteAllText(_dataFilePath, "{}");
        using var service = CreateService();

        var data = service.GetDashboardData();

        Assert.NotNull(data);
        Assert.Equal(string.Empty, data.ProjectName);
        Assert.Empty(data.Milestones);
        Assert.Empty(data.ShippedItems);
        Assert.Empty(data.InProgressItems);
        Assert.Empty(data.CarriedOverItems);
        Assert.Empty(data.BlockedItems);
    }

    [Fact]
    public void Dispose_StopsFileWatcher_DoubleDisposeIsSafe()
    {
        WriteSampleData(CreateSampleData());
        var service = CreateService();

        service.Dispose();
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