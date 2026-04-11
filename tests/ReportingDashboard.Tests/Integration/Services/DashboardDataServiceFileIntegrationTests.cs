using FluentAssertions;
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
    private readonly DashboardDataService _service;
    private readonly Mock<ILogger<DashboardDataService>> _mockLogger;

    public DashboardDataServiceFileIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardInteg_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _mockLogger = new Mock<ILogger<DashboardDataService>>();
        _service = new DashboardDataService(_mockLogger.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteDataJson(string content)
    {
        var path = Path.Combine(_tempDir, "data.json");
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
                    "name": "Chatbot Integration",
                    "label": "M1",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-01-20", "label": "Jan 20", "type": "checkpoint" },
                        { "date": "2026-02-15", "label": "Feb 15", "type": "poc" },
                        { "date": "2026-04-01", "label": "Apr 1", "type": "production" }
                    ]
                },
                {
                    "name": "Data Pipeline",
                    "label": "M2",
                    "color": "#00897B",
                    "milestones": [
                        { "date": "2026-03-01", "label": "Mar 1", "type": "poc" },
                        { "date": "2026-05-15", "label": "May 15", "type": "production" }
                    ]
                },
                {
                    "name": "Compliance Engine",
                    "label": "M3",
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
        var path = WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync(path);

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
        var path = WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync(path);

        _service.Data!.Months.Should().HaveCount(4);
        _service.Data.Months.Should().ContainInOrder("Jan", "Feb", "Mar", "Apr");
    }

    [Fact]
    public async Task LoadAsync_FullSampleData_TimelineHasThreeTracks()
    {
        var path = WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync(path);

        var timeline = _service.Data!.Timeline;
        timeline.StartDate.Should().Be("2026-01-01");
        timeline.EndDate.Should().Be("2026-06-30");
        timeline.NowDate.Should().Be("2026-04-10");
        timeline.Tracks.Should().HaveCount(3);
    }

    [Fact]
    public async Task LoadAsync_FullSampleData_TrackNamesAndColorsCorrect()
    {
        var path = WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync(path);

        var tracks = _service.Data!.Timeline.Tracks;
        tracks[0].Label.Should().Be("M1");
        tracks[0].Color.Should().Be("#0078D4");
        tracks[1].Label.Should().Be("M2");
        tracks[1].Color.Should().Be("#00897B");
        tracks[2].Label.Should().Be("M3");
        tracks[2].Color.Should().Be("#546E7A");
    }

    [Fact]
    public async Task LoadAsync_FullSampleData_MilestonesHaveAllThreeTypes()
    {
        var path = WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync(path);

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
        var path = WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync(path);

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
        var path = WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync(path);

        var inProgress = _service.Data!.Heatmap.InProgress;
        inProgress.Should().ContainKey("mar");
        inProgress.Should().ContainKey("apr");
        inProgress["apr"].Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadAsync_FullSampleData_HeatmapCarryoverAndBlockers()
    {
        var path = WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync(path);

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
        var path = WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync(path);

        _service.Data!.Heatmap.Shipped.ContainsKey("may").Should().BeFalse();
        _service.Data.Heatmap.Blockers.ContainsKey("jan").Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_MissingFile_SetsIsErrorWithPath()
    {
        var nonExistentPath = Path.Combine(_tempDir, "data.json");

        await _service.LoadAsync(nonExistentPath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("not found");
        _service.ErrorMessage.Should().Contain("data.json");
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_SetsIsErrorWithParseDetails()
    {
        var path = WriteDataJson("{ invalid json }");

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("parse");
    }

    [Fact]
    public async Task LoadAsync_EmptyObjectJson_FailsValidation()
    {
        // PR #595's Validate() requires non-empty title
        var path = WriteDataJson("{}");

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("title");
    }

    [Fact]
    public async Task LoadAsync_NullJson_SetsIsError()
    {
        var path = WriteDataJson("null");

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("null");
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_SetsIsError()
    {
        var path = WriteDataJson("");

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_ValidThenReload_SecondCallOverwritesState()
    {
        var path = WriteDataJson(GetFullSampleJson());
        await _service.LoadAsync(path);
        _service.Data!.Title.Should().Be("Privacy Automation Release Roadmap");

        var updatedJson = GetFullSampleJson().Replace(
            "Privacy Automation Release Roadmap",
            "Updated Dashboard");
        var updatedPath = Path.Combine(_tempDir, "updated.json");
        File.WriteAllText(updatedPath, updatedJson);
        await _service.LoadAsync(updatedPath);

        _service.Data!.Title.Should().Be("Updated Dashboard");
        _service.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_ValidThenCorrupt_SecondCallSetsError()
    {
        var path = WriteDataJson(GetFullSampleJson());
        await _service.LoadAsync(path);
        _service.IsError.Should().BeFalse();

        var corruptPath = Path.Combine(_tempDir, "corrupt.json");
        File.WriteAllText(corruptPath, "not json at all");
        await _service.LoadAsync(corruptPath);

        _service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_ExplicitPath_LoadsFromSpecifiedLocation()
    {
        var customDir = Path.Combine(_tempDir, "custom");
        Directory.CreateDirectory(customDir);
        var customPath = Path.Combine(customDir, "mydata.json");
        File.WriteAllText(customPath, GetFullSampleJson().Replace(
            "Privacy Automation Release Roadmap",
            "Custom Path Data"));

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
            "subtitle": "Test Subtitle",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "Track", "label": "T1", "milestones": [] }]
            },
            "heatmap": {
                "shipped": { "jan": [], "feb": [] },
                "inProgress": {},
                "carryover": { "mar": [] },
                "blockers": {}
            }
        }
        """;
        var path = WriteDataJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeFalse();
        _service.Data!.Heatmap.Shipped["jan"].Should().BeEmpty();
        _service.Data.Heatmap.Shipped["feb"].Should().BeEmpty();
        _service.Data.Heatmap.Carryover["mar"].Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_MissingTitle_SetsValidationError()
    {
        var json = """
        {
            "subtitle": "No title here",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-01",
                "tracks": [{ "name": "T", "label": "T1", "milestones": [] }]
            }
        }
        """;
        var path = WriteDataJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("title");
    }

    [Fact]
    public async Task LoadAsync_EmptyTracks_SetsValidationError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-01",
                "tracks": []
            }
        }
        """;
        var path = WriteDataJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("tracks");
    }

    [Fact]
    public async Task LoadAsync_ValidJson_LogsSuccessMessage()
    {
        var path = WriteDataJson(GetFullSampleJson());

        await _service.LoadAsync(path);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_LogsErrorMessage()
    {
        var nonExistentPath = Path.Combine(_tempDir, "missing.json");

        await _service.LoadAsync(nonExistentPath);

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