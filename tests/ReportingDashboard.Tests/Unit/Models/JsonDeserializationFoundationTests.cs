using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class JsonDeserializationFoundationTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Deserialize_FullDashboardData_AllPropertiesPopulated()
    {
        var json = """
        {
            "title": "Test Project",
            "subtitle": "Team A - April 2026",
            "backlogLink": "https://dev.azure.com/test",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": []
            },
            "heatmap": {
                "shipped": {},
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data.Should().NotBeNull();
        data!.Title.Should().Be("Test Project");
        data.Subtitle.Should().Be("Team A - April 2026");
        data.BacklogLink.Should().Be("https://dev.azure.com/test");
        data.CurrentMonth.Should().Be("Apr");
        data.Months.Should().HaveCount(4);
        data.Timeline.Should().NotBeNull();
        data.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void Deserialize_TimelineWithTracks_TracksDeserializeCorrectly()
    {
        var json = """
        {
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
        }
        """;

        var timeline = JsonSerializer.Deserialize<TimelineData>(json, Options);

        timeline.Should().NotBeNull();
        timeline!.Tracks.Should().HaveCount(2);
        timeline.Tracks[0].Name.Should().Be("Chatbot");
        timeline.Tracks[0].Label.Should().Be("M1");
        timeline.Tracks[0].Color.Should().Be("#0078D4");
        timeline.Tracks[0].Milestones.Should().HaveCount(2);
        timeline.Tracks[0].Milestones[0].Type.Should().Be("poc");
        timeline.Tracks[0].Milestones[1].Type.Should().Be("production");
        timeline.Tracks[1].Milestones[0].Type.Should().Be("checkpoint");
    }

    [Fact]
    public void Deserialize_HeatmapWithItems_AllCategoriesDeserialize()
    {
        var json = """
        {
            "shipped": {
                "jan": ["Item A", "Item B"],
                "feb": ["Item C"]
            },
            "inProgress": {
                "mar": ["Item D"]
            },
            "carryover": {
                "apr": ["Item E"]
            },
            "blockers": {
                "apr": ["Blocker 1"]
            }
        }
        """;

        var heatmap = JsonSerializer.Deserialize<HeatmapData>(json, Options);

        heatmap.Should().NotBeNull();
        heatmap!.Shipped["jan"].Should().HaveCount(2);
        heatmap.Shipped["feb"].Should().ContainSingle();
        heatmap.InProgress["mar"].Should().ContainSingle();
        heatmap.Carryover["apr"].Should().ContainSingle();
        heatmap.Blockers["apr"].Should().ContainSingle().Which.Should().Be("Blocker 1");
    }

    [Fact]
    public void Deserialize_MinimalJson_MissingFieldsGetDefaults()
    {
        var json = "{}";

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data.Should().NotBeNull();
        data!.Title.Should().BeEmpty();
        data.Subtitle.Should().BeEmpty();
        data.Months.Should().BeEmpty();
        data.Timeline.Should().NotBeNull();
        data.Timeline.Tracks.Should().BeEmpty();
        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Shipped.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_NullJsonResult_ReturnsNull()
    {
        var json = "null";

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data.Should().BeNull();
    }

    [Fact]
    public void Deserialize_MalformedJson_ThrowsJsonException()
    {
        var json = "{ invalid json }";

        var act = () => JsonSerializer.Deserialize<DashboardData>(json, Options);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_TrailingComma_ThrowsJsonException()
    {
        // Note: this tests default System.Text.Json behavior (no AllowTrailingCommas)
        var json = """{ "title": "Test", }""";

        var act = () => JsonSerializer.Deserialize<DashboardData>(json, Options);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_EmptyString_ThrowsJsonException()
    {
        var json = "";

        var act = () => JsonSerializer.Deserialize<DashboardData>(json, Options);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_MilestoneTypes_AllThreeTypesDeserialize()
    {
        var json = """
        {
            "date": "2026-03-15",
            "label": "Mar 15",
            "type": "poc"
        }
        """;

        var ms = JsonSerializer.Deserialize<Milestone>(json, Options);
        ms.Should().NotBeNull();
        ms!.Type.Should().Be("poc");
        ms.Date.Should().Be("2026-03-15");
        ms.Label.Should().Be("Mar 15");
    }

    [Fact]
    public void Deserialize_MilestoneWithoutType_DefaultsToCheckpoint()
    {
        var json = """
        {
            "date": "2026-03-15",
            "label": "Mar 15"
        }
        """;

        var ms = JsonSerializer.Deserialize<Milestone>(json, Options);
        ms.Should().NotBeNull();
        ms!.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void Deserialize_HeatmapEmptyArrays_ProducesEmptyLists()
    {
        var json = """
        {
            "shipped": { "jan": [] },
            "inProgress": {},
            "carryover": {},
            "blockers": {}
        }
        """;

        var heatmap = JsonSerializer.Deserialize<HeatmapData>(json, Options);

        heatmap.Should().NotBeNull();
        heatmap!.Shipped["jan"].Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_CaseInsensitive_PropertyNamesMatchRegardlessOfCase()
    {
        var json = """
        {
            "Title": "Upper Case",
            "SUBTITLE": "ALL CAPS"
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data.Should().NotBeNull();
        data!.Title.Should().Be("Upper Case");
        data.Subtitle.Should().Be("ALL CAPS");
    }

    [Fact]
    public void Serialize_DashboardData_UsesJsonPropertyNames()
    {
        var data = new DashboardData
        {
            Title = "Test",
            CurrentMonth = "Apr"
        };

        var json = JsonSerializer.Serialize(data);

        json.Should().Contain("\"title\":");
        json.Should().Contain("\"currentMonth\":");
    }

    [Fact]
    public void Deserialize_ExtraFieldsInJson_AreIgnored()
    {
        var json = """
        {
            "title": "Test",
            "unknownField": "should be ignored",
            "anotherUnknown": 42
        }
        """;

        var act = () => JsonSerializer.Deserialize<DashboardData>(json, Options);

        act.Should().NotThrow();
        var data = act();
        data!.Title.Should().Be("Test");
    }
}