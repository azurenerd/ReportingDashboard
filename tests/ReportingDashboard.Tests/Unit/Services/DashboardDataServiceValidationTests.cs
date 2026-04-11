using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

/// <summary>
/// Focuses on validation edge cases and boundary conditions in DashboardDataService.Load().
/// Complements DashboardDataServiceTests which covers the core happy/error paths.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardDataServiceValidationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dataDir;
    private readonly string _dataJsonPath;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DashboardDataServiceValidationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardValTests_{Guid.NewGuid():N}");
        _dataDir = Path.Combine(_tempDir, "Data");
        Directory.CreateDirectory(_dataDir);
        _dataJsonPath = Path.Combine(_dataDir, "data.json");

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.ContentRootPath).Returns(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private DashboardDataService CreateService() => new(_envMock.Object);

    private void WriteJson(string json) => File.WriteAllText(_dataJsonPath, json);

    private static string BuildJson(
        string title = "Test",
        string subtitle = "Sub",
        string backlogUrl = "",
        string currentMonth = "Apr",
        string timelineMonths = """["Jan","Feb"]""",
        string nowPosition = "0.5",
        string tracks = "[]",
        string heatmapMonths = """["Jan","Feb"]""",
        string categories = "[]")
    {
        return $$"""
        {
            "project": { "title": "{{title}}", "subtitle": "{{subtitle}}", "backlogUrl": "{{backlogUrl}}", "currentMonth": "{{currentMonth}}" },
            "timeline": { "months": {{timelineMonths}}, "nowPosition": {{nowPosition}} },
            "tracks": {{tracks}},
            "heatmap": { "months": {{heatmapMonths}}, "categories": {{categories}} }
        }
        """;
    }

    // ---- Subtitle Validation ----

    [Fact]
    public void Load_EmptySubtitle_ReturnsValidationError()
    {
        WriteJson(BuildJson(subtitle: ""));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("project.subtitle");
    }

    [Fact]
    public void Load_WhitespaceSubtitle_ReturnsValidationError()
    {
        WriteJson(BuildJson(subtitle: "   "));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("project.subtitle");
    }

    // ---- CurrentMonth Validation ----

    [Fact]
    public void Load_EmptyCurrentMonth_ReturnsValidationError()
    {
        WriteJson(BuildJson(currentMonth: ""));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("project.currentMonth");
    }

    [Fact]
    public void Load_WhitespaceCurrentMonth_ReturnsValidationError()
    {
        WriteJson(BuildJson(currentMonth: "   "));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("project.currentMonth");
    }

    // ---- NowPosition Boundary Values ----

    [Fact]
    public void Load_NowPositionExactlyZero_ReturnsSuccess()
    {
        WriteJson(BuildJson(nowPosition: "0.0"));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Timeline.NowPosition.Should().Be(0.0);
    }

    [Fact]
    public void Load_NowPositionExactlyOne_ReturnsSuccess()
    {
        WriteJson(BuildJson(nowPosition: "1.0"));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Timeline.NowPosition.Should().Be(1.0);
    }

    [Fact]
    public void Load_NowPositionSlightlyAboveOne_ReturnsValidationError()
    {
        WriteJson(BuildJson(nowPosition: "1.001"));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("nowPosition");
    }

    [Fact]
    public void Load_NowPositionNegative_ReturnsValidationError()
    {
        WriteJson(BuildJson(nowPosition: "-0.1"));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("nowPosition");
    }

    [Fact]
    public void Load_NowPositionLargeValue_ReturnsValidationError()
    {
        WriteJson(BuildJson(nowPosition: "99.9"));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("nowPosition");
    }

    [Fact]
    public void Load_NowPositionMidRange_ReturnsSuccess()
    {
        WriteJson(BuildJson(nowPosition: "0.42"));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data!.Timeline.NowPosition.Should().Be(0.42);
    }

    // ---- Timeline Months Validation ----

    [Fact]
    public void Load_EmptyTimelineMonths_ReturnsValidationError()
    {
        WriteJson(BuildJson(timelineMonths: "[]"));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("timeline.months");
    }

    [Fact]
    public void Load_SingleTimelineMonth_ReturnsSuccess()
    {
        WriteJson(BuildJson(timelineMonths: """["Jan"]"""));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data!.Timeline.Months.Should().HaveCount(1);
    }

    [Fact]
    public void Load_ManyTimelineMonths_ReturnsSuccess()
    {
        WriteJson(BuildJson(timelineMonths: """["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"]"""));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data!.Timeline.Months.Should().HaveCount(12);
    }

    // ---- Heatmap Categories Validation ----

    [Fact]
    public void Load_NullHeatmapCategories_ReturnsValidationError()
    {
        // Construct JSON where heatmap has no categories key at all
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"] }
        }
        """;
        WriteJson(json);
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("heatmap.categories");
    }

    [Fact]
    public void Load_EmptyHeatmapCategories_ReturnsSuccess()
    {
        WriteJson(BuildJson(categories: "[]"));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data!.Heatmap.Categories.Should().BeEmpty();
    }

    // ---- JSON Edge Cases ----

    [Fact]
    public void Load_JsonWithExtraFields_IgnoresUnknownFields()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan", "unknownField": 42 },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] },
            "extraTopLevel": true
        }
        """;
        WriteJson(json);
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Project.Title.Should().Be("T");
    }

    [Fact]
    public void Load_CaseInsensitivePropertyNames_Deserializes()
    {
        var json = """
        {
            "Project": { "Title": "T", "Subtitle": "S", "BacklogUrl": "", "CurrentMonth": "Jan" },
            "Timeline": { "Months": ["Jan"], "NowPosition": 0.5 },
            "Tracks": [],
            "Heatmap": { "Months": ["Jan"], "Categories": [] }
        }
        """;
        WriteJson(json);
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data!.Project.Title.Should().Be("T");
    }

    [Fact]
    public void Load_JsonArray_ReturnsError()
    {
        WriteJson("[]");
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Load_JsonString_ReturnsError()
    {
        WriteJson("\"hello\"");
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Load_JsonNumber_ReturnsError()
    {
        WriteJson("42");
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Load_EmptyObject_ReturnsValidationError()
    {
        WriteJson("{}");
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Load_UnicodeContent_PreservesCharacters()
    {
        var json = """
        {
            "project": { "title": "Проект Dashboard 🚀", "subtitle": "Ünit Tëst — Special «chars»", "backlogUrl": "", "currentMonth": "Апр" },
            "timeline": { "months": ["Янв"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Янв"], "categories": [] }
        }
        """;
        WriteJson(json);
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data!.Project.Title.Should().Contain("🚀");
        data.Project.Subtitle.Should().Contain("«chars»");
    }

    [Fact]
    public void Load_JsonWithBom_DeserializesSuccessfully()
    {
        var json = BuildJson();
        // Write with UTF-8 BOM
        File.WriteAllText(_dataJsonPath, json, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data.Should().NotBeNull();
    }

    [Fact]
    public void Load_NeverThrows_ReturnsErrorTupleInstead()
    {
        // Even with an unreadable path, Load should never throw
        var badEnv = new Mock<IWebHostEnvironment>();
        badEnv.Setup(e => e.ContentRootPath).Returns(Path.Combine("Z:\\", "nonexistent_" + Guid.NewGuid().ToString("N")));
        var service = new DashboardDataService(badEnv.Object);

        var act = () => service.Load();

        act.Should().NotThrow();
        var (data, error) = service.Load();
        data.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    // ---- Tracks Deserialization ----

    [Fact]
    public void Load_WithFullTrackData_DeserializesAllMilestones()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan","Feb"], "nowPosition": 0.5 },
            "tracks": [
                {
                    "id": "m1",
                    "label": "Track 1",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2024-01-15", "type": "checkpoint", "position": 0.1, "label": null },
                        { "date": "2024-02-01", "type": "poc", "position": 0.3, "label": "PoC Ready" },
                        { "date": "2024-03-01", "type": "production", "position": 0.7, "label": "GA" }
                    ]
                },
                {
                    "id": "m2",
                    "label": "Track 2",
                    "color": "#00897B",
                    "milestones": []
                }
            ],
            "heatmap": { "months": ["Jan","Feb"], "categories": [] }
        }
        """;
        WriteJson(json);
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data!.Tracks.Should().HaveCount(2);
        data.Tracks[0].Milestones.Should().HaveCount(3);
        data.Tracks[0].Milestones[0].Label.Should().BeNull();
        data.Tracks[0].Milestones[1].Label.Should().Be("PoC Ready");
        data.Tracks[0].Milestones[1].Type.Should().Be("poc");
        data.Tracks[1].Milestones.Should().BeEmpty();
    }

    // ---- Heatmap Items Deserialization ----

    [Fact]
    public void Load_WithHeatmapItems_DeserializesDictionaryCorrectly()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": {
                "months": ["Jan","Feb"],
                "categories": [
                    {
                        "name": "Shipped",
                        "cssClass": "ship",
                        "emoji": "✅",
                        "items": {
                            "Jan": ["Feature A", "Feature B"],
                            "Feb": ["Feature C"]
                        }
                    },
                    {
                        "name": "Blockers",
                        "cssClass": "block",
                        "emoji": "🚫",
                        "items": {}
                    }
                ]
            }
        }
        """;
        WriteJson(json);
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data!.Heatmap.Categories.Should().HaveCount(2);

        var shipped = data.Heatmap.Categories[0];
        shipped.Name.Should().Be("Shipped");
        shipped.CssClass.Should().Be("ship");
        shipped.Items.Should().HaveCount(2);
        shipped.Items["Jan"].Should().BeEquivalentTo(new[] { "Feature A", "Feature B" });
        shipped.Items["Feb"].Should().ContainSingle("Feature C");

        var blockers = data.Heatmap.Categories[1];
        blockers.Items.Should().BeEmpty();
    }

    // ---- Constructor ----

    [Fact]
    public void Constructor_UsesContentRootPathFromEnvironment()
    {
        var customPath = Path.Combine(Path.GetTempPath(), "CustomRoot_" + Guid.NewGuid().ToString("N"));
        var customDataDir = Path.Combine(customPath, "Data");
        Directory.CreateDirectory(customDataDir);

        try
        {
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.ContentRootPath).Returns(customPath);

            var service = new DashboardDataService(env.Object);
            var (data, error) = service.Load();

            // Should look for data.json in the custom path's Data/ dir
            error.Should().Contain("data.json not found at:");
            error.Should().Contain(customPath);
        }
        finally
        {
            Directory.Delete(customPath, true);
        }
    }

    // ---- Validation Order ----

    [Fact]
    public void Load_MultipleValidationFailures_ReturnsFirstError()
    {
        // Both title and subtitle are empty; should report title first
        var json = """
        {
            "project": { "title": "", "subtitle": "", "backlogUrl": "", "currentMonth": "" },
            "timeline": { "months": [], "nowPosition": 5.0 },
            "tracks": [],
            "heatmap": { "months": [], "categories": null }
        }
        """;
        WriteJson(json);
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("project.title", "should report the first validation failure");
    }

    [Fact]
    public void Load_TitleValidButSubtitleInvalid_ReportsSubtitleError()
    {
        var json = """
        {
            "project": { "title": "Valid Title", "subtitle": "", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        WriteJson(json);
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("project.subtitle");
    }

    [Fact]
    public void Load_TitleAndSubtitleValidButCurrentMonthEmpty_ReportsCurrentMonthError()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        WriteJson(json);
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("project.currentMonth");
    }

    [Fact]
    public void Load_ProjectValidButTimelineMonthsEmpty_ReportsTimelineError()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": [], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        WriteJson(json);
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("timeline.months");
    }

    [Fact]
    public void Load_ProjectAndMonthsValidButNowPositionInvalid_ReportsNowPositionError()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": -0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        WriteJson(json);
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("nowPosition");
    }
}