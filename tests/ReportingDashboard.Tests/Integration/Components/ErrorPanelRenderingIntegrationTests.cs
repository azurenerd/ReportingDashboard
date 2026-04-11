using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Components;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Integration tests verifying ErrorPanel rendering for various error scenarios
/// flowing through DashboardDataService → Dashboard component.
/// Complements the existing DashboardErrorPanelIntegrationTests with
/// coverage for validation errors and malformed JSON error messages.
/// </summary>
[Trait("Category", "Integration")]
public class ErrorPanelRenderingIntegrationTests : TestContext
{
    private readonly string _tempDir;
    private readonly DashboardDataService _service;

    public ErrorPanelRenderingIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ErrorPanel_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        var mockLogger = new Mock<ILogger<DashboardDataService>>();
        _service = new DashboardDataService(mockLogger.Object);
        Services.AddSingleton(_service);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
        base.Dispose(disposing);
    }

    private string WriteJson(string json)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public async Task Dashboard_ValidationError_EmptyTitle_ShowsValidationMessage()
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
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "T", "label": "L", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Error:");
        cut.Markup.Should().Contain("title");
    }

    [Fact]
    public async Task Dashboard_ValidationError_EmptyTracks_ShowsTracksMessage()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": []
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Error:");
        cut.Markup.Should().Contain("tracks");
    }

    [Fact]
    public async Task Dashboard_ValidationError_EmptyMonths_ShowsMonthsMessage()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": [],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "T", "label": "L", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Error:");
        cut.Markup.Should().Contain("months");
    }

    [Fact]
    public async Task Dashboard_NullJsonLiteral_ShowsDeserializationError()
    {
        var path = WriteJson("null");
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Error:");
        cut.Markup.Should().Contain("deserialization returned null");
    }

    [Fact]
    public async Task ErrorPanel_VariousMessages_AllRenderCorrectly()
    {
        var messages = new[]
        {
            "data.json not found at /some/path",
            "Failed to parse data.json: unexpected character",
            "data.json validation: title is required and must be non-empty",
            "data.json validation: timeline.tracks[0].milestones[1].date 'bad' is not a valid date"
        };

        foreach (var msg in messages)
        {
            var cut = RenderComponent<ErrorPanel>(p =>
                p.Add(x => x.ErrorMessage, msg));

            cut.Find(".error-panel").Should().NotBeNull();
            cut.Markup.Should().Contain(msg.Replace("<", "&lt;").Replace(">", "&gt;"));
            cut.Find("h2").TextContent.Should().Be("Dashboard data could not be loaded");
            cut.Find(".error-hint").Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Dashboard_HeatmapKeyMismatch_ShowsHeatmapError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "T", "label": "L", "milestones": [] }]
            },
            "heatmap": {
                "shipped": { "dec": ["Mismatched"] },
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        var path = WriteJson(json);
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Error:");
        cut.Markup.Should().Contain("shipped");
        cut.Markup.Should().Contain("dec");
    }
}