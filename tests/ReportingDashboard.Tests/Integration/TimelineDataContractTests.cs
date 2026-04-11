using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying the data contract between data.json timeline fields
/// and the DashboardDataService → DashboardData → TimelineData model pipeline.
/// Ensures timeline-specific fields round-trip correctly through file I/O and JSON deserialization.
/// </summary>
[Trait("Category", "Integration")]
public class TimelineDataContractTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public TimelineDataContractTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"TlContract_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteJson(string json)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    private string WriteJsonObj(object data)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOpts));
        return path;
    }

    private DashboardDataService CreateService() => new(_logger);

    private object CreateValidDataWith(object timeline) => new
    {
        title = "T",
        subtitle = "S",
        backlogLink = "https://link",
        currentMonth = "April",
        months = new[] { "April" },
        timeline,
        heatmap = new
        {
            shipped = new Dictionary<string, string[]>(),
            inProgress = new Dictionary<string, string[]>(),
            carryover = new Dictionary<string, string[]>(),
            blockers = new Dictionary<string, string[]>()
        }
    };

    #region Timeline Date Fields

    [Theory]
    [InlineData("2026-01-01", "2026-07-01", "2026-04-10")]
    [InlineData("2025-06-01", "2026-12-31", "2026-01-15")]
    [InlineData("2026-01-01", "2026-01-02", "2026-01-01")]
    public async Task LoadAsync_TimelineDates_DeserializedCorrectly(string start, string end, string now)
    {
        var data = CreateValidDataWith(new
        {
            startDate = start,
            endDate = end,
            nowDate = now,
            tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(start, svc.Data!.Timeline.StartDate);
        Assert.Equal(end, svc.Data.Timeline.EndDate);
        Assert.Equal(now, svc.Data.Timeline.NowDate);
    }

    [Fact]
    public async Task LoadAsync_TimelineNowDateOptional_CanBeEmpty()
    {
        var data = CreateValidDataWith(new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            nowDate = "",
            tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        // NowDate is not validated as required, so empty is fine
        Assert.False(svc.IsError);
        Assert.Equal("", svc.Data!.Timeline.NowDate);
    }

    [Fact]
    public async Task LoadAsync_TimelineNowDateMissing_DefaultsToEmpty()
    {
        var json = """
        {
            "title": "T", "subtitle": "S", "backlogLink": "https://link",
            "currentMonth": "April", "months": ["April"],
            "timeline": {
                "startDate": "2026-01-01", "endDate": "2026-07-01",
                "tracks": [{"name":"M1","label":"L","color":"#000","milestones":[]}]
            },
            "heatmap": {"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(string.Empty, svc.Data!.Timeline.NowDate);
    }

    #endregion

    #region Track Data Contract

    [Fact]
    public async Task LoadAsync_SingleTrack_AllFieldsDeserialized()
    {
        var data = CreateValidDataWith(new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            nowDate = "2026-04-10",
            tracks = new[]
            {
                new
                {
                    name = "M1",
                    label = "Core Platform",
                    color = "#4285F4",
                    milestones = new[]
                    {
                        new { date = "2026-02-15", type = "poc", label = "Feb 15 PoC" },
                        new { date = "2026-05-01", type = "production", label = "May 1 GA" },
                        new { date = "2026-03-10", type = "checkpoint", label = "Mar 10 Check" }
                    }
                }
            }
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        var track = svc.Data!.Timeline.Tracks[0];
        Assert.Equal("M1", track.Name);
        Assert.Equal("Core Platform", track.Label);
        Assert.Equal("#4285F4", track.Color);
        Assert.Equal(3, track.Milestones.Count);
        Assert.Equal("poc", track.Milestones[0].Type);
        Assert.Equal("production", track.Milestones[1].Type);
        Assert.Equal("checkpoint", track.Milestones[2].Type);
        Assert.Equal("Feb 15 PoC", track.Milestones[0].Label);
    }

    [Fact]
    public async Task LoadAsync_MultipleTracks_PreservesOrder()
    {
        var data = CreateValidDataWith(new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            nowDate = "2026-04-10",
            tracks = new[]
            {
                new { name = "M1", label = "Core", color = "#4285F4", milestones = Array.Empty<object>() },
                new { name = "M2", label = "Data", color = "#EA4335", milestones = Array.Empty<object>() },
                new { name = "M3", label = "UI", color = "#34A853", milestones = Array.Empty<object>() },
                new { name = "M4", label = "Infra", color = "#F4B400", milestones = Array.Empty<object>() },
                new { name = "M5", label = "API", color = "#0078D4", milestones = Array.Empty<object>() }
            }
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(5, svc.Data!.Timeline.Tracks.Count);
        Assert.Equal("M1", svc.Data.Timeline.Tracks[0].Name);
        Assert.Equal("M3", svc.Data.Timeline.Tracks[2].Name);
        Assert.Equal("M5", svc.Data.Timeline.Tracks[4].Name);
    }

    [Fact]
    public async Task LoadAsync_TrackWithEmptyMilestones_IsValid()
    {
        var data = CreateValidDataWith(new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            nowDate = "2026-04-10",
            tracks = new[]
            {
                new { name = "M1", label = "Empty Track", color = "#000", milestones = Array.Empty<object>() }
            }
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Empty(svc.Data!.Timeline.Tracks[0].Milestones);
    }

    [Theory]
    [InlineData("#4285F4")]
    [InlineData("#EA4335")]
    [InlineData("#34A853")]
    [InlineData("#F4B400")]
    [InlineData("rgb(66, 133, 244)")]
    [InlineData("red")]
    public async Task LoadAsync_TrackColors_PreservedExactly(string color)
    {
        var data = CreateValidDataWith(new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            nowDate = "2026-04-10",
            tracks = new[] { new { name = "M1", label = "L", color, milestones = Array.Empty<object>() } }
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(color, svc.Data!.Timeline.Tracks[0].Color);
    }

    #endregion

    #region Timeline Validation Failures

    [Fact]
    public async Task LoadAsync_MissingTimelineStartDate_ValidationError()
    {
        var data = CreateValidDataWith(new
        {
            startDate = "",
            endDate = "2026-07-01",
            tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("startDate", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingTimelineEndDate_ValidationError()
    {
        var data = CreateValidDataWith(new
        {
            startDate = "2026-01-01",
            endDate = "",
            tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("endDate", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_EmptyTracks_ValidationError()
    {
        var data = CreateValidDataWith(new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            tracks = Array.Empty<object>()
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("tracks", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_NullTimeline_ValidationError()
    {
        var json = """
        {
            "title": "T", "subtitle": "S", "backlogLink": "https://link",
            "currentMonth": "April", "months": ["April"],
            "timeline": null,
            "heatmap": {"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("timeline", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Milestone Type Variations

    [Theory]
    [InlineData("poc")]
    [InlineData("production")]
    [InlineData("checkpoint")]
    public async Task LoadAsync_MilestoneType_PreservedCorrectly(string type)
    {
        var data = CreateValidDataWith(new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            nowDate = "2026-04-10",
            tracks = new[]
            {
                new
                {
                    name = "M1", label = "L", color = "#000",
                    milestones = new[] { new { date = "2026-03-01", type, label = $"Test {type}" } }
                }
            }
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(type, svc.Data!.Timeline.Tracks[0].Milestones[0].Type);
        Assert.Equal($"Test {type}", svc.Data.Timeline.Tracks[0].Milestones[0].Label);
    }

    [Fact]
    public async Task LoadAsync_MilestoneWithSpecialCharsInLabel_PreservedCorrectly()
    {
        var label = "Release <v2.0> & \"Final\"";
        var data = CreateValidDataWith(new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            nowDate = "2026-04-10",
            tracks = new[]
            {
                new
                {
                    name = "M1", label = "L", color = "#000",
                    milestones = new[] { new { date = "2026-03-01", type = "poc", label } }
                }
            }
        });
        var path = WriteJsonObj(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(label, svc.Data!.Timeline.Tracks[0].Milestones[0].Label);
    }

    #endregion
}