using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _tempFile;
    private readonly Mock<ILogger<DashboardDataService>> _loggerMock;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _tempFile = Path.Combine(_tempDir, "dashboard-data.json");
        _loggerMock = new Mock<ILogger<DashboardDataService>>();
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private DashboardDataService CreateService(string? filePath = null)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DashboardDataFile"] = filePath ?? _tempFile
            })
            .Build();

        var envMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        envMock.Setup(e => e.ContentRootPath).Returns(_tempDir);

        return new DashboardDataService(config, _loggerMock.Object, envMock.Object);
    }

    private static string GetValidJson() => JsonSerializer.Serialize(new
    {
        project = new { title = "Test Project", subtitle = "Test Sub", backlogUrl = "https://example.com", currentDate = "2026-04-15" },
        timeline = new { startDate = "2026-01-01", endDate = "2026-08-01", tracks = new[] { new { id = "M1", name = "Track 1", color = "#0078D4", milestones = new[] { new { date = "2026-02-15", label = "Alpha", type = "checkpoint" } } } } },
        heatmap = new { months = new[] { "Jan", "Feb" }, highlightMonth = "Feb", rows = new[] { new { category = "Shipped", items = new Dictionary<string, string[]> { ["Jan"] = new[] { "Item A" } } } } }
    });

    [Fact]
    public void LoadsValidJson_ReturnsDeserializedData()
    {
        File.WriteAllText(_tempFile, GetValidJson());
        using var service = CreateService();

        var data = service.GetData();
        var error = service.GetError();

        data.Should().NotBeNull();
        error.Should().BeNull();
        data!.Project.Title.Should().Be("Test Project");
        data.Project.Subtitle.Should().Be("Test Sub");
        data.Timeline.Tracks.Should().HaveCount(1);
        data.Heatmap.Months.Should().HaveCount(2);
    }

    [Fact]
    public void MissingFile_ReturnsNullDataWithErrorMessage()
    {
        var nonExistentPath = Path.Combine(_tempDir, "nonexistent.json");
        using var service = CreateService(nonExistentPath);

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("Dashboard data file not found");
        service.GetError().Should().Contain("Expected location:");
    }

    [Fact]
    public void MalformedJson_ReturnsNullDataWithParseError()
    {
        File.WriteAllText(_tempFile, "{ invalid json,,, }");
        using var service = CreateService();

        service.GetData().Should().BeNull();
        service.GetError().Should().StartWith("Error reading dashboard data:");
    }

    [Fact]
    public void NullJsonContent_ReturnsErrorAboutEmptyFile()
    {
        File.WriteAllText(_tempFile, "null");
        using var service = CreateService();

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("empty or contains only 'null'");
    }

    [Fact]
    public void FiresOnDataChanged_WhenFileIsModified()
    {
        File.WriteAllText(_tempFile, GetValidJson());
        using var service = CreateService();

        var eventFired = new ManualResetEventSlim(false);
        service.OnDataChanged += () => eventFired.Set();

        File.WriteAllText(_tempFile, GetValidJson());

        var fired = eventFired.Wait(TimeSpan.FromSeconds(3));
        fired.Should().BeTrue("OnDataChanged should fire when the file is modified");
    }
}