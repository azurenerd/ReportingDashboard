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
    private readonly Mock<ILogger<DashboardDataService>> _loggerMock;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "DashboardTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _loggerMock = new Mock<ILogger<DashboardDataService>>();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch { }
    }

    private DashboardDataService CreateService(string filePath)
    {
        var configData = new Dictionary<string, string?>
        {
            { "DashboardDataFile", filePath }
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var envMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        envMock.Setup(e => e.ContentRootPath).Returns(_tempDir);

        return new DashboardDataService(config, envMock.Object, _loggerMock.Object);
    }

    private string WriteSampleJson(string fileName = "dashboard-data.json")
    {
        var filePath = Path.Combine(_tempDir, fileName);
        var json = GetValidSampleJson();
        File.WriteAllText(filePath, json);
        return filePath;
    }

    private static string GetValidSampleJson() => """
    {
        "project": {
            "title": "Test Project",
            "subtitle": "Test Subtitle",
            "backlogUrl": "https://dev.azure.com/test",
            "currentDate": "2026-04-15"
        },
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-07-31",
            "tracks": [
                {
                    "id": "M1",
                    "name": "Track One",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-02-15", "label": "Feb Checkpoint", "type": "checkpoint" },
                        { "date": "2026-04-01", "label": "PoC Ready", "type": "poc" }
                    ]
                }
            ]
        },
        "heatmap": {
            "months": ["January", "February", "March", "April"],
            "highlightMonth": "April",
            "rows": [
                {
                    "category": "Shipped",
                    "items": {
                        "January": ["Feature A"],
                        "February": ["Feature B", "Feature C"],
                        "March": [],
                        "April": ["Feature D"]
                    }
                },
                {
                    "category": "In Progress",
                    "items": {
                        "April": ["Feature E"]
                    }
                }
            ]
        }
    }
    """;

    [Fact]
    public void ValidJson_LoadsDataCorrectly()
    {
        var filePath = WriteSampleJson();
        using var service = CreateService(filePath);

        var data = service.GetData();
        var error = service.GetError();

        data.Should().NotBeNull();
        error.Should().BeNull();
        data!.Project.Title.Should().Be("Test Project");
        data.Timeline.Tracks.Should().HaveCount(1);
        data.Heatmap.Months.Should().HaveCount(4);
    }

    [Fact]
    public void MissingFile_ReturnsNullDataWithError()
    {
        var filePath = Path.Combine(_tempDir, "nonexistent.json");
        using var service = CreateService(filePath);

        var data = service.GetData();
        var error = service.GetError();

        data.Should().BeNull();
        error.Should().NotBeNull();
        error.Should().Contain("not found");
        error.Should().Contain(filePath);
    }

    [Fact]
    public void MalformedJson_ReturnsNullDataWithParseError()
    {
        var filePath = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(filePath, "{ this is not valid json }}}");
        using var service = CreateService(filePath);

        var data = service.GetData();
        var error = service.GetError();

        data.Should().BeNull();
        error.Should().NotBeNull();
        error.Should().Contain("Error reading dashboard data");
    }

    [Fact]
    public void FileChange_TriggersOnDataChangedEvent()
    {
        var filePath = WriteSampleJson();
        using var service = CreateService(filePath);

        var eventFired = new ManualResetEventSlim(false);
        service.OnDataChanged += () => eventFired.Set();

        // Modify the file to trigger watcher/polling
        Thread.Sleep(500); // ensure timestamp differs
        File.WriteAllText(filePath, GetValidSampleJson().Replace("Test Project", "Updated Project"));

        var fired = eventFired.Wait(TimeSpan.FromSeconds(8));
        fired.Should().BeTrue("OnDataChanged should fire after file modification");
        service.GetData()!.Project.Title.Should().Be("Updated Project");
    }

    [Fact]
    public void RecoveryAfterError_ReloadsValidData()
    {
        var filePath = Path.Combine(_tempDir, "recover.json");
        File.WriteAllText(filePath, "INVALID JSON!!!");
        using var service = CreateService(filePath);

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("Error reading dashboard data");

        var eventFired = new ManualResetEventSlim(false);
        service.OnDataChanged += () => eventFired.Set();

        Thread.Sleep(500);
        File.WriteAllText(filePath, GetValidSampleJson());

        var fired = eventFired.Wait(TimeSpan.FromSeconds(8));
        fired.Should().BeTrue();
        service.GetData().Should().NotBeNull();
        service.GetError().Should().BeNull();
    }
}