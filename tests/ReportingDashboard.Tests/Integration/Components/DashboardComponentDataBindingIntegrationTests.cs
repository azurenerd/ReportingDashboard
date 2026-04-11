using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Integration tests verifying Dashboard component correctly renders
/// different data scenarios, including multi-track data, large heatmaps,
/// and various valid/invalid configurations flowing through the service.
/// </summary>
[Trait("Category", "Integration")]
public class DashboardComponentDataBindingIntegrationTests : TestContext
{
    private readonly string _tempDir;
    private readonly DashboardDataService _service;

    public DashboardComponentDataBindingIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardBind_{Guid.NewGuid():N}");
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
    public async Task Dashboard_WithThreeTracks_ShowsTitle()
    {
        var json = """
        {
            "title": "Three Track Dashboard",
            "subtitle": "Integration",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    { "name": "Alpha", "label": "M1", "color": "#0078D4", "milestones": [{ "date": "2026-02-15", "label": "Feb 15", "type": "poc" }] },
                    { "name": "Beta", "label": "M2", "color": "#00897B", "milestones": [{ "date": "2026-03-01", "label": "Mar 1", "type": "checkpoint" }] },
                    { "name": "Gamma", "label": "M3", "color": "#546E7A", "milestones": [{ "date": "2026-05-01", "label": "May 1", "type": "production" }] }
                ]
            },
            "heatmap": {
                "shipped": { "jan": ["A", "B"], "feb": ["C"] },
                "inProgress": { "apr": ["D", "E"] },
                "carryover": { "apr": ["F"] },
                "blockers": { "apr": ["G"] }
            }
        }
        """;
        var path = WriteJson(json);
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Data loaded:");
        cut.Markup.Should().Contain("Three Track Dashboard");
        _service.Data!.Timeline.Tracks.Should().HaveCount(3);
        _service.Data.Heatmap.Shipped.Should().HaveCount(2);
    }

    [Fact]
    public async Task Dashboard_WithSpecialCharsInTitle_RendersEscaped()
    {
        var json = """
        {
            "title": "Dashboard <Alpha> & \"Beta\"",
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

        cut.Markup.Should().Contain("Data loaded:");
        // Blazor HTML-encodes the special characters
        cut.Markup.Should().Contain("&amp;");
    }

    [Fact]
    public async Task Dashboard_EndDateAfterStartDate_Succeeds()
    {
        var json = """
        {
            "title": "Date Ordering Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-01-02",
                "nowDate": "2026-01-01",
                "tracks": [{ "name": "T", "label": "L", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Data loaded:");
        cut.Markup.Should().Contain("Date Ordering Test");
    }

    [Fact]
    public async Task Dashboard_InvalidMilestoneType_ShowsValidationError()
    {
        var json = """
        {
            "title": "Type Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "T", "label": "L", "milestones": [{ "date": "2026-03-01", "type": "invalid_type", "label": "X" }] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Error:");
        cut.Markup.Should().Contain("type");
        cut.Markup.Should().Contain("must be one of");
    }

    [Fact]
    public async Task Dashboard_CurrentMonthMismatch_ShowsValidationError()
    {
        var json = """
        {
            "title": "Month Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Dec",
            "months": ["Jan", "Feb"],
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
        cut.Markup.Should().Contain("currentMonth must exist");
    }

    [Fact]
    public async Task Dashboard_EmptyJsonObject_ShowsValidationError()
    {
        var path = WriteJson("{}");
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Error:");
        cut.Markup.Should().Contain("title");
    }

    [Fact]
    public async Task Dashboard_JsonArrayInsteadOfObject_ShowsParseError()
    {
        var path = WriteJson("[]");
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Error:");
    }

    [Fact]
    public async Task Dashboard_ManyMonthsAndHeatmapKeys_AllValidate()
    {
        var json = """
        {
            "title": "12 Month Dashboard",
            "subtitle": "Full Year",
            "backlogLink": "https://test.com",
            "currentMonth": "Jun",
            "months": ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-12-31",
                "nowDate": "2026-06-15",
                "tracks": [{ "name": "T", "label": "L", "milestones": [] }]
            },
            "heatmap": {
                "shipped": { "jan": ["A"], "feb": ["B"], "mar": ["C"], "apr": ["D"], "may": ["E"], "jun": ["F"] },
                "inProgress": { "jul": ["G"], "aug": ["H"] },
                "carryover": { "sep": ["I"] },
                "blockers": { "oct": ["J"], "nov": ["K"], "dec": ["L"] }
            }
        }
        """;
        var path = WriteJson(json);
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Data loaded:");
        cut.Markup.Should().Contain("12 Month Dashboard");
        _service.Data!.Months.Should().HaveCount(12);
        _service.Data.Heatmap.Shipped.Should().HaveCount(6);
        _service.Data.Heatmap.Blockers.Should().HaveCount(3);
    }
}