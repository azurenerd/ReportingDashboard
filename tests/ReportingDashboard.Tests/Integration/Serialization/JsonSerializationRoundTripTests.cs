using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Serialization;

/// <summary>
/// Tests that the DashboardData model correctly round-trips through JSON serialization,
/// ensuring the JsonPropertyName attributes and default values work as expected.
/// </summary>
[Trait("Category", "Integration")]
public class JsonSerializationRoundTripTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void DashboardData_RoundTrip_ShouldPreserveAllFields()
    {
        var original = new DashboardData
        {
            Title = "Round Trip Test",
            Subtitle = "Team A - April",
            BacklogLink = "https://example.com/backlog",
            CurrentMonth = "Apr",
            Months = new List<string> { "Jan", "Feb", "Mar", "Apr" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-06-30",
                NowDate = "2026-04-10",
                Tracks = new List<TimelineTrack>
                {
                    new()
                    {
                        Id = "M1", Name = "Platform", Color = "#0078D4",
                        Milestones = new List<MilestoneMarker>
                        {
                            new() { Date = "2026-02-15", Label = "Feb PoC", Type = "poc" },
                            new() { Date = "2026-04-01", Label = "Apr GA", Type = "production" }
                        }
                    }
                }
            },
            Heatmap = new HeatmapData
            {
                Shipped = new() { ["Jan"] = new() { "A", "B" } },
                InProgress = new() { ["Feb"] = new() { "C" } },
                Carryover = new() { ["Mar"] = new() { "D" } },
                Blockers = new() { ["Apr"] = new() { "E" } }
            }
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json, Options);

        deserialized.Should().NotBeNull();
        deserialized!.Title.Should().Be(original.Title);
        deserialized.Subtitle.Should().Be(original.Subtitle);
        deserialized.BacklogLink.Should().Be(original.BacklogLink);
        deserialized.CurrentMonth.Should().Be(original.CurrentMonth);
        deserialized.Months.Should().BeEquivalentTo(original.Months);

        deserialized.Timeline.StartDate.Should().Be(original.Timeline.StartDate);
        deserialized.Timeline.EndDate.Should().Be(original.Timeline.EndDate);
        deserialized.Timeline.NowDate.Should().Be(original.Timeline.NowDate);
        deserialized.Timeline.Tracks.Should().HaveCount(1);
        deserialized.Timeline.Tracks[0].Milestones.Should().HaveCount(2);

        deserialized.Heatmap.Shipped["Jan"].Should().HaveCount(2);
        deserialized.Heatmap.InProgress["Feb"].Should().ContainSingle();
        deserialized.Heatmap.Blockers["Apr"].Should().ContainSingle();
    }

    [Fact]
    public void DashboardData_DeserializeFromCamelCase_ShouldWork()
    {
        var json = """
        {
            "title": "camelCase test",
            "subtitle": "sub",
            "backlogLink": "https://link",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-01-15",
                "tracks": [{
                    "id": "M1",
                    "name": "Track",
                    "color": "#000",
                    "milestones": [{
                        "date": "2026-02-01",
                        "label": "Test",
                        "type": "poc"
                    }]
                }]
            },
            "heatmap": {
                "shipped": {},
                "inProgress": { "Jan": ["Item"] },
                "carryover": {},
                "blockers": {}
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data.Should().NotBeNull();
        data!.Title.Should().Be("camelCase test");
        data.Timeline.Tracks[0].Milestones[0].Type.Should().Be("poc");
        data.Heatmap.InProgress["Jan"].Should().ContainSingle();
    }

    [Fact]
    public void DashboardData_DeserializeFromPascalCase_ShouldWorkWithCaseInsensitive()
    {
        var json = """
        {
            "Title": "PascalCase test",
            "Subtitle": "sub",
            "BacklogLink": "https://link",
            "CurrentMonth": "Feb"
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data.Should().NotBeNull();
        data!.Title.Should().Be("PascalCase test");
        data.CurrentMonth.Should().Be("Feb");
    }

    [Fact]
    public void HeatmapData_EmptyCategories_ShouldDeserializeAsEmptyDictionaries()
    {
        var json = """
        {
            "shipped": {},
            "inProgress": {},
            "carryover": {},
            "blockers": {}
        }
        """;

        var heatmap = JsonSerializer.Deserialize<HeatmapData>(json, Options);

        heatmap.Should().NotBeNull();
        heatmap!.Shipped.Should().BeEmpty();
        heatmap.InProgress.Should().BeEmpty();
        heatmap.Carryover.Should().BeEmpty();
        heatmap.Blockers.Should().BeEmpty();
    }

    [Fact]
    public void TimelineTrack_DefaultColor_ShouldSurviveRoundTrip()
    {
        var track = new TimelineTrack { Id = "M1", Name = "Test" };
        // Don't set color - should use default

        var json = JsonSerializer.Serialize(track);
        var deserialized = JsonSerializer.Deserialize<TimelineTrack>(json, Options);

        deserialized!.Color.Should().Be("#0078D4");
    }

    [Fact]
    public void MilestoneMarker_DefaultType_ShouldSurviveRoundTrip()
    {
        var marker = new MilestoneMarker { Date = "2026-01-01", Label = "Test" };
        // Don't set type - should use default

        var json = JsonSerializer.Serialize(marker);
        var deserialized = JsonSerializer.Deserialize<MilestoneMarker>(json, Options);

        deserialized!.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void DashboardData_LargeNestedStructure_ShouldSerializeAndDeserialize()
    {
        var data = new DashboardData
        {
            Title = "Large",
            Months = Enumerable.Range(1, 12).Select(m => new DateTime(2026, m, 1).ToString("MMM")).ToList(),
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-12-31",
                NowDate = "2026-06-15",
                Tracks = Enumerable.Range(1, 20).Select(i => new TimelineTrack
                {
                    Id = $"M{i}",
                    Name = $"Track {i}",
                    Color = $"#{i:X2}{i:X2}{i:X2}",
                    Milestones = Enumerable.Range(1, 5).Select(j => new MilestoneMarker
                    {
                        Date = $"2026-{j:D2}-15",
                        Label = $"MS {i}.{j}",
                        Type = j % 3 == 0 ? "poc" : j % 3 == 1 ? "production" : "checkpoint"
                    }).ToList()
                }).ToList()
            },
            Heatmap = new HeatmapData
            {
                Shipped = Enumerable.Range(1, 6).ToDictionary(
                    m => new DateTime(2026, m, 1).ToString("MMM"),
                    m => Enumerable.Range(1, 3).Select(i => $"Shipped-{m}-{i}").ToList()),
                InProgress = new(),
                Carryover = new(),
                Blockers = new()
            }
        };

        var json = JsonSerializer.Serialize(data);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json, Options);

        deserialized.Should().NotBeNull();
        deserialized!.Months.Should().HaveCount(12);
        deserialized.Timeline.Tracks.Should().HaveCount(20);
        deserialized.Timeline.Tracks[0].Milestones.Should().HaveCount(5);
        deserialized.Heatmap.Shipped.Should().HaveCount(6);
    }
}