using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Integration tests verifying the full pipeline from JSON deserialization
/// to model construction, ensuring the data.json contract is correctly
/// consumed by the TimelineData model hierarchy.
/// </summary>
[Trait("Category", "Integration")]
public class TimelineJsonIntegrationTests
{
    [Fact]
    public void FullDashboardJson_Deserializes_TimelineWithAllFields()
    {
        var json = """
        {
            "title": "Executive Reporting Dashboard",
            "subtitle": "Platform Engineering · AI Workstream · April 2026",
            "backlogLink": "https://dev.azure.com/project/backlog",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "id": "M1",
                        "name": "Chatbot & MS Role",
                        "color": "#0078D4",
                        "milestones": [
                            { "date": "2026-03-26", "label": "PoC Complete", "type": "poc" },
                            { "date": "2026-05-20", "label": "Production Release", "type": "production" },
                            { "date": "2026-02-15", "label": "Design Review", "type": "checkpoint" }
                        ]
                    },
                    {
                        "id": "M2",
                        "name": "Backend Services",
                        "color": "#00897B",
                        "milestones": [
                            { "date": "2026-02-28", "label": "API PoC", "type": "poc" },
                            { "date": "2026-06-01", "label": "GA", "type": "production" }
                        ]
                    },
                    {
                        "id": "M3",
                        "name": "Infrastructure",
                        "color": "#546E7A",
                        "milestones": [
                            { "date": "2026-04-15", "label": "Checkpoint", "type": "checkpoint" },
                            { "date": "2026-05-30", "label": "Deploy", "type": "production" }
                        ]
                    }
                ]
            },
            "heatmap": {
                "shipped": { "Jan": ["Feature A"], "Feb": ["Feature B"] },
                "inProgress": { "Mar": ["Feature C"] },
                "carryover": {},
                "blockers": {}
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Timeline.Should().NotBeNull();
        data.Timeline.StartDate.Should().Be("2026-01-01");
        data.Timeline.EndDate.Should().Be("2026-06-30");
        data.Timeline.NowDate.Should().Be("2026-04-10");
        data.Timeline.Tracks.Should().HaveCount(3);
    }

    [Fact]
    public void FullDashboardJson_Track1_HasCorrectMilestones()
    {
        var json = CreateSampleDashboardJson();
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;

        var track1 = data.Timeline.Tracks[0];
        track1.Id.Should().Be("M1");
        track1.Name.Should().Be("Chatbot & MS Role");
        track1.Color.Should().Be("#0078D4");
        track1.Milestones.Should().HaveCount(3);

        track1.Milestones[0].Type.Should().Be("poc");
        track1.Milestones[0].Label.Should().Be("PoC Complete");

        track1.Milestones[1].Type.Should().Be("production");
        track1.Milestones[2].Type.Should().Be("checkpoint");
    }

    [Fact]
    public void FullDashboardJson_AllTrackColors_AreDistinct()
    {
        var json = CreateSampleDashboardJson();
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;

        var colors = data.Timeline.Tracks.Select(t => t.Color).ToList();
        colors.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void FullDashboardJson_AllMilestoneDates_AreValidIso8601()
    {
        var json = CreateSampleDashboardJson();
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;

        foreach (var track in data.Timeline.Tracks)
        {
            foreach (var ms in track.Milestones)
            {
                DateTime.TryParse(ms.Date, out var dt).Should().BeTrue(
                    $"Milestone '{ms.Label}' on track '{track.Id}' has invalid date '{ms.Date}'");
                dt.Should().BeAfter(new DateTime(2025, 1, 1));
                dt.Should().BeBefore(new DateTime(2027, 12, 31));
            }
        }
    }

    [Fact]
    public void FullDashboardJson_AllMilestoneTypes_AreKnownValues()
    {
        var json = CreateSampleDashboardJson();
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;
        var validTypes = new[] { "poc", "production", "checkpoint" };

        foreach (var track in data.Timeline.Tracks)
        {
            foreach (var ms in track.Milestones)
            {
                validTypes.Should().Contain(ms.Type,
                    $"Milestone '{ms.Label}' has unknown type '{ms.Type}'");
            }
        }
    }

    [Fact]
    public void FullDashboardJson_TimelineDates_AreInOrder()
    {
        var json = CreateSampleDashboardJson();
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;

        var start = DateTime.Parse(data.Timeline.StartDate);
        var end = DateTime.Parse(data.Timeline.EndDate);
        var now = DateTime.Parse(data.Timeline.NowDate);

        start.Should().BeBefore(end);
        now.Should().BeOnOrAfter(start);
        now.Should().BeOnOrBefore(end);
    }

    [Fact]
    public void FullDashboardJson_SerializeAndDeserialize_RoundTrip()
    {
        var json = CreateSampleDashboardJson();
        var original = JsonSerializer.Deserialize<DashboardData>(json)!;

        var serialized = JsonSerializer.Serialize(original);
        var roundTripped = JsonSerializer.Deserialize<DashboardData>(serialized)!;

        roundTripped.Timeline.StartDate.Should().Be(original.Timeline.StartDate);
        roundTripped.Timeline.EndDate.Should().Be(original.Timeline.EndDate);
        roundTripped.Timeline.NowDate.Should().Be(original.Timeline.NowDate);
        roundTripped.Timeline.Tracks.Should().HaveCount(original.Timeline.Tracks.Count);

        for (int i = 0; i < original.Timeline.Tracks.Count; i++)
        {
            roundTripped.Timeline.Tracks[i].Id.Should().Be(original.Timeline.Tracks[i].Id);
            roundTripped.Timeline.Tracks[i].Milestones.Should()
                .HaveCount(original.Timeline.Tracks[i].Milestones.Count);
        }
    }

    [Fact]
    public void MinimalJson_TimelineWithNoTracks_DeserializesCleanly()
    {
        var json = """
        {
            "title": "Minimal",
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": []
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Timeline.Tracks.Should().BeEmpty();
    }

    [Fact]
    public void Json_TrackWithEmptyMilestones_DeserializesCleanly()
    {
        var json = """
        {
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "id": "M1",
                        "name": "Empty",
                        "color": "#0078D4",
                        "milestones": []
                    }
                ]
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data!.Timeline.Tracks.Should().HaveCount(1);
        data.Timeline.Tracks[0].Milestones.Should().BeEmpty();
    }

    [Fact]
    public void Json_MissingOptionalFields_DefaultsApply()
    {
        var json = """
        {
            "timeline": {
                "tracks": [
                    {
                        "id": "M1",
                        "name": "Track",
                        "milestones": [
                            { "date": "2026-03-15", "label": "Milestone" }
                        ]
                    }
                ]
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json)!;

        // Timeline dates default to empty
        data.Timeline.StartDate.Should().BeEmpty();
        data.Timeline.EndDate.Should().BeEmpty();

        // Track color defaults to blue
        data.Timeline.Tracks[0].Color.Should().Be("#0078D4");

        // Milestone type defaults to checkpoint
        data.Timeline.Tracks[0].Milestones[0].Type.Should().Be("checkpoint");
    }

    [Fact]
    public void Json_ExtraUnknownFields_AreIgnored()
    {
        var json = """
        {
            "title": "Test",
            "unknownField": "should be ignored",
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "extraField": true,
                "tracks": [
                    {
                        "id": "M1",
                        "name": "Track",
                        "color": "#0078D4",
                        "priority": 1,
                        "milestones": []
                    }
                ]
            }
        }
        """;

        var action = () => JsonSerializer.Deserialize<DashboardData>(json);

        action.Should().NotThrow();
        var data = action()!;
        data.Timeline.Tracks.Should().HaveCount(1);
    }

    private static string CreateSampleDashboardJson() => """
        {
            "title": "Executive Reporting Dashboard",
            "subtitle": "Platform Engineering · AI Workstream · April 2026",
            "backlogLink": "https://dev.azure.com/project/backlog",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "id": "M1",
                        "name": "Chatbot & MS Role",
                        "color": "#0078D4",
                        "milestones": [
                            { "date": "2026-03-26", "label": "PoC Complete", "type": "poc" },
                            { "date": "2026-05-20", "label": "Production Release", "type": "production" },
                            { "date": "2026-02-15", "label": "Design Review", "type": "checkpoint" }
                        ]
                    },
                    {
                        "id": "M2",
                        "name": "Backend Services",
                        "color": "#00897B",
                        "milestones": [
                            { "date": "2026-02-28", "label": "API PoC", "type": "poc" },
                            { "date": "2026-06-01", "label": "GA", "type": "production" }
                        ]
                    },
                    {
                        "id": "M3",
                        "name": "Infrastructure",
                        "color": "#546E7A",
                        "milestones": [
                            { "date": "2026-04-15", "label": "Checkpoint", "type": "checkpoint" },
                            { "date": "2026-05-30", "label": "Deploy", "type": "production" }
                        ]
                    }
                ]
            },
            "heatmap": {
                "shipped": { "Jan": ["Feature A"], "Feb": ["Feature B"] },
                "inProgress": { "Mar": ["Feature C"] },
                "carryover": {},
                "blockers": {}
            }
        }
        """;
}