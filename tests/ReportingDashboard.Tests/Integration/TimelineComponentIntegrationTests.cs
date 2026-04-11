using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for the root Components/Timeline.razor rendered with
/// real DashboardDataService data loaded from disk. Verifies the SVG timeline
/// renders correctly with data flowing from service → component.
/// </summary>
[Trait("Category", "Integration")]
public class TimelineComponentIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public TimelineComponentIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"TlComp_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public new void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            base.Dispose();
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
    }

    private DashboardDataService CreateServiceFromJson(string json)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        return svc;
    }

    private DashboardDataService CreateServiceWithTracks(int trackCount = 2, bool includeNow = true)
    {
        var tracks = Enumerable.Range(1, trackCount).Select(i => new
        {
            name = $"M{i}",
            label = $"Track {i}",
            color = $"#{i:D2}85F4",
            milestones = new object[]
            {
                new { date = "2026-02-15", type = "poc", label = $"Feb PoC {i}" },
                new { date = "2026-05-01", type = "production", label = $"May GA {i}" },
                new { date = "2026-03-10", type = "checkpoint", label = $"Mar Check {i}" }
            }
        }).ToArray();

        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Timeline Integration Test",
            subtitle = "Test Team",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "January", "February", "March", "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = includeNow ? "2026-04-10" : "",
                tracks
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        return CreateServiceFromJson(json);
    }

    #region Timeline Renders Through Service Data

    [Fact]
    public void Timeline_RendersTrackNamesFromService()
    {
        var svc = CreateServiceWithTracks(3);
        var tl = svc.Data!.Timeline;

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("M1", cut.Markup);
        Assert.Contains("Track 1", cut.Markup);
        Assert.Contains("M2", cut.Markup);
        Assert.Contains("Track 2", cut.Markup);
        Assert.Contains("M3", cut.Markup);
        Assert.Contains("Track 3", cut.Markup);
    }

    [Fact]
    public void Timeline_RendersSvgWithCorrectWidthFromService()
    {
        var svc = CreateServiceWithTracks(2);
        var tl = svc.Data!.Timeline;

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var svg = cut.Find("svg");
        Assert.Equal("1560", svg.GetAttribute("width"));
    }

    [Fact]
    public void Timeline_RendersPocDiamondsFromService()
    {
        var svc = CreateServiceWithTracks(1);
        var tl = svc.Data!.Timeline;

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("#F4B400", cut.Markup);
        Assert.Contains("Feb PoC 1", cut.Markup);
    }

    [Fact]
    public void Timeline_RendersProductionDiamondsFromService()
    {
        var svc = CreateServiceWithTracks(1);
        var tl = svc.Data!.Timeline;

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("#34A853", cut.Markup);
        Assert.Contains("May GA 1", cut.Markup);
    }

    [Fact]
    public void Timeline_RendersCheckpointCirclesFromService()
    {
        var svc = CreateServiceWithTracks(1);
        var tl = svc.Data!.Timeline;

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("circle", cut.Markup);
        Assert.Contains("Mar Check 1", cut.Markup);
    }

    [Fact]
    public void Timeline_RendersNowMarkerFromService()
    {
        var svc = CreateServiceWithTracks(1, includeNow: true);
        var tl = svc.Data!.Timeline;

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("NOW", cut.Markup);
        Assert.Contains("#EA4335", cut.Markup);
        Assert.Contains("stroke-dasharray=\"5,3\"", cut.Markup);
    }

    [Fact]
    public void Timeline_NoNowMarker_WhenNowDateEmpty()
    {
        var svc = CreateServiceWithTracks(1, includeNow: false);
        var tl = svc.Data!.Timeline;

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.DoesNotContain("NOW", cut.Markup);
    }

    [Fact]
    public void Timeline_RendersMonthGridLabelsFromServiceDates()
    {
        var svc = CreateServiceWithTracks(1);
        var tl = svc.Data!.Timeline;

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        // Jan 2026 through Jul 2026
        Assert.Contains("Jan", cut.Markup);
        Assert.Contains("Feb", cut.Markup);
        Assert.Contains("Mar", cut.Markup);
        Assert.Contains("Apr", cut.Markup);
        Assert.Contains("May", cut.Markup);
        Assert.Contains("Jun", cut.Markup);
        Assert.Contains("Jul", cut.Markup);
    }

    [Fact]
    public void Timeline_MultipleTracks_AllMilestonesRendered()
    {
        var svc = CreateServiceWithTracks(3);
        var tl = svc.Data!.Timeline;

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        // Each track has 3 milestones, 3 tracks = 9 milestones total
        Assert.Contains("Feb PoC 1", cut.Markup);
        Assert.Contains("Feb PoC 2", cut.Markup);
        Assert.Contains("Feb PoC 3", cut.Markup);
        Assert.Contains("May GA 1", cut.Markup);
        Assert.Contains("May GA 2", cut.Markup);
        Assert.Contains("May GA 3", cut.Markup);
    }

    [Fact]
    public void Timeline_SvgHeight_ScalesWithTrackCount()
    {
        var svc = CreateServiceWithTracks(5);
        var tl = svc.Data!.Timeline;

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var svg = cut.Find("svg");
        var height = double.Parse(svg.GetAttribute("height")!);
        // 5 tracks * 56 = 280
        Assert.Equal(280, height);
    }

    [Fact]
    public void Timeline_DropShadowFilter_PresentInSvg()
    {
        var svc = CreateServiceWithTracks(1);
        var tl = svc.Data!.Timeline;

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("filter", cut.Markup);
        Assert.Contains("id=\"sh\"", cut.Markup);
        Assert.Contains("feDropShadow", cut.Markup);
    }

    #endregion

    #region Track Color Integration

    [Fact]
    public void Timeline_TrackLines_UseTrackColorsFromService()
    {
        var svc = CreateServiceWithTracks(2);
        var tl = svc.Data!.Timeline;

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("stroke=\"#0185F4\"", cut.Markup);
        Assert.Contains("stroke=\"#0285F4\"", cut.Markup);
    }

    [Fact]
    public void Timeline_Sidebar_TrackLabelsColoredCorrectly()
    {
        var svc = CreateServiceWithTracks(2);
        var tl = svc.Data!.Timeline;

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("color:#0185F4", cut.Markup);
        Assert.Contains("color:#0285F4", cut.Markup);
    }

    #endregion
}