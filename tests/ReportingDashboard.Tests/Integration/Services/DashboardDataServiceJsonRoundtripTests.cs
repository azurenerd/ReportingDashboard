using System.Text.Json;
using FluentAssertions;
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

        var mockLogger = new Mock<ILogger<DashboardDataService>>();
        _service = new DashboardDataService(mockLogger.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteJson(string content)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
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
                        Name = "Feature A",
                        Label = "T1",
                        Color = "#FF0000",
                        Milestones = new List<Milestone>
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
        var path = WriteJson(json);

        await _service.LoadAsync(path);

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
    public async Task Roundtrip_SpecialCharacters_PreservedThroughSerialization()
    {
        var original = new DashboardData
        {
            Title = "Dashboard with \"quotes\" & <brackets>",
            Subtitle = "Unicode: éèê — ñ",
            BacklogLink = "https://example.com",
            CurrentMonth = "Jan",
            Months = new List<string> { "Jan" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-06-30",
                NowDate = "2026-03-15",
                Tracks = new List<TimelineTrack>
                {
                    new() { Name = "Track", Label = "T1", Milestones = new List<Milestone>() }
                }
            },
            Heatmap = new HeatmapData()
        };

        var json = JsonSerializer.Serialize(original);
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.Data!.Title.Should().Contain("quotes");
        _service.Data.Title.Should().Contain("&");
    }

    [Fact]
    public async Task Roundtrip_ManyTracks_AllPreserved()
    {
        var tracks = Enumerable.Range(1, 10).Select(i => new TimelineTrack
        {
            Name = $"Track {i}",
            Label = $"M{i}",
            Color = $"#00{i:D2}00",
            Milestones = new List<Milestone>
            {
                new() { Date = $"2026-{i:D2}-15", Label = $"MS {i}", Type = i % 3 == 0 ? "production" : i % 3 == 1 ? "poc" : "checkpoint" }
            }
        }).ToList();

        var original = new DashboardData
        {
            Title = "Many Tracks",
            Subtitle = "Test Subtitle",
            BacklogLink = "https://example.com",
            CurrentMonth = "Jan",
            Months = new List<string> { "Jan" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-12-31",
                NowDate = "2026-06-15",
                Tracks = tracks
            },
            Heatmap = new HeatmapData()
        };

        var json = JsonSerializer.Serialize(original);
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.Data!.Timeline.Tracks.Should().HaveCount(10);
        for (int i = 0; i < 10; i++)
        {
            _service.Data.Timeline.Tracks[i].Name.Should().Be($"Track {i + 1}");
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
            Subtitle = "Test Subtitle",
            BacklogLink = "https://example.com",
            CurrentMonth = "Jan",
            Months = new List<string> { "Jan" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-06-30",
                NowDate = "2026-03-15",
                Tracks = new List<TimelineTrack>
                {
                    new() { Name = "Track", Label = "T1", Milestones = new List<Milestone>() }
                }
            },
            Heatmap = new HeatmapData { Shipped = shipped }
        };

        var json = JsonSerializer.Serialize(original);
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.Data!.Heatmap.Shipped.Should().HaveCount(6);
        foreach (var month in months)
        {
            _service.Data.Heatmap.Shipped[month].Should().HaveCount(20);
        }
    }
}