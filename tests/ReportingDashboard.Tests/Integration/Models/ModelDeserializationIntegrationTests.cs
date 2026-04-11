using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Models;

/// <summary>
/// Integration tests for model deserialization using the same JsonSerializerOptions
/// as the production DashboardDataService, exercising camelCase mapping, trailing commas,
/// comments, missing fields, and null-safe defaults through the full System.Text.Json pipeline.
/// </summary>
[Trait("Category", "Integration")]
public class ModelDeserializationIntegrationTests
{
    private static readonly JsonSerializerOptions ProductionOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    // --- DashboardData camelCase mapping ---

    [Fact]
    public void DashboardData_CamelCaseJson_AllPropertiesMapped()
    {
        var json = """
        {
            "title": "Integration Test",
            "subtitle": "Team Z - March 2026",
            "backlogLink": "https://dev.azure.com/org",
            "currentMonth": "Mar",
            "months": ["Jan", "Feb", "Mar"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-12-31",
                "nowDate": "2026-03-15",
                "tracks": []
            },
            "heatmap": {
                "shipped": { "jan": ["SDK"] },
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, ProductionOptions)!;

        data.Title.Should().Be("Integration Test");
        data.Subtitle.Should().Be("Team Z - March 2026");
        data.BacklogLink.Should().Be("https://dev.azure.com/org");
        data.CurrentMonth.Should().Be("Mar");
        data.Months.Should().HaveCount(3);
        data.Timeline.StartDate.Should().Be("2026-01-01");
        data.Heatmap.Shipped["jan"].Should().Contain("SDK");
    }

    [Fact]
    public void DashboardData_PascalCaseKeys_NotMapped()
    {
        var json = """{ "Title": "Wrong", "Subtitle": "Wrong" }""";

        var data = JsonSerializer.Deserialize<DashboardData>(json, ProductionOptions)!;

        data.Title.Should().BeEmpty("PascalCase should not map with case-sensitive options");
        data.Subtitle.Should().BeEmpty();
    }

    [Fact]
    public void DashboardData_ExtraFields_Ignored()
    {
        var json = """
        {
            "title": "Test",
            "unknownField": 42,
            "deeply": { "nested": { "extra": true } }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, ProductionOptions)!;

        data.Title.Should().Be("Test");
    }

    [Fact]
    public void DashboardData_EmptyObject_DefaultsApplied()
    {
        var data = JsonSerializer.Deserialize<DashboardData>("{}", ProductionOptions)!;

        data.Title.Should().BeEmpty();
        data.Months.Should().NotBeNull().And.BeEmpty();
        data.Timeline.Should().NotBeNull();
        data.Timeline.Tracks.Should().NotBeNull().And.BeEmpty();
        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Shipped.Should().NotBeNull().And.BeEmpty();
        data.Heatmap.InProgress.Should().NotBeNull().And.BeEmpty();
        data.Heatmap.Carryover.Should().NotBeNull().And.BeEmpty();
        data.Heatmap.Blockers.Should().NotBeNull().And.BeEmpty();
    }

    // --- HeatmapData null-safety ---

    [Fact]
    public void HeatmapData_MissingCategories_DefaultToEmptyDictionaries()
    {
        var json = """{ "shipped": { "jan": ["Done"] } }""";

        var data = JsonSerializer.Deserialize<HeatmapData>(json, ProductionOptions)!;

        data.Shipped.Should().ContainKey("jan");
        data.InProgress.Should().NotBeNull().And.BeEmpty();
        data.Carryover.Should().NotBeNull().And.BeEmpty();
        data.Blockers.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void HeatmapData_EmptyListsPerMonth_DeserializesCorrectly()
    {
        var json = """
        {
            "shipped": { "jan": [], "feb": [] },
            "inProgress": { "mar": [] },
            "carryover": {},
            "blockers": {}
        }
        """;

        var data = JsonSerializer.Deserialize<HeatmapData>(json, ProductionOptions)!;

        data.Shipped["jan"].Should().BeEmpty();
        data.Shipped["feb"].Should().BeEmpty();
        data.InProgress["mar"].Should().BeEmpty();
    }

    // --- TimelineTrack defaults ---

    [Fact]
    public void TimelineTrack_MissingColor_DefaultsToGray()
    {
        var json = """{ "name": "Track A", "label": "T1", "milestones": [] }""";

        var track = JsonSerializer.Deserialize<TimelineTrack>(json, ProductionOptions)!;

        track.Color.Should().Be("#999");
    }

    [Fact]
    public void TimelineTrack_WithColor_OverridesDefault()
    {
        var json = """{ "name": "Track A", "label": "T1", "color": "#FF0000", "milestones": [] }""";

        var track = JsonSerializer.Deserialize<TimelineTrack>(json, ProductionOptions)!;

        track.Color.Should().Be("#FF0000");
    }

    // --- Milestone type defaults ---

    [Fact]
    public void Milestone_MissingType_DefaultsToCheckpoint()
    {
        var json = """{ "date": "2026-05-01", "label": "May 1" }""";

        var milestone = JsonSerializer.Deserialize<Milestone>(json, ProductionOptions)!;

        milestone.Type.Should().Be("checkpoint");
    }

    [Theory]
    [InlineData("checkpoint")]
    [InlineData("poc")]
    [InlineData("production")]
    public void Milestone_AllValidTypes_DeserializeCorrectly(string type)
    {
        var json = $$"""{ "date": "2026-03-01", "type": "{{type}}", "label": "Mar 1" }""";

        var milestone = JsonSerializer.Deserialize<Milestone>(json, ProductionOptions)!;

        milestone.Type.Should().Be(type);
    }

    // --- Trailing commas and comments ---

    [Fact]
    public void DashboardData_TrailingCommas_Accepted()
    {
        var json = """
        {
            "title": "Trailing",
            "months": ["Jan", "Feb",],
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, ProductionOptions)!;

        data.Title.Should().Be("Trailing");
        data.Months.Should().HaveCount(2);
    }

    [Fact]
    public void DashboardData_Comments_Accepted()
    {
        var json = """
        {
            // Line comment
            "title": "Commented",
            /* Block comment */
            "subtitle": "Sub"
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, ProductionOptions)!;

        data.Title.Should().Be("Commented");
        data.Subtitle.Should().Be("Sub");
    }

    // --- Full roundtrip ---

    [Fact]
    public void Roundtrip_FullDashboardData_AllFieldsPreserved()
    {
        var original = new DashboardData
        {
            Title = "Roundtrip Integration",
            Subtitle = "Full Test",
            BacklogLink = "https://example.com/backlog",
            CurrentMonth = "Feb",
            Months = new List<string> { "Jan", "Feb" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-06-30",
                NowDate = "2026-02-15",
                Tracks = new List<TimelineTrack>
                {
                    new()
                    {
                        Name = "Feature Alpha",
                        Label = "M1",
                        Color = "#0078D4",
                        Milestones = new List<Milestone>
                        {
                            new() { Date = "2026-01-15", Label = "Jan 15", Type = "checkpoint" },
                            new() { Date = "2026-03-01", Label = "Mar 1", Type = "poc" },
                            new() { Date = "2026-05-01", Label = "May 1", Type = "production" }
                        }
                    },
                    new()
                    {
                        Name = "Feature Beta",
                        Label = "M2",
                        Color = "#00897B",
                        Milestones = new List<Milestone>
                        {
                            new() { Date = "2026-04-01", Label = "Apr 1", Type = "production" }
                        }
                    }
                }
            },
            Heatmap = new HeatmapData
            {
                Shipped = new Dictionary<string, List<string>>
                {
                    ["jan"] = new() { "Item 1", "Item 2" },
                    ["feb"] = new() { "Item 3" }
                },
                InProgress = new Dictionary<string, List<string>>
                {
                    ["feb"] = new() { "Item 4" }
                },
                Carryover = new Dictionary<string, List<string>>(),
                Blockers = new Dictionary<string, List<string>>
                {
                    ["feb"] = new() { "Blocker X" }
                }
            }
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json, ProductionOptions)!;

        deserialized.Title.Should().Be(original.Title);
        deserialized.Subtitle.Should().Be(original.Subtitle);
        deserialized.BacklogLink.Should().Be(original.BacklogLink);
        deserialized.CurrentMonth.Should().Be(original.CurrentMonth);
        deserialized.Months.Should().BeEquivalentTo(original.Months);
        deserialized.Timeline.Tracks.Should().HaveCount(2);
        deserialized.Timeline.Tracks[0].Milestones.Should().HaveCount(3);
        deserialized.Timeline.Tracks[1].Milestones.Should().HaveCount(1);
        deserialized.Heatmap.Shipped.Should().HaveCount(2);
        deserialized.Heatmap.Shipped["jan"].Should().HaveCount(2);
        deserialized.Heatmap.Blockers["feb"].Should().ContainSingle();
    }

    [Fact]
    public void Roundtrip_LargeTrackCount_AllPreserved()
    {
        var tracks = Enumerable.Range(1, 20).Select(i => new TimelineTrack
        {
            Name = $"Track {i}",
            Label = $"M{i}",
            Color = $"#{i:D2}{i:D2}{i:D2}",
            Milestones = new List<Milestone>
            {
                new() { Date = $"2026-{(i % 12) + 1:D2}-15", Label = $"MS{i}", Type = i % 3 == 0 ? "production" : i % 3 == 1 ? "poc" : "checkpoint" }
            }
        }).ToList();

        var original = new DashboardData
        {
            Title = "Many Tracks",
            Subtitle = "Scale Test",
            BacklogLink = "https://test.com",
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
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json, ProductionOptions)!;

        deserialized.Timeline.Tracks.Should().HaveCount(20);
        for (int i = 0; i < 20; i++)
        {
            deserialized.Timeline.Tracks[i].Name.Should().Be($"Track {i + 1}");
        }
    }
}