using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly Mock<ILogger<DashboardDataService>> _loggerMock;
    private readonly DashboardDataService _service;
    private readonly string _tempDir;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_tempDir);

        _loggerMock = new Mock<ILogger<DashboardDataService>>();
        _service = new DashboardDataService(_envMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private async Task WriteDataJsonAsync(string content)
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "data.json"), content);
    }

    private static string CreateValidJson(string title = "Test Dashboard") => $$"""
    {
        "title": "{{title}}",
        "subtitle": "Test Team - Q1",
        "backlogLink": "https://ado.example.com",
        "currentMonth": "Apr",
        "months": ["Jan", "Feb", "Mar", "Apr"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "id": "M1",
                    "name": "Platform",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-03-01", "label": "Mar 1", "type": "poc" }
                    ]
                }
            ]
        },
        "heatmap": {
            "shipped": { "Jan": ["Item A"] },
            "inProgress": { "Feb": ["Item B"] },
            "carryover": {},
            "blockers": {}
        }
    }
    """;

    // --- Happy Path Tests ---

    [Fact]
    public async Task LoadAsync_ValidJson_ShouldPopulateData()
    {
        await WriteDataJsonAsync(CreateValidJson());

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        _service.ErrorMessage.Should().BeNull();
        _service.Data.Should().NotBeNull();
        _service.Data!.Title.Should().Be("Test Dashboard");
    }

    [Fact]
    public async Task LoadAsync_ValidJson_ShouldParseTimeline()
    {
        await WriteDataJsonAsync(CreateValidJson());

        await _service.LoadAsync();

        _service.Data!.Timeline.Should().NotBeNull();
        _service.Data.Timeline.StartDate.Should().Be("2026-01-01");
        _service.Data.Timeline.EndDate.Should().Be("2026-06-30");
        _service.Data.Timeline.NowDate.Should().Be("2026-04-10");
        _service.Data.Timeline.Tracks.Should().HaveCount(1);
    }

    [Fact]
    public async Task LoadAsync_ValidJson_ShouldParseHeatmap()
    {
        await WriteDataJsonAsync(CreateValidJson());

        await _service.LoadAsync();

        _service.Data!.Heatmap.Should().NotBeNull();
        _service.Data.Heatmap.Shipped.Should().ContainKey("Jan");
        _service.Data.Heatmap.InProgress.Should().ContainKey("Feb");
    }

    [Fact]
    public async Task LoadAsync_ValidJson_ShouldParseMonths()
    {
        await WriteDataJsonAsync(CreateValidJson());

        await _service.LoadAsync();

        _service.Data!.Months.Should().BeEquivalentTo(new[] { "Jan", "Feb", "Mar", "Apr" });
    }

    [Fact]
    public async Task LoadAsync_ValidJson_ShouldParseMilestones()
    {
        await WriteDataJsonAsync(CreateValidJson());

        await _service.LoadAsync();

        var track = _service.Data!.Timeline.Tracks[0];
        track.Milestones.Should().HaveCount(1);
        track.Milestones[0].Date.Should().Be("2026-03-01");
        track.Milestones[0].Label.Should().Be("Mar 1");
        track.Milestones[0].Type.Should().Be("poc");
    }

    // --- Custom File Path Tests ---

    [Fact]
    public async Task LoadAsync_CustomFilePath_ShouldLoadFromSpecifiedPath()
    {
        var customPath = Path.Combine(_tempDir, "custom", "dashboard.json");
        Directory.CreateDirectory(Path.GetDirectoryName(customPath)!);
        await File.WriteAllTextAsync(customPath, CreateValidJson("Custom Path"));

        await _service.LoadAsync(customPath);

        _service.IsError.Should().BeFalse();
        _service.Data!.Title.Should().Be("Custom Path");
    }

    [Fact]
    public async Task LoadAsync_NullFilePath_ShouldUseDefaultWebRootPath()
    {
        await WriteDataJsonAsync(CreateValidJson("Default Path"));

        await _service.LoadAsync(null);

        _service.IsError.Should().BeFalse();
        _service.Data!.Title.Should().Be("Default Path");
    }

    // --- File Not Found Tests ---

    [Fact]
    public async Task LoadAsync_FileNotFound_ShouldSetError()
    {
        await _service.LoadAsync();

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("not found");
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_ShouldLogError()
    {
        await _service.LoadAsync();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_NonExistentCustomPath_ShouldSetError()
    {
        await _service.LoadAsync("/nonexistent/path/data.json");

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("not found");
    }

    // --- Invalid JSON Tests ---

    [Fact]
    public async Task LoadAsync_InvalidJson_ShouldSetError()
    {
        await WriteDataJsonAsync("{ this is not valid json }}}");

        await _service.LoadAsync();

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("Failed to parse data.json");
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_ShouldSetError()
    {
        await WriteDataJsonAsync("");

        await _service.LoadAsync();

        _service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_NullJsonLiteral_ShouldSetError()
    {
        await WriteDataJsonAsync("null");

        await _service.LoadAsync();

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("empty or could not be parsed");
    }

    [Fact]
    public async Task LoadAsync_JsonArray_ShouldSetError()
    {
        await WriteDataJsonAsync("[1, 2, 3]");

        await _service.LoadAsync();

        _service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_TruncatedJson_ShouldSetError()
    {
        await WriteDataJsonAsync("""{ "title": "Test" """);

        await _service.LoadAsync();

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("Failed to parse data.json");
    }

    // --- Edge Case JSON Tests ---

    [Fact]
    public async Task LoadAsync_MinimalValidJson_ShouldSucceed()
    {
        await WriteDataJsonAsync("{}");

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        _service.Data.Should().NotBeNull();
        _service.Data!.Title.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_ExtraJsonFields_ShouldIgnoreThem()
    {
        await WriteDataJsonAsync("""
        {
            "title": "Test",
            "unknownField": "should be ignored",
            "anotherExtra": 42
        }
        """);

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        _service.Data!.Title.Should().Be("Test");
    }

    [Fact]
    public async Task LoadAsync_CaseInsensitivePropertyNames_ShouldWork()
    {
        await WriteDataJsonAsync("""
        {
            "Title": "Case Test",
            "SUBTITLE": "Upper",
            "currentmonth": "Jan"
        }
        """);

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        _service.Data!.Title.Should().Be("Case Test");
    }

    // --- Validation / Warning Tests ---

    [Fact]
    public async Task LoadAsync_MissingTitle_ShouldLogWarning()
    {
        await WriteDataJsonAsync("""{ "subtitle": "No Title" }""");

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("title")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_EmptyTracks_ShouldLogWarning()
    {
        await WriteDataJsonAsync("""
        {
            "title": "Test",
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-06-30", "nowDate": "2026-04-10", "tracks": [] }
        }
        """);

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("no timeline tracks")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_WhitespaceTitle_ShouldLogWarning()
    {
        await WriteDataJsonAsync("""{ "title": "   " }""");

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("title")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // --- Success Logging Tests ---

    [Fact]
    public async Task LoadAsync_Success_ShouldLogInformation()
    {
        await WriteDataJsonAsync(CreateValidJson());

        await _service.LoadAsync();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // --- State Consistency Tests ---

    [Fact]
    public async Task LoadAsync_AfterError_ErrorStateIsSet()
    {
        await _service.LoadAsync("/nonexistent/data.json");

        _service.IsError.Should().BeTrue();
        _service.Data.Should().BeNull();
        _service.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoadAsync_AfterSuccess_ErrorStateIsCleared()
    {
        await WriteDataJsonAsync(CreateValidJson());

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        _service.ErrorMessage.Should().BeNull();
        _service.Data.Should().NotBeNull();
    }

    // --- Complex Data Tests ---

    [Fact]
    public async Task LoadAsync_MultipleTracksAndMilestones_ShouldParseAll()
    {
        var json = """
        {
            "title": "Complex",
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "id": "M1",
                        "name": "Platform",
                        "color": "#0078D4",
                        "milestones": [
                            { "date": "2026-02-01", "label": "PoC", "type": "poc" },
                            { "date": "2026-04-01", "label": "GA", "type": "production" },
                            { "date": "2026-03-01", "label": "Check", "type": "checkpoint" }
                        ]
                    },
                    {
                        "id": "M2",
                        "name": "API",
                        "color": "#34A853",
                        "milestones": []
                    },
                    {
                        "id": "M3",
                        "name": "UX",
                        "color": "#EA4335",
                        "milestones": [
                            { "date": "2026-05-15", "label": "Launch", "type": "production" }
                        ]
                    }
                ]
            }
        }
        """;
        await WriteDataJsonAsync(json);

        await _service.LoadAsync();

        _service.Data!.Timeline.Tracks.Should().HaveCount(3);
        _service.Data.Timeline.Tracks[0].Milestones.Should().HaveCount(3);
        _service.Data.Timeline.Tracks[1].Milestones.Should().BeEmpty();
        _service.Data.Timeline.Tracks[2].Milestones.Should().HaveCount(1);
    }

    [Fact]
    public async Task LoadAsync_HeatmapWithMultipleMonthsAndItems_ShouldParseAll()
    {
        var json = """
        {
            "title": "Heatmap Test",
            "heatmap": {
                "shipped": {
                    "Jan": ["A", "B", "C"],
                    "Feb": ["D"],
                    "Mar": ["E", "F"]
                },
                "inProgress": {
                    "Apr": ["G"]
                },
                "carryover": {
                    "Feb": ["H"]
                },
                "blockers": {
                    "Mar": ["I", "J"],
                    "Apr": ["K"]
                }
            }
        }
        """;
        await WriteDataJsonAsync(json);

        await _service.LoadAsync();

        var heatmap = _service.Data!.Heatmap;
        heatmap.Shipped.Should().HaveCount(3);
        heatmap.Shipped["Jan"].Should().HaveCount(3);
        heatmap.InProgress["Apr"].Should().ContainSingle();
        heatmap.Blockers.Should().HaveCount(2);
    }

    // --- Unicode and Special Characters ---

    [Fact]
    public async Task LoadAsync_UnicodeContent_ShouldParseCorrectly()
    {
        await WriteDataJsonAsync("""
        {
            "title": "Ünîcödé Tëst — Dashboard",
            "subtitle": "日本語チーム"
        }
        """);

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        _service.Data!.Title.Should().Contain("Ünîcödé");
        _service.Data.Subtitle.Should().Be("日本語チーム");
    }

    [Fact]
    public async Task LoadAsync_SpecialCharactersInLinks_ShouldPreserve()
    {
        await WriteDataJsonAsync("""
        {
            "title": "Test",
            "backlogLink": "https://dev.azure.com/org/project/_backlogs?param=a&other=b%20c"
        }
        """);

        await _service.LoadAsync();

        _service.Data!.BacklogLink.Should().Contain("param=a&other=b%20c");
    }

    // --- Empty Collections ---

    [Fact]
    public async Task LoadAsync_EmptyMonths_ShouldReturnEmptyList()
    {
        await WriteDataJsonAsync("""{ "title": "Test", "months": [] }""");

        await _service.LoadAsync();

        _service.Data!.Months.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_EmptyHeatmapCategories_ShouldReturnEmptyDicts()
    {
        await WriteDataJsonAsync("""
        {
            "title": "Test",
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """);

        await _service.LoadAsync();

        _service.Data!.Heatmap.Shipped.Should().BeEmpty();
        _service.Data.Heatmap.InProgress.Should().BeEmpty();
        _service.Data.Heatmap.Carryover.Should().BeEmpty();
        _service.Data.Heatmap.Blockers.Should().BeEmpty();
    }
}