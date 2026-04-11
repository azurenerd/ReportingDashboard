using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying the Timeline component renders correctly
/// when driven through the Dashboard page with a real DashboardDataService
/// loaded from JSON on disk. Covers SVG milestone rendering, track labels,
/// NOW marker, and data flow from service → Dashboard → Timeline.
/// </summary>
[Trait("Category", "Integration")]
public class TimelineComponentIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public TimelineComponentIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"TlInteg_{Guid.NewGuid():N}");
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

    private string BuildJson(
        int trackCount = 3,
        bool includeNowDate = true,
        string[]? milestoneTypes = null)
    {
        milestoneTypes ??= new[] { "poc", "production", "checkpoint" };
        var tracks = Enumerable.Range(1, trackCount).Select(i => new
        {
            name = $"M{i}",
            label = $"Track {i} Label",
            color = $"#{i:D2}{i:D2}F4",
            milestones = milestoneTypes.Select((type, idx) => new
            {
                date = $"2026-{(idx + 2):D2}-15",
                type,
                label = $"M{i} {type} milestone"
            }).ToArray()
        }).ToArray();

        return System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Timeline Integration Test",
            subtitle = "Team - April 2026",
            backlogLink = "https://dev.azure.com/test",
            currentMonth = "April",
            months = new[] { "January", "February", "March", "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = includeNowDate ? "2026-04-10" : "",
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
    }

    #region Timeline Section Rendering via Dashboard

    [Fact]
    public void Dashboard_WithTimeline_RendersTimelineArea()
    {
        var svc = CreateServiceFromJson(BuildJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("tl-area", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithTimeline_RendersSvgElement()
    {
        var svc = CreateServiceFromJson(BuildJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var svg = cut.Find("svg");
        Assert.NotNull(svg);
        Assert.Equal("1560", svg.GetAttribute("width"));
    }

    [Fact]
    public void Dashboard_With3Tracks_RendersAllTrackNames()
    {
        var svc = CreateServiceFromJson(BuildJson(trackCount: 3));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("M1", cut.Markup);
        Assert.Contains("M2", cut.Markup);
        Assert.Contains("M3", cut.Markup);
        Assert.Contains("Track 1 Label", cut.Markup);
        Assert.Contains("Track 2 Label", cut.Markup);
        Assert.Contains("Track 3 Label", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithPocMilestones_RendersGoldDiamonds()
    {
        var svc = CreateServiceFromJson(BuildJson(trackCount: 1, milestoneTypes: new[] { "poc" }));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("#F4B400", cut.Markup);
        Assert.Contains("polygon", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithProductionMilestones_RendersGreenDiamonds()
    {
        var svc = CreateServiceFromJson(BuildJson(trackCount: 1, milestoneTypes: new[] { "production" }));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("#34A853", cut.Markup);
        Assert.Contains("polygon", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithCheckpointMilestones_RendersCircles()
    {
        var svc = CreateServiceFromJson(BuildJson(trackCount: 1, milestoneTypes: new[] { "checkpoint" }));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("circle", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithAllMilestoneTypes_RendersAllShapes()
    {
        var svc = CreateServiceFromJson(BuildJson(trackCount: 1));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // PoC diamond
        Assert.Contains("#F4B400", cut.Markup);
        // Production diamond
        Assert.Contains("#34A853", cut.Markup);
        // Checkpoint circle
        Assert.Contains("circle", cut.Markup);
        // Drop shadow filter
        Assert.Contains("feDropShadow", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithNowDate_RendersNowMarker()
    {
        var svc = CreateServiceFromJson(BuildJson(includeNowDate: true));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("NOW", cut.Markup);
        Assert.Contains("#EA4335", cut.Markup);
        Assert.Contains("stroke-dasharray=\"5,3\"", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithoutNowDate_NoNowMarker()
    {
        var svc = CreateServiceFromJson(BuildJson(includeNowDate: false));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain(">NOW<", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithMilestoneLabels_RendersLabelText()
    {
        var svc = CreateServiceFromJson(BuildJson(trackCount: 1, milestoneTypes: new[] { "poc" }));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("M1 poc milestone", cut.Markup);
    }

    [Fact]
    public void Dashboard_Timeline_MonthLabelsRendered()
    {
        var svc = CreateServiceFromJson(BuildJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Jan", cut.Markup);
        Assert.Contains("Feb", cut.Markup);
        Assert.Contains("Mar", cut.Markup);
    }

    [Fact]
    public void Dashboard_With5Tracks_SvgHeightScales()
    {
        var svc = CreateServiceFromJson(BuildJson(trackCount: 5));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var svg = cut.Find("svg");
        var height = svg.GetAttribute("height");
        Assert.NotNull(height);
        var h = double.Parse(height!);
        // 5 tracks * 56 = 280
        Assert.True(h >= 280, $"SVG height {h} should scale for 5 tracks (expected ≥280)");
    }

    [Fact]
    public void Dashboard_TrackColors_FlowThroughToSvg()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Color Test",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[]
                {
                    new { name = "M1", label = "Red Track", color = "#FF0000", milestones = Array.Empty<object>() },
                    new { name = "M2", label = "Blue Track", color = "#0000FF", milestones = Array.Empty<object>() }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        var svc = CreateServiceFromJson(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("#FF0000", cut.Markup);
        Assert.Contains("#0000FF", cut.Markup);
    }

    #endregion
}