using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class DashboardDataFlowIntegrationTests : TestContext
{
    private readonly string _tempDir;
    private readonly DashboardDataService _service;

    public DashboardDataFlowIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardFlow_{Guid.NewGuid():N}");
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

    private static string GetFullJson() => """
    {
        "title": "Privacy Automation Roadmap",
        "subtitle": "Trusted Platform – Privacy – April 2026",
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
                        { "date": "2026-02-15", "label": "Feb 15", "type": "poc" },
                        { "date": "2026-04-01", "label": "Apr 1", "type": "production" }
                    ]
                },
                {
                    "name": "Pipeline",
                    "label": "M2",
                    "color": "#00897B",
                    "milestones": [
                        { "date": "2026-03-01", "label": "Mar 1", "type": "checkpoint" }
                    ]
                }
            ]
        },
        "heatmap": {
            "shipped": {
                "jan": ["SDK v2.1", "Pipeline"],
                "feb": ["API v3"]
            },
            "inProgress": {
                "apr": ["Monitoring"]
            },
            "carryover": {},
            "blockers": {
                "apr": ["Legal Review"]
            }
        }
    }
    """;

    [Fact]
    public async Task Dashboard_WithValidData_ShowsTitle()
    {
        var path = WriteJson(GetFullJson());
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Data loaded:");
        cut.Markup.Should().Contain("Privacy Automation Roadmap");
    }

    [Fact]
    public async Task Dashboard_WithValidData_ServiceHasCorrectTrackCount()
    {
        var path = WriteJson(GetFullJson());
        await _service.LoadAsync(path);

        _service.Data!.Timeline.Tracks.Should().HaveCount(2);
        _service.Data.Timeline.Tracks[0].Name.Should().Be("Chatbot");
        _service.Data.Timeline.Tracks[1].Name.Should().Be("Pipeline");
    }

    [Fact]
    public async Task Dashboard_WithValidData_ServiceHasMilestoneTypes()
    {
        var path = WriteJson(GetFullJson());
        await _service.LoadAsync(path);

        var allMilestones = _service.Data!.Timeline.Tracks
            .SelectMany(t => t.Milestones).ToList();

        allMilestones.Should().Contain(m => m.Type == "poc");
        allMilestones.Should().Contain(m => m.Type == "production");
        allMilestones.Should().Contain(m => m.Type == "checkpoint");
    }

    [Fact]
    public async Task Dashboard_WithEmptyTimeline_DoesNotCrash()
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
                "tracks": [{ "name": "T", "label": "T1", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Data loaded:");
    }

    [Fact]
    public async Task Dashboard_WithError_ShowsErrorMessage()
    {
        await _service.LoadAsync(Path.Combine(_tempDir, "nonexistent.json"));

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Error:");
        cut.Markup.Should().NotContain("Data loaded:");
    }

    [Fact]
    public async Task Dashboard_TrackDataFlowsToService()
    {
        var path = WriteJson(GetFullJson());
        await _service.LoadAsync(path);

        _service.Data!.Timeline.Tracks[0].Label.Should().Be("M1");
        _service.Data.Timeline.Tracks[0].Color.Should().Be("#0078D4");
        _service.Data.Timeline.Tracks[1].Label.Should().Be("M2");
        _service.Data.Timeline.Tracks[1].Color.Should().Be("#00897B");
    }

    [Fact]
    public async Task Dashboard_MilestoneLabelsFlowToService()
    {
        var path = WriteJson(GetFullJson());
        await _service.LoadAsync(path);

        var labels = _service.Data!.Timeline.Tracks
            .SelectMany(t => t.Milestones)
            .Select(m => m.Label)
            .ToList();

        labels.Should().Contain("Feb 15");
        labels.Should().Contain("Apr 1");
        labels.Should().Contain("Mar 1");
    }
}