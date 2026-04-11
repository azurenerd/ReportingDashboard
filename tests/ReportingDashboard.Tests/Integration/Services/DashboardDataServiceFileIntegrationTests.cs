using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Services;

[Trait("Category", "Integration")]
public class DashboardDataServiceFileIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _webRootPath;
    private readonly DashboardDataService _service;
    private readonly Mock<ILogger<DashboardDataService>> _mockLogger;

    public DashboardDataServiceFileIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardInteg_{Guid.NewGuid():N}");
        _webRootPath = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(_webRootPath);

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(_webRootPath);

        _mockLogger = new Mock<ILogger<DashboardDataService>>();
        _service = new DashboardDataService(mockEnv.Object, _mockLogger.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteDataJson(string content)
    {
        var path = Path.Combine(_webRootPath, "data.json");
        File.WriteAllText(path, content);
        return path;
    }

    private static string GetFullSampleJson() => """
    {
        "title": "Privacy Automation Release Roadmap",
        "subtitle": "Trusted Platform – Privacy Automation – April 2026",
        "backlogLink": "https://dev.azure.com/org/project/_backlogs",
        "currentMonth": "Apr",
        "months": ["Jan", "Feb", "Mar", "Apr"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "id": "M1",
                    "name": "Chatbot Integration",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-01-20", "label": "Jan 20", "type": "checkpoint" },
                        { "date": "2026-02-15", "label": "Feb 15", "type": "poc" },
                        { "date": "2026-04-01", "label": "Apr 1", "type": "production" }
                    ]
                },
                {
                    "id": "M2",
                    "name": "Data Pipeline",
                    "color": "#00897B",
                    "milestones": [
                        { "date": "2026-03-01", "label": "Mar 1", "type": "poc" },
                        { "date": "2026-05-15", "label": "May 15", "type": "production" }
                    ]
                },
                {
                    "id": "M3",
                    "name": "Compliance Engine",
                    "color": "#546E7A",
                    "milestones": [
                        { "date": "2026-02-01", "label": "Feb 1", "type": "checkpoint" },
                        { "date": "2026-06-01", "label": "Jun 1", "type": "production" }
                    ]
                }
            ]
        },
        "heatmap": {
            "shipped": {
                "jan": ["Privacy SDK v2.1", "Telemetry Pipeline"],
                "feb": ["Consent API v3", "Data Classification ML"],
                "mar": ["Auto-redaction Engine"]
            },
            "inProgress": {
                "mar": ["Cross-tenant Sync"],
                "apr": ["Real-time Monitoring", "Compliance Dashboard v2"]
            },
            "carryover": {
                "apr": ["Legacy Migration Script"]
            },
            "blockers": {
                "apr": ["Pending Legal Review on EU Data"]
            }
        }
    }
    """;

    [Fact]
    public async Task LoadAsync_FullSampleData_DeserializesAllTopLevelFields()
    {
        WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        _service.Data.Should().NotBeNull();
        _service.Data!.Title.Should().Be("Privacy Automation Release Roadmap");
        _service.Data.Subtitle.Should().Contain("Trusted Platform");
        _service.Data.BacklogLink.Should().StartWith("https://");
        _service.Data.CurrentMonth.Should().Be("Apr");
    }

    [Fact]
    public async Task LoadAsync_FullSampleData_MonthsDeserializeAsList()
    {
        WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync();

        _service.Data!.Months.Should().HaveCount(4);
        _service.Data.Months.Should().ContainInOrder("Jan", "Feb", "Mar", "Apr");
    }

    [Fact]
    public async Task LoadAsync_FullSampleData_TimelineHasThreeTracks()
    {
        WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync();

        var timeline = _service.Data!.Timeline;
        timeline.StartDate.Should().Be("2026-01-01");
        timeline.EndDate.Should().Be("2026-06-30");
        timeline.NowDate.Should().Be("2026-04-10");
        timeline.Tracks.Should().HaveCount(3);
    }

    [Fact]
    public async Task LoadAsync_FullSampleData_TrackIdsAndColorsCorrect()
    {
        WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync();

        var tracks = _service.Data!.Timeline.Tracks;
        tracks[0].Id.Should().Be("M1");
        tracks[0].Color.Should().Be("#0078D4");
        tracks[1].Id.Should().Be("M2");
        tracks[1].Color.Should().Be("#00897B");
        tracks[2].Id.Should().Be("M3");
        tracks[2].Color.Should().Be("#546E7A");
    }

    [Fact]
    public async Task LoadAsync_FullSampleData_MilestonesHaveAllThreeTypes()
    {
        WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync();

        var allMilestones = _service.Data!.Timeline.Tracks
            .SelectMany(t => t.Milestones)
            .ToList();

        allMilestones.Should().Contain(m => m.Type == "checkpoint");
        allMilestones.Should().Contain(m => m.Type == "poc");
        allMilestones.Should().Contain(m => m.Type == "production");
    }

    [Fact]
    public async Task LoadAsync_FullSampleData_HeatmapShippedHasMultipleMonths()
    {
        WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync();

        var shipped = _service.Data!.Heatmap.Shipped;
        shipped.Should().ContainKey("jan");
        shipped.Should().ContainKey("feb");
        shipped.Should().ContainKey("mar");
        shipped["jan"].Should().HaveCount(2);
        shipped["feb"].Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadAsync_FullSampleData_HeatmapInProgressCorrect()
    {
        WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync();

        var inProgress = _service.Data!.Heatmap.InProgress;
        inProgress.Should().ContainKey("mar");
        inProgress.Should().ContainKey("apr");
        inProgress["apr"].Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadAsync_FullSampleData_HeatmapCarryoverAndBlockers()
    {
        WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync();

        _service.Data!.Heatmap.Carryover.Should().ContainKey("apr");
        _service.Data.Heatmap.Carryover["apr"].Should().ContainSingle()
            .Which.Should().Contain("Legacy");

        _service.Data.Heatmap.Blockers.Should().ContainKey("apr");
        _service.Data.Heatmap.Blockers["apr"].Should().ContainSingle()
            .Which.Should().Contain("Legal Review");
    }

    [Fact]
    public async Task LoadAsync_FullSampleData_MissingHeatmapKeyReturnsNoEntry()
    {
        WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync();

        _service.Data!.Heatmap.Shipped.ContainsKey("may").Should().BeFalse();
        _service.Data.Heatmap.Blockers.ContainsKey("jan").Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_MissingFile_DefaultPath_SetsIsErrorWithPath()
    {
        // Don't create any file
        await _service.LoadAsync();

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("not found");
        _service.ErrorMessage.Should().Contain("data.json");
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_SetsIsErrorWithParseDetails()
    {
        WriteDataJson("{ \"title\": \"Test\", }"); // trailing comma

        await _service.LoadAsync();

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("parse");
    }

    [Fact]
    public async Task LoadAsync_EmptyJsonObject_LoadsWithDefaults()
    {
        WriteDataJson("{}");

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        _service.Data.Should().NotBeNull();
        _service.Data!.Title.Should().BeEmpty();
        _service.Data.Months.Should().BeEmpty();
        _service.Data.Timeline.Tracks.Should().BeEmpty();
        _service.Data.Heatmap.Shipped.Should().BeEmpty();
        _service.Data.Heatmap.InProgress.Should().BeEmpty();
        _service.Data.Heatmap.Carryover.Should().BeEmpty();
        _service.Data.Heatmap.Blockers.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_NullJson_SetsIsError()
    {
        WriteDataJson("null");

        await _service.LoadAsync();

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("empty or could not be parsed");
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_SetsIsError()
    {
        WriteDataJson("");

        await _service.LoadAsync();

        _service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_ValidThenReload_SecondCallOverwritesState()
    {
        WriteDataJson(GetFullSampleJson());
        await _service.LoadAsync();
        _service.Data!.Title.Should().Be("Privacy Automation Release Roadmap");

        // Overwrite with different data
        WriteDataJson("""{ "title": "Updated Dashboard" }""");
        await _service.LoadAsync();

        _service.Data!.Title.Should().Be("Updated Dashboard");
        _service.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_ValidThenCorrupt_SecondCallSetsError()
    {
        WriteDataJson(GetFullSampleJson());
        await _service.LoadAsync();
        _service.IsError.Should().BeFalse();

        WriteDataJson("not json at all");
        await _service.LoadAsync();

        _service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_ExplicitPath_OverridesDefaultWebRoot()
    {
        var customDir = Path.Combine(_tempDir, "custom");
        Directory.CreateDirectory(customDir);
        var customPath = Path.Combine(customDir, "mydata.json");
        File.WriteAllText(customPath, """{ "title": "Custom Path Data" }""");

        await _service.LoadAsync(customPath);

        _service.IsError.Should().BeFalse();
        _service.Data!.Title.Should().Be("Custom Path Data");
    }

    [Fact]
    public async Task LoadAsync_WithEmptyArraysInHeatmap_NoNullReferenceException()
    {
        var json = """
        {
            "title": "Empty Arrays Test",
            "heatmap": {
                "shipped": { "jan": [], "feb": [] },
                "inProgress": {},
                "carryover": { "mar": [] },
                "blockers": {}
            }
        }
        """;
        WriteDataJson(json);

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        _service.Data!.Heatmap.Shipped["jan"].Should().BeEmpty();
        _service.Data.Heatmap.Shipped["feb"].Should().BeEmpty();
        _service.Data.Heatmap.Carryover["mar"].Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_MissingTitle_LogsWarningButLoadsSuccessfully()
    {
        WriteDataJson("""{ "subtitle": "No title here" }""");

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        _service.Data.Should().NotBeNull();
        _service.Data!.Title.Should().BeEmpty();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("title")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_EmptyTracks_LogsWarningButLoadsSuccessfully()
    {
        var json = """
        {
            "title": "Test",
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-06-30", "nowDate": "2026-04-01", "tracks": [] }
        }
        """;
        WriteDataJson(json);

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("tracks")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_ValidJson_LogsSuccessMessage()
    {
        WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("successfully")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_LogsErrorMessage()
    {
        await _service.LoadAsync();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}