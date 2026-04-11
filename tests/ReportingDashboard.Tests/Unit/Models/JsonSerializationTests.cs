using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class JsonSerializationTests
{
    [Fact]
    public void TimelineData_Deserialize_ValidJson_ReturnsCorrectObject()
    {
        var json = """
        {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "id": "M1",
                    "name": "Chatbot & MS Role",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-03-26", "label": "PoC", "type": "poc" },
                        { "date": "2026-05-20", "label": "Prod", "type": "production" }
                    ]
                }
            ]
        }
        """;

        var result = JsonSerializer.Deserialize<TimelineData>(json);

        result.Should().NotBeNull();
        result!.StartDate.Should().Be("2026-01-01");
        result.EndDate.Should().Be("2026-06-30");
        result.NowDate.Should().Be("2026-04-10");
        result.Tracks.Should().HaveCount(1);
        result.Tracks[0].Id.Should().Be("M1");
        result.Tracks[0].Milestones.Should().HaveCount(2);
        result.Tracks[0].Milestones[0].Type.Should().Be("poc");
    }

    [Fact]
    public void TimelineData_Deserialize_EmptyTracks_ReturnsEmptyList()
    {
        var json = """
        {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": []
        }
        """;

        var result = JsonSerializer.Deserialize<TimelineData>(json);

        result.Should().NotBeNull();
        result!.Tracks.Should().BeEmpty();
    }

    [Fact]
    public void TimelineData_Deserialize_MissingFields_UsesDefaults()
    {
        var json = "{}";

        var result = JsonSerializer.Deserialize<TimelineData>(json);

        result.Should().NotBeNull();
        result!.StartDate.Should().BeEmpty();
        result.EndDate.Should().BeEmpty();
        result.NowDate.Should().BeEmpty();
        result.Tracks.Should().NotBeNull();
    }

    [Fact]
    public void MilestoneMarker_Deserialize_AllTypes()
    {
        var pocJson = """{"date":"2026-03-15","label":"PoC","type":"poc"}""";
        var prodJson = """{"date":"2026-05-01","label":"GA","type":"production"}""";
        var cpJson = """{"date":"2026-04-01","label":"Review","type":"checkpoint"}""";

        var poc = JsonSerializer.Deserialize<MilestoneMarker>(pocJson);
        var prod = JsonSerializer.Deserialize<MilestoneMarker>(prodJson);
        var cp = JsonSerializer.Deserialize<MilestoneMarker>(cpJson);

        poc!.Type.Should().Be("poc");
        prod!.Type.Should().Be("production");
        cp!.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void TimelineTrack_Deserialize_NoMilestones_DefaultsToEmpty()
    {
        var json = """{"id":"M1","name":"Test","color":"#FF0000"}""";

        var result = JsonSerializer.Deserialize<TimelineTrack>(json);

        result.Should().NotBeNull();
        result!.Id.Should().Be("M1");
        result.Milestones.Should().NotBeNull();
    }

    [Fact]
    public void DashboardData_Deserialize_FullJson_AllPropertiesPopulated()
    {
        var json = """
        {
            "title": "Executive Dashboard",
            "subtitle": "Q2 2026",
            "backlogLink": "https://dev.azure.com",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "id": "M1",
                        "name": "Track 1",
                        "color": "#0078D4",
                        "milestones": [
                            { "date": "2026-03-15", "label": "PoC", "type": "poc" }
                        ]
                    }
                ]
            },
            "heatmap": {
                "shipped": { "Jan": ["Feature A"] },
                "inProgress": { "Feb": ["Feature B"] },
                "carryover": {},
                "blockers": {}
            }
        }
        """;

        var result = JsonSerializer.Deserialize<DashboardData>(json);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Executive Dashboard");
        result.Months.Should().HaveCount(4);
        result.Timeline.Tracks.Should().HaveCount(1);
        result.Heatmap.Shipped.Should().ContainKey("Jan");
        result.Heatmap.InProgress.Should().ContainKey("Feb");
    }

    [Fact]
    public void TimelineData_Serialize_RoundTrip_PreservesData()
    {
        var original = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1",
                    Name = "Track 1",
                    Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "Test", Type = "poc" }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<TimelineData>(json);

        deserialized.Should().NotBeNull();
        deserialized!.StartDate.Should().Be(original.StartDate);
        deserialized.Tracks.Should().HaveCount(1);
        deserialized.Tracks[0].Milestones[0].Label.Should().Be("Test");
    }

    [Fact]
    public void MilestoneMarker_Deserialize_MissingType_DefaultsToCheckpoint()
    {
        var json = """{"date":"2026-04-01","label":"Review"}""";

        var result = JsonSerializer.Deserialize<MilestoneMarker>(json);

        result.Should().NotBeNull();
        result!.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void TimelineTrack_Deserialize_MissingColor_DefaultsToBlue()
    {
        var json = """{"id":"M1","name":"Test"}""";

        var result = JsonSerializer.Deserialize<TimelineTrack>(json);

        result.Should().NotBeNull();
        result!.Color.Should().Be("#0078D4");
    }

    [Fact]
    public void HeatmapData_Deserialize_NestedArrays_Preserved()
    {
        var json = """
        {
            "shipped": {
                "Jan": ["Feature A", "Feature B"],
                "Feb": ["Feature C"]
            },
            "inProgress": {},
            "carryover": {},
            "blockers": { "Mar": ["Blocker 1"] }
        }
        """;

        var result = JsonSerializer.Deserialize<HeatmapData>(json);

        result.Should().NotBeNull();
        result!.Shipped["Jan"].Should().HaveCount(2);
        result.Shipped["Feb"].Should().HaveCount(1);
        result.Blockers["Mar"].Should().ContainSingle().Which.Should().Be("Blocker 1");
    }

    [Fact]
    public void DashboardData_Deserialize_MultipleTracks_PreservesOrder()
    {
        var json = """
        {
            "title": "Test",
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    { "id": "M1", "name": "First", "color": "#0078D4", "milestones": [] },
                    { "id": "M2", "name": "Second", "color": "#00897B", "milestones": [] },
                    { "id": "M3", "name": "Third", "color": "#546E7A", "milestones": [] }
                ]
            }
        }
        """;

        var result = JsonSerializer.Deserialize<DashboardData>(json);

        result!.Timeline.Tracks.Should().HaveCount(3);
        result.Timeline.Tracks[0].Id.Should().Be("M1");
        result.Timeline.Tracks[1].Id.Should().Be("M2");
        result.Timeline.Tracks[2].Id.Should().Be("M3");
    }
}