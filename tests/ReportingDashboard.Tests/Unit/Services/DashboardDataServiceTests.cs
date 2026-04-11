using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dataFilePath;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashDataSvcTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _dataFilePath = Path.Combine("data", "data.json");
        Directory.CreateDirectory(Path.Combine(_tempDir, "data"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private DashboardDataService CreateService(string? dataFilePath = null)
    {
        var env = new TestWebHostEnvironment { ContentRootPath = _tempDir };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Dashboard:DataFilePath"] = dataFilePath ?? _dataFilePath
            })
            .Build();
        var logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
        return new DashboardDataService(env, config, logger);
    }

    private void WriteDataJson(object data)
    {
        var fullPath = Path.Combine(_tempDir, _dataFilePath);
        var dir = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        File.WriteAllText(fullPath, json);
    }

    private void WriteRawJson(string json)
    {
        var fullPath = Path.Combine(_tempDir, _dataFilePath);
        File.WriteAllText(fullPath, json);
    }

    [Fact]
    public async Task LoadDataAsync_ValidJson_DeserializesAllFields()
    {
        WriteDataJson(new
        {
            project = new { name = "Test Project", lead = "Alice", status = "On Track", lastUpdated = "2026-04-01", summary = "A test" },
            milestones = new[] { new { title = "M1", targetDate = "2026-05-01", status = "Completed" } },
            shipped = new[] { new { title = "Feature A", description = "Done", category = "Core", percentComplete = 100 } },
            inProgress = new[] { new { title = "Feature B", percentComplete = 50 } },
            carriedOver = new[] { new { title = "Feature C", carryOverReason = "Delayed" } },
            currentMonth = new { month = "April", totalItems = 10, completedItems = 7, carriedItems = 2, overallHealth = "On Track" }
        });

        var svc = CreateService();
        var data = await svc.LoadDataAsync();

        data.ErrorMessage.Should().BeNull();
        data.Project.Should().NotBeNull();
        data.Project!.Name.Should().Be("Test Project");
        data.Project.Lead.Should().Be("Alice");
        data.Project.Status.Should().Be("On Track");
        data.Milestones.Should().HaveCount(1);
        data.Milestones[0].Title.Should().Be("M1");
        data.Shipped.Should().HaveCount(1);
        data.Shipped[0].Title.Should().Be("Feature A");
        data.InProgress.Should().HaveCount(1);
        data.CarriedOver.Should().HaveCount(1);
        data.CurrentMonth.Should().NotBeNull();
        data.CurrentMonth!.Month.Should().Be("April");
        data.CurrentMonth.TotalItems.Should().Be(10);
    }

    [Fact]
    public async Task LoadDataAsync_FileNotFound_SetsIsErrorAndErrorMessage()
    {
        var svc = CreateService("nonexistent/path.json");

        var data = await svc.LoadDataAsync();

        data.ErrorMessage.Should().NotBeNullOrEmpty();
        data.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task LoadDataAsync_MalformedJson_SetsErrorWithDetails()
    {
        WriteRawJson("{ invalid json content !!! }");

        var svc = CreateService();
        var data = await svc.LoadDataAsync();

        data.ErrorMessage.Should().NotBeNullOrEmpty();
        data.ErrorMessage.Should().Contain("Invalid JSON");
    }

    [Fact]
    public async Task LoadDataAsync_NullDeserialization_SetsError()
    {
        WriteRawJson("null");

        var svc = CreateService();
        var data = await svc.LoadDataAsync();

        data.ErrorMessage.Should().NotBeNullOrEmpty();
        data.ErrorMessage.Should().Contain("empty");
    }

    [Fact]
    public async Task LoadDataAsync_EmptyObject_CollectionsAreNotNull()
    {
        WriteRawJson("{}");

        var svc = CreateService();
        var data = await svc.LoadDataAsync();

        data.Milestones.Should().NotBeNull().And.BeEmpty();
        data.Shipped.Should().NotBeNull().And.BeEmpty();
        data.InProgress.Should().NotBeNull().And.BeEmpty();
        data.CarriedOver.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task LoadDataAsync_ConsecutiveLoads_ReflectUpdatedFile()
    {
        WriteDataJson(new { project = new { name = "V1" } });
        var svc = CreateService();

        var data1 = await svc.LoadDataAsync();
        data1.Project!.Name.Should().Be("V1");

        WriteDataJson(new { project = new { name = "V2" } });

        var data2 = await svc.LoadDataAsync();
        data2.Project!.Name.Should().Be("V2");
    }

    [Fact]
    public async Task LoadDataAsync_MultipleListItems_AllDeserialized()
    {
        WriteDataJson(new
        {
            milestones = new[]
            {
                new { title = "Alpha", targetDate = "2026-01-01", status = "Completed" },
                new { title = "Beta", targetDate = "2026-03-01", status = "In Progress" }
            },
            shipped = new[]
            {
                new { title = "S1", percentComplete = 100 },
                new { title = "S2", percentComplete = 100 }
            }
        });

        var svc = CreateService();
        var data = await svc.LoadDataAsync();

        data.Milestones.Should().HaveCount(2);
        data.Shipped.Should().HaveCount(2);
    }

    [Fact]
    public void GetFullPath_ReturnsCombinedContentRootAndDataPath()
    {
        var svc = CreateService();
        var fullPath = svc.GetFullPath();

        fullPath.Should().Be(Path.Combine(_tempDir, _dataFilePath));
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ApplicationName { get; set; } = "ReportingDashboard";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; } = "";
        public string EnvironmentName { get; set; } = "Development";
    }
}