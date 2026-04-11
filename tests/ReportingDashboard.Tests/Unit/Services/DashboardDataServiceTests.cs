using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly Mock<ILogger<DashboardDataService>> _mockLogger;
    private readonly DashboardDataService _service;
    private readonly string _tempDir;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardTest_{Guid.NewGuid():N}");
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

    private static string GetValidJson() => """
    {
        "title": "Test Dashboard",
        "subtitle": "Team - April 2026",
        "backlogLink": "https://dev.azure.com/test",
        "currentMonth": "Apr",
        "months": ["Jan", "Feb", "Mar", "Apr"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "name": "Chatbot",
                    "label": "M1",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-02-15", "label": "Feb 15", "type": "poc" }
                    ]
                }
            ]
        },
        "heatmap": {
            "shipped": { "jan": ["Item A"] },
            "inProgress": { "mar": ["Item B"] },
            "carryover": {},
            "blockers": { "apr": ["Blocker 1"] }
        }
    }
    """;

    [Fact]
    public async Task LoadAsync_ValidJson_DataIsPopulated()
    {
        var path = WriteDataJson(GetValidJson());

        await _service.LoadAsync(path);

        _service.Data.Should().NotBeNull();
        _service.IsError.Should().BeFalse();
        _service.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_ValidJson_TitleIsCorrect()
    {
        var path = WriteDataJson(GetValidJson());

        await _service.LoadAsync(path);

        _service.Data!.Title.Should().Be("Test Dashboard");
    }

    [Fact]
    public async Task LoadAsync_ValidJson_MonthsArePopulated()
    {
        var path = WriteDataJson(GetValidJson());

        await _service.LoadAsync(path);

        _service.Data!.Months.Should().HaveCount(4);
        _service.Data.Months.Should().ContainInOrder("Jan", "Feb", "Mar", "Apr");
    }

    [Fact]
    public async Task LoadAsync_ValidJson_TimelineTracksArePopulated()
    {
        var path = WriteDataJson(GetValidJson());

        await _service.LoadAsync(path);

        _service.Data!.Timeline.Tracks.Should().HaveCount(1);
        _service.Data.Timeline.Tracks[0].Name.Should().Be("Chatbot");
    }

    [Fact]
    public async Task LoadAsync_ValidJson_HeatmapIsPopulated()
    {
        var path = WriteDataJson(GetValidJson());

        await _service.LoadAsync(path);

        _service.Data!.Heatmap.Shipped.Should().ContainKey("jan");
        _service.Data.Heatmap.Blockers.Should().ContainKey("apr");
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_SetsIsError()
    {
        var nonExistentPath = Path.Combine(_tempDir, "nonexistent.json");

        await _service.LoadAsync(nonExistentPath);

        _service.IsError.Should().BeTrue();
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_ErrorMessageContainsPath()
    {
        var nonExistentPath = Path.Combine(_tempDir, "nonexistent.json");

        await _service.LoadAsync(nonExistentPath);

        _service.ErrorMessage.Should().NotBeNullOrEmpty();
        _service.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_SetsIsError()
    {
        var path = WriteDataJson("{ invalid json }");

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_ErrorMessageContainsParseDetails()
    {
        var path = WriteDataJson("{ invalid json }");

        await _service.LoadAsync(path);

        _service.ErrorMessage.Should().Contain("parse");
    }

    [Fact]
    public async Task LoadAsync_TrailingComma_AcceptedByParser()
    {
        // PR #595 sets AllowTrailingCommas = true
        var json = """
        {
            "title": "Test Dashboard",
            "subtitle": "Team - April 2026",
            "backlogLink": "https://dev.azure.com/test",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr",],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    { "name": "Track", "label": "T1", "milestones": [],},
                ],
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {}, },
        }
        """;
        var path = WriteDataJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeFalse();
        _service.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_SetsIsError()
    {
        var path = WriteDataJson("");

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_NullJsonLiteral_SetsIsError()
    {
        var path = WriteDataJson("null");

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("null");
    }

    [Fact]
    public async Task LoadAsync_EmptyObjectJson_FailsValidation()
    {
        var path = WriteDataJson("{}");

        await _service.LoadAsync(path);

        // PR #595's Validate() requires non-empty title
        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("title");
    }

    [Fact]
    public async Task LoadAsync_MissingTitle_FailsValidation()
    {
        var json = """
        {
            "subtitle": "Team - April",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Apr"],
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
    public async Task LoadAsync_EmptyTitle_SetsValidationError()
    {
        var json = """
        {
            "title": "",
            "subtitle": "Sub",
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
    public async Task LoadAsync_NoTracks_SetsValidationError()
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
    public async Task LoadAsync_ValidThenInvalid_SecondCallOverwritesState()
    {
        var validPath = WriteDataJson(GetValidJson());
        await _service.LoadAsync(validPath);
        _service.IsError.Should().BeFalse();

        var invalidPath = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(invalidPath, "{ bad }");
        await _service.LoadAsync(invalidPath);

        // PR #595's SetError() clears Data
        _service.IsError.Should().BeTrue();
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_LogsError()
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

    [Fact]
    public async Task LoadAsync_ValidJson_LogsInformation()
    {
        var path = WriteDataJson(GetValidJson());

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
    public async Task LoadAsync_JsonArray_SetsIsError()
    {
        var path = WriteDataJson("[1, 2, 3]");

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_LargeDataSet_HandlesGracefully()
    {
        var items = string.Join(", ", Enumerable.Range(1, 100).Select(i => $"\"Item {i}\""));
        var json = $$"""
        {
            "title": "Large Set",
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
                "shipped": { "jan": [{{items}}] },
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        var path = WriteDataJson(json);

        await _service.LoadAsync(path);

        _service.Data.Should().NotBeNull();
        _service.Data!.Heatmap.Shipped["jan"].Should().HaveCount(100);
    }

    [Fact]
    public async Task LoadAsync_SpecialCharactersInData_DeserializesCorrectly()
    {
        var json = """
        {
            "title": "Dashboard with \"quotes\" & <html>",
            "subtitle": "Unicode: \u00e9\u00e8\u00ea",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "Track", "label": "T1", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteDataJson(json);

        await _service.LoadAsync(path);

        _service.Data.Should().NotBeNull();
        _service.Data!.Title.Should().Contain("quotes");
        _service.Data.Title.Should().Contain("&");
    }

    [Fact]
    public async Task LoadAsync_MultipleTracksWithMultipleMilestones_AllDeserialize()
    {
        var json = """
        {
            "title": "Multi Track",
            "subtitle": "Test Subtitle",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "name": "Track 1", "label": "M1", "color": "#0078D4",
                        "milestones": [
                            { "date": "2026-01-15", "label": "Jan 15", "type": "checkpoint" },
                            { "date": "2026-02-15", "label": "Feb 15", "type": "poc" },
                            { "date": "2026-03-15", "label": "Mar 15", "type": "production" }
                        ]
                    },
                    {
                        "name": "Track 2", "label": "M2", "color": "#00897B",
                        "milestones": [
                            { "date": "2026-04-01", "label": "Apr 1", "type": "production" }
                        ]
                    },
                    {
                        "name": "Track 3", "label": "M3", "color": "#546E7A",
                        "milestones": []
                    }
                ]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteDataJson(json);

        await _service.LoadAsync(path);

        _service.Data!.Timeline.Tracks.Should().HaveCount(3);
        _service.Data.Timeline.Tracks[0].Milestones.Should().HaveCount(3);
        _service.Data.Timeline.Tracks[1].Milestones.Should().HaveCount(1);
        _service.Data.Timeline.Tracks[2].Milestones.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_InitializesWithNoError()
    {
        var service = new DashboardDataService(_mockLogger.Object);

        service.Data.Should().BeNull();
        service.IsError.Should().BeFalse();
        service.ErrorMessage.Should().BeNull();
    }
}