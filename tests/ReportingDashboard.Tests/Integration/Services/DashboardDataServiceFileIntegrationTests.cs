using System.Text.Json;
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
    private readonly DashboardDataService _service;
    private readonly string _tempDir;

    public DashboardDataServiceFileIntegrationTests()
    {
        var logger = new Mock<ILogger<DashboardDataService>>();
        _service = new DashboardDataService(logger.Object);
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardInteg_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteJson(string json, string fileName = "data.json")
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, json);
        return path;
    }

    private static string CreateFullValidJson() => """
    {
        "title": "Privacy Automation Release Roadmap",
        "subtitle": "Trusted Platform - Privacy Automation - April 2026",
        "backlogLink": "https://dev.azure.com/org/project/_backlogs",
        "currentMonth": "Apr",
        "months": ["Jan", "Feb", "Mar", "Apr"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "name": "M1",
                    "label": "Chatbot",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-02-01", "type": "checkpoint", "label": "Design Review" },
                        { "date": "2026-03-15", "type": "poc", "label": "PoC Complete" },
                        { "date": "2026-05-01", "type": "production", "label": "GA Release" }
                    ]
                },
                {
                    "name": "M2",
                    "label": "Data Pipeline",
                    "color": "#00897B",
                    "milestones": [
                        { "date": "2026-01-15", "type": "checkpoint", "label": "Kickoff" },
                        { "date": "2026-04-01", "type": "poc", "label": "Pipeline PoC" }
                    ]
                },
                {
                    "name": "M3",
                    "label": "Compliance",
                    "color": "#546E7A",
                    "milestones": [
                        { "date": "2026-06-01", "type": "production", "label": "Audit Complete" }
                    ]
                }
            ]
        },
        "heatmap": {
            "shipped": {
                "jan": ["Auth Module", "CI Pipeline Setup"],
                "feb": ["Search API", "Data Export"],
                "mar": ["Notification Service"]
            },
            "inProgress": {
                "mar": ["Dashboard UI"],
                "apr": ["Reporting Engine", "API Gateway"]
            },
            "carryover": {
                "apr": ["Legacy Migration"]
            },
            "blockers": {
                "apr": ["Vendor License Delay"]
            }
        }
    }
    """;

    [Fact]
    public async Task LoadAsync_FullValidJson_DeserializesAllTopLevelFields()
    {
        var path = WriteJson(CreateFullValidJson());

        await _service.LoadAsync(path);

        _service.IsError.Should().BeFalse();
        _service.Data.Should().NotBeNull();
        _service.Data!.Title.Should().Be("Privacy Automation Release Roadmap");
        _service.Data.Subtitle.Should().Contain("April 2026");
        _service.Data.BacklogLink.Should().StartWith("https://");
        _service.Data.CurrentMonth.Should().Be("Apr");
        _service.Data.Months.Should().HaveCount(4);
        _service.Data.Months.Should().ContainInOrder("Jan", "Feb", "Mar", "Apr");
    }

    [Fact]
    public async Task LoadAsync_FullValidJson_DeserializesTimelineWithThreeTracks()
    {
        var path = WriteJson(CreateFullValidJson());

        await _service.LoadAsync(path);

        var timeline = _service.Data!.Timeline;
        timeline.Should().NotBeNull();
        timeline!.StartDate.Should().Be("2026-01-01");
        timeline.EndDate.Should().Be("2026-06-30");
        timeline.NowDate.Should().Be("2026-04-10");
        timeline.Tracks.Should().HaveCount(3);
    }

    [Fact]
    public async Task LoadAsync_FullValidJson_DeserializesTrackDetails()
    {
        var path = WriteJson(CreateFullValidJson());

        await _service.LoadAsync(path);

        var tracks = _service.Data!.Timeline!.Tracks;

        tracks[0].Name.Should().Be("M1");
        tracks[0].Label.Should().Be("Chatbot");
        tracks[0].Color.Should().Be("#0078D4");

        tracks[1].Name.Should().Be("M2");
        tracks[1].Label.Should().Be("Data Pipeline");
        tracks[1].Color.Should().Be("#00897B");

        tracks[2].Name.Should().Be("M3");
        tracks[2].Label.Should().Be("Compliance");
        tracks[2].Color.Should().Be("#546E7A");
    }

    [Fact]
    public async Task LoadAsync_FullValidJson_DeserializesMilestonesWithAllTypes()
    {
        var path = WriteJson(CreateFullValidJson());

        await _service.LoadAsync(path);

        var m1Milestones = _service.Data!.Timeline!.Tracks[0].Milestones;
        m1Milestones.Should().HaveCount(3);

        m1Milestones[0].Type.Should().Be("checkpoint");
        m1Milestones[0].Label.Should().Be("Design Review");
        m1Milestones[0].Date.Should().Be("2026-02-01");

        m1Milestones[1].Type.Should().Be("poc");
        m1Milestones[1].Label.Should().Be("PoC Complete");

        m1Milestones[2].Type.Should().Be("production");
        m1Milestones[2].Label.Should().Be("GA Release");
    }

    [Fact]
    public async Task LoadAsync_FullValidJson_DeserializesHeatmapShippedItems()
    {
        var path = WriteJson(CreateFullValidJson());

        await _service.LoadAsync(path);

        var heatmap = _service.Data!.Heatmap;
        heatmap.Should().NotBeNull();
        heatmap!.Shipped.Should().ContainKey("jan");
        heatmap.Shipped["jan"].Should().HaveCount(2);
        heatmap.Shipped["jan"].Should().Contain("Auth Module");
        heatmap.Shipped["jan"].Should().Contain("CI Pipeline Setup");
        heatmap.Shipped["feb"].Should().HaveCount(2);
        heatmap.Shipped["mar"].Should().HaveCount(1);
    }

    [Fact]
    public async Task LoadAsync_FullValidJson_DeserializesHeatmapAllCategories()
    {
        var path = WriteJson(CreateFullValidJson());

        await _service.LoadAsync(path);

        var heatmap = _service.Data!.Heatmap!;

        heatmap.InProgress.Should().ContainKey("mar");
        heatmap.InProgress.Should().ContainKey("apr");
        heatmap.InProgress["apr"].Should().HaveCount(2);

        heatmap.Carryover.Should().ContainKey("apr");
        heatmap.Carryover["apr"].Should().Contain("Legacy Migration");

        heatmap.Blockers.Should().ContainKey("apr");
        heatmap.Blockers["apr"].Should().Contain("Vendor License Delay");
    }

    [Fact]
    public async Task LoadAsync_MissingHeatmapKeys_DoNotCauseNullReference()
    {
        var path = WriteJson(CreateFullValidJson());

        await _service.LoadAsync(path);

        var heatmap = _service.Data!.Heatmap!;

        heatmap.Shipped.ContainsKey("may").Should().BeFalse();
        heatmap.Shipped.ContainsKey("jun").Should().BeFalse();
        heatmap.InProgress.ContainsKey("jan").Should().BeFalse();
        heatmap.Blockers.ContainsKey("jan").Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_SetsErrorWithPath()
    {
        var missingPath = Path.Combine(_tempDir, "nonexistent", "data.json");

        await _service.LoadAsync(missingPath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("not found");
        _service.ErrorMessage.Should().Contain(missingPath);
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_TrailingComma_SetsParseError()
    {
        var json = """{ "title": "Test", "subtitle": "Sub", }""";
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("Failed to parse data.json");
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_UnclosedBrace_SetsParseError()
    {
        var json = """{ "title": "Test" """;
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("Failed to parse");
    }

    [Fact]
    public async Task LoadAsync_EmptyTitle_SetsValidationError()
    {
        var json = """
        {
            "title": "",
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
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("title is required");
    }

    [Fact]
    public async Task LoadAsync_MissingMultipleRequiredFields_ReportsAllErrors()
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
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("title is required");
        _service.ErrorMessage.Should().Contain("subtitle is required");
        _service.ErrorMessage.Should().Contain("backlogLink is required");
        _service.ErrorMessage.Should().Contain("months is required");
        _service.ErrorMessage.Should().Contain("timeline.tracks is required");
    }

    [Fact]
    public async Task LoadAsync_EmptyMonthsArray_SetsValidationError()
    {
        var json = """
        {
            "title": "T",
            "subtitle": "S",
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
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("months is required and must be non-empty");
    }

    [Fact]
    public async Task LoadAsync_EmptyTracksArray_SetsValidationError()
    {
        var json = """
        {
            "title": "T",
            "subtitle": "S",
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
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("timeline.tracks is required");
    }

    [Fact]
    public async Task LoadAsync_NullJsonLiteral_SetsError()
    {
        var path = WriteJson("null");

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("could not be loaded");
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_SetsError()
    {
        var path = WriteJson("");

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_JsonArray_SetsError()
    {
        var path = WriteJson("[1,2,3]");

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_SuccessThenFailure_ResetsState()
    {
        var validPath = WriteJson(CreateFullValidJson());
        await _service.LoadAsync(validPath);
        _service.Data.Should().NotBeNull();
        _service.IsError.Should().BeFalse();

        await _service.LoadAsync(Path.Combine(_tempDir, "nope.json"));

        _service.IsError.Should().BeTrue();
        _service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_FailureThenSuccess_RestoresData()
    {
        await _service.LoadAsync(Path.Combine(_tempDir, "nope.json"));
        _service.IsError.Should().BeTrue();

        var validPath = WriteJson(CreateFullValidJson());
        await _service.LoadAsync(validPath);

        _service.IsError.Should().BeFalse();
        _service.Data.Should().NotBeNull();
        _service.Data!.Title.Should().Be("Privacy Automation Release Roadmap");
    }

    [Fact]
    public async Task LoadAsync_Utf8WithBom_LoadsSuccessfully()
    {
        var json = CreateFullValidJson();
        var path = Path.Combine(_tempDir, "bom.json");
        await File.WriteAllTextAsync(path, json, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        await _service.LoadAsync(path);

        _service.IsError.Should().BeFalse();
        _service.Data!.Title.Should().Be("Privacy Automation Release Roadmap");
    }

    [Fact]
    public async Task LoadAsync_UnicodeCharacters_PreservedCorrectly()
    {
        var json = """
        {
            "title": "Datenübersicht für Projekte",
            "subtitle": "Équipe – Données spéciales",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "Überblick", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeFalse();
        _service.Data!.Title.Should().Contain("für");
        _service.Data.Subtitle.Should().Contain("Équipe");
        _service.Data.Timeline!.Tracks[0].Label.Should().Contain("Überblick");
    }

    [Fact]
    public async Task LoadAsync_ExtraJsonFields_AreIgnored()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "unknownField": "should be ignored",
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "extraTimelineField": true,
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [], "extra": 42 }]
            }
        }
        """;
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeFalse();
        _service.Data!.Title.Should().Be("Test");
    }

    [Fact]
    public async Task LoadAsync_LargeHeatmapPayload_HandlesCorrectly()
    {
        var items = string.Join(",", Enumerable.Range(1, 50).Select(i => $"\"Work Item {i}\""));
        var json = $$"""
        {
            "title": "Large Data",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            },
            "heatmap": {
                "shipped": { "jan": [{{items}}], "feb": [{{items}}] },
                "inProgress": { "mar": [{{items}}] },
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeFalse();
        _service.Data!.Heatmap!.Shipped["jan"].Should().HaveCount(50);
        _service.Data.Heatmap.Shipped["feb"].Should().HaveCount(50);
        _service.Data.Heatmap.InProgress["mar"].Should().HaveCount(50);
    }
}