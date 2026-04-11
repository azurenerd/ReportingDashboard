using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Services;

[Trait("Category", "Integration")]
public class DashboardDataServiceJsonRoundtripTests : IDisposable
{
    private readonly string _tempDir;
    private readonly DashboardDataService _service;

    public DashboardDataServiceJsonRoundtripTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardRT_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(_tempDir);
        var mockLogger = new Mock<ILogger<DashboardDataService>>();
        _service = new DashboardDataService(mockEnv.Object, mockLogger.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteJson(string content)
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public async Task Roundtrip_SerializeAndDeserialize_ProducesEquivalentData()
    {
        var original = new DashboardData
        {
            Title = "Roundtrip Test",
            Subtitle = "Testing Serialization",
            BacklogLink = "https://example.com",
            CurrentMonth = "Mar",
            Months = new List<string> { "Jan", "Feb", "Mar" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-06-30",
                NowDate = "2026-03-15",
                Tracks = new List<TimelineTrack>
                {
                    new()
                    {
                        Id = "T1",
                        Name = "Feature A",
                        Color = "#FF0000",
                        Milestones = new List<MilestoneMarker>
                        {
                            new() { Date = "2026-02-01", Label = "Feb 1", Type = "poc" },
                            new() { Date = "2026-05-01", Label = "May 1", Type = "production" }
                        }
                    }
                }
            },
            Heatmap = new HeatmapData
            {
                Shipped = new Dictionary<string, List<string>>
                {
                    ["jan"] = new() { "Item 1", "Item 2" }
                },
                InProgress = new Dictionary<string, List<string>>
                {
                    ["mar"] = new() { "Item 3" }
                },
                Carryover = new Dictionary<string, List<string>>(),
                Blockers = new Dictionary<string, List<string>>
                {
                    ["mar"] = new() { "Blocker A" }
                }
            }
        };

        var json = JsonSerializer.Serialize(original);
        WriteJson(json);

        await _service.LoadAsync();

        _service.IsError.Should().BeFalse();
        var loaded = _service.Data!;

        loaded.Title.Should().Be(original.Title);
        loaded.Subtitle.Should().Be(original.Subtitle);
        loaded.BacklogLink.Should().Be(original.BacklogLink);
        loaded.CurrentMonth.Should().Be(original.CurrentMonth);
        loaded.Months.Should().BeEquivalentTo(original.Months);
        loaded.Timeline.Tracks.Should().HaveCount(1);
        loaded.Timeline.Tracks[0].Milestones.Should().HaveCount(2);
        loaded.Heatmap.Shipped["jan"].Should().HaveCount(2);
        loaded.Heatmap.Blockers["mar"].Should().ContainSingle();
    }

    [Fact]
    public async Task Roundtrip_EmptyCollections_SurviveRoundtrip()
    {
        var original = new DashboardData
        {
            Title = "Empty Collections",
            Months = new List<string>(),
            Timeline = new TimelineData { Tracks = new List<TimelineTrack>() },
            Heatmap = new HeatmapData()
        };

        var json = JsonSerializer.Serialize(original);
        WriteJson(json);

        await _service.LoadAsync();

        _service.Data!.Months.Should().BeEmpty();
        _service.Data.Timeline.Tracks.Should().BeEmpty();
        _service.Data.Heatmap.Shipped.Should().BeEmpty();
    }

    [Fact]
    public async Task Roundtrip_SpecialCharacters_PreservedThroughSerialization()
    {
        var original = new DashboardData
        {
            Title = "Dashboard with \"quotes\" & <brackets>",
            Subtitle = "Unicode: éèê — ñ"
        };

        var json = JsonSerializer.Serialize(original);
        WriteJson(json);

        await _service.LoadAsync();

        _service.Data!.Title.Should().Contain("quotes");
        _service.Data.Title.Should().Contain("&");
        _service.Data.Title.Should().Contain("<brackets>");
    }

    [Fact]
    public async Task Roundtrip_ManyTracks_AllPreserved()
    {
        var tracks = Enumerable.Range(1, 10).Select(i => new TimelineTrack
        {
            Id = $"M{i}",
            Name = $"Track {i}",
            Color = $"#00{i:D2}00",
            Milestones = new List<MilestoneMarker>
            {
                new() { Date = $"2026-{i:D2}-15", Label = $"MS {i}", Type = i % 3 == 0 ? "production" : i % 3 == 1 ? "poc" : "checkpoint" }
            }
        }).ToList();

        var original = new DashboardData
        {
            Title = "Many Tracks",
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-12-31",
                NowDate = "2026-06-15",
                Tracks = tracks
            }
        };

        var json = JsonSerializer.Serialize(original);
        WriteJson(json);

        await _service.LoadAsync();

        _service.Data!.Timeline.Tracks.Should().HaveCount(10);
        for (int i = 0; i < 10; i++)
        {
            _service.Data.Timeline.Tracks[i].Id.Should().Be($"M{i + 1}");
        }
    }

    [Fact]
    public async Task Roundtrip_LargeHeatmap_AllItemsPreserved()
    {
        var shipped = new Dictionary<string, List<string>>();
        var months = new[] { "jan", "feb", "mar", "apr", "may", "jun" };
        foreach (var month in months)
        {
            shipped[month] = Enumerable.Range(1, 20).Select(i => $"Item {month}-{i}").ToList();
        }

        var original = new DashboardData
        {
            Title = "Large Heatmap",
            Heatmap = new HeatmapData { Shipped = shipped }
        };

        var json = JsonSerializer.Serialize(original);
        WriteJson(json);

        await _service.LoadAsync();

        _service.Data!.Heatmap.Shipped.Should().HaveCount(6);
        foreach (var month in months)
        {
            _service.Data.Heatmap.Shipped[month].Should().HaveCount(20);
        }
    }
}