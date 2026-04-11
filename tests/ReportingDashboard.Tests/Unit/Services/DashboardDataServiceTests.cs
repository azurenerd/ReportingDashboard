using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly Mock<ILogger<DashboardDataService>> _loggerMock;
    private readonly DashboardDataService _service;
    private readonly string _tempDir;

    public DashboardDataServiceTests()
    {
        _loggerMock = new Mock<ILogger<DashboardDataService>>();
        _service = new DashboardDataService(_loggerMock.Object);
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private string WriteTempJson(string json)
    {
        var filePath = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(filePath, json);
        return filePath;
    }

    private static string ValidJson => """
    {
        "title": "Test Dashboard",
        "subtitle": "Test Team - April 2026",
        "backlogLink": "https://dev.azure.com/test",
        "currentMonth": "Apr",
        "months": ["Jan", "Feb", "Mar", "Apr"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "name": "M1",
                    "label": "Feature A",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-03-01", "type": "poc", "label": "PoC" }
                    ]
                }
            ]
        },
        "heatmap": {
            "shipped": { "jan": ["Item A"] },
            "inProgress": { "feb": ["Item B"] },
            "carryover": {},
            "blockers": {}
        }
    }
    """;

    [Fact]
    public void Constructor_InitializesWithNoError()
    {
        _service.Data.Should().BeNull();
        _service.IsError.Should().BeFalse();
        _service.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_ValidJson_LoadsSuccessfully()
    {
        var filePath = WriteTempJson(ValidJson);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeFalse();
        _service.ErrorMessage.Should().BeNull();
        _service.Data.Should().NotBeNull();
        _service.Data!.Title.Should().Be("Test Dashboard");
    }

    [Fact]
    public async Task LoadAsync_ValidJson_PopulatesAllFields()
    {
        var filePath = WriteTempJson(ValidJson);

        await _service.LoadAsync(filePath);

        _service.Data!.Subtitle.Should().Be("Test Team - April 2026");
        _service.Data.BacklogLink.Should().Be("https://dev.azure.com/test");
        _service.Data.CurrentMonth.Should().Be("Apr");
        _service.Data.Months.Should().HaveCount(4);
        _service.Data.Timeline.Should().NotBeNull();
        _service.Data.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadAsync_ValidJson_PopulatesTimeline()
    {
        var filePath = WriteTempJson(ValidJson);

        await _service.LoadAsync(filePath);

        _service.Data!.Timeline!.StartDate.Should().Be("2026-01-01");
        _service.Data.Timeline.EndDate.Should().Be("2026-06-30");
        _service.Data.Timeline.NowDate.Should().Be("2026-04-10");
        _service.Data.Timeline.Tracks.Should().HaveCount(1);
        _service.Data.Timeline.Tracks[0].Milestones.Should().HaveCount(1);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_SetsError()
    {
        var fakePath = Path.Combine(_tempDir, "nonexistent.json");

        await _service.LoadAsync(fakePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("not found");
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_LogsError()
    {
        var fakePath = Path.Combine(_tempDir, "nonexistent.json");

        await _service.LoadAsync(fakePath);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_SetsParseError()
    {
        var filePath = WriteTempJson("{ invalid json }}}}");

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("Failed to parse data.json");
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_EmptyJsonObject_SetsValidationError()
    {
        var filePath = WriteTempJson("{}");

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("validation");
        _service.ErrorMessage.Should().Contain("title is required");
    }

    [Fact]
    public async Task LoadAsync_MissingTitle_SetsValidationError()
    {
        var json = """
        {
            "subtitle": "Test",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("title is required");
    }

    [Fact]
    public async Task LoadAsync_MissingSubtitle_SetsValidationError()
    {
        var json = """
        {
            "title": "Test",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("subtitle is required");
    }

    [Fact]
    public async Task LoadAsync_MissingBacklogLink_SetsValidationError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("backlogLink is required");
    }

    [Fact]
    public async Task LoadAsync_EmptyMonths_SetsValidationError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": [],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("months is required");
    }

    [Fact]
    public async Task LoadAsync_MissingTimelineStartDate_SetsValidationError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "timeline": {
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("timeline.startDate is required");
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
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": []
            }
        }
        """;
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("timeline.tracks is required");
    }

    [Fact]
    public async Task LoadAsync_MultipleValidationErrors_ReportsAll()
    {
        var json = """
        {
            "currentMonth": "Apr",
            "months": [],
            "timeline": {
                "tracks": []
            }
        }
        """;
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("title is required");
        _service.ErrorMessage.Should().Contain("subtitle is required");
        _service.ErrorMessage.Should().Contain("backlogLink is required");
        _service.ErrorMessage.Should().Contain("months is required");
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_SetsParseError()
    {
        var filePath = WriteTempJson("");

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_NullDeserialization_SetsError()
    {
        var filePath = WriteTempJson("null");

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("could not be loaded");
    }

    [Fact]
    public async Task LoadAsync_ResetsStateBetweenCalls()
    {
        // First call: fail with missing file
        await _service.LoadAsync(Path.Combine(_tempDir, "nope.json"));
        _service.IsError.Should().BeTrue();

        // Second call: succeed
        var filePath = WriteTempJson(ValidJson);
        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeFalse();
        _service.ErrorMessage.Should().BeNull();
        _service.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadAsync_ResetsDataOnSubsequentFailure()
    {
        // First call: succeed
        var filePath = WriteTempJson(ValidJson);
        await _service.LoadAsync(filePath);
        _service.Data.Should().NotBeNull();

        // Second call: fail
        await _service.LoadAsync(Path.Combine(_tempDir, "missing.json"));

        _service.IsError.Should().BeTrue();
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_ValidJson_LogsSuccess()
    {
        var filePath = WriteTempJson(ValidJson);

        await _service.LoadAsync(filePath);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("loaded successfully")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_LogsParseError()
    {
        var filePath = WriteTempJson("{bad}");

        await _service.LoadAsync(filePath);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to parse")),
                It.IsAny<JsonException?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_WhitespaceOnlyTitle_SetsValidationError()
    {
        var json = """
        {
            "title": "   ",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("title is required");
    }

    [Fact]
    public async Task LoadAsync_JsonArray_SetsError()
    {
        var filePath = WriteTempJson("[1, 2, 3]");

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_ValidData_HeatmapIsPopulated()
    {
        var filePath = WriteTempJson(ValidJson);

        await _service.LoadAsync(filePath);

        _service.Data!.Heatmap.Should().NotBeNull();
        _service.Data.Heatmap!.Shipped.Should().ContainKey("jan");
    }

    [Fact]
    public async Task LoadAsync_ErrorMessageContainsFilePath_ForMissingFile()
    {
        var fakePath = Path.Combine(_tempDir, "specific_missing.json");

        await _service.LoadAsync(fakePath);

        _service.ErrorMessage.Should().Contain(fakePath);
    }

    [Fact]
    public async Task LoadAsync_MissingTimelineEndDate_SetsValidationError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("timeline.endDate is required");
    }

    [Fact]
    public async Task LoadAsync_MissingTimelineNowDate_SetsValidationError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("timeline.nowDate is required");
    }
}