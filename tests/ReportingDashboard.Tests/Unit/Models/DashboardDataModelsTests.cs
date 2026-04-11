using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class DashboardDataModelsTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    // ---- ProjectInfo ----

    [Fact]
    public void ProjectInfo_Deserialize_AllFieldsMapped()
    {
        var json = """{"title":"P","subtitle":"S","backlogUrl":"http://x","currentMonth":"Apr"}""";
        var result = JsonSerializer.Deserialize<ProjectInfo>(json, Options);

        result.Should().NotBeNull();
        result!.Title.Should().Be("P");
        result.Subtitle.Should().Be("S");
        result.BacklogUrl.Should().Be("http://x");
        result.CurrentMonth.Should().Be("Apr");
    }

    [Fact]
    public void ProjectInfo_Serialize_UsesJsonPropertyNames()
    {
        var info = new ProjectInfo("Title", "Sub", "http://url", "Mar");
        var json = JsonSerializer.Serialize(info);

        json.Should().Contain("\"title\":");
        json.Should().Contain("\"subtitle\":");
        json.Should().Contain("\"backlogUrl\":");
        json.Should().Contain("\"currentMonth\":");
    }

    // ---- TimelineConfig ----

    [Fact]
    public void TimelineConfig_Deserialize_AllFieldsMapped()
    {
        var json = """{"months":["Jan","Feb"],"nowPosition":0.42}""";
        var result = JsonSerializer.Deserialize<TimelineConfig>(json, Options);

        result.Should().NotBeNull();
        result!.Months.Should().BeEquivalentTo(new[] { "Jan", "Feb" });
        result.NowPosition.Should().Be(0.42);
    }

    // ---- MilestoneTrack ----

    [Fact]
    public void MilestoneTrack_Deserialize_AllFieldsMapped()
    {
        var json = """{"id":"m1","label":"Track 1","color":"#FF0000","milestones":[]}""";
        var result = JsonSerializer.Deserialize<MilestoneTrack>(json, Options);

        result.Should().NotBeNull();
        result!.Id.Should().Be("m1");
        result.Label.Should().Be("Track 1");
        result.Color.Should().Be("#FF0000");
        result.Milestones.Should().BeEmpty();
    }

    // ---- Milestone ----

    [Fact]
    public void Milestone_Deserialize_WithLabel()
    {
        var json = """{"date":"2024-03-15","type":"poc","position":0.5,"label":"PoC Ready"}""";
        var result = JsonSerializer.Deserialize<Milestone>(json, Options);

        result.Should().NotBeNull();
        result!.Date.Should().Be("2024-03-15");
        result.Type.Should().Be("poc");
        result.Position.Should().Be(0.5);
        result.Label.Should().Be("PoC Ready");
    }

    [Fact]
    public void Milestone_Deserialize_WithoutLabel()
    {
        var json = """{"date":"2024-03-15","type":"checkpoint","position":0.3}""";
        var result = JsonSerializer.Deserialize<Milestone>(json, Options);

        result.Should().NotBeNull();
        result!.Label.Should().BeNull();
    }

    // ---- HeatmapData ----

    [Fact]
    public void HeatmapData_Deserialize_AllFieldsMapped()
    {
        var json = """{"months":["Jan","Feb","Mar"],"categories":[]}""";
        var result = JsonSerializer.Deserialize<HeatmapData>(json, Options);

        result.Should().NotBeNull();
        result!.Months.Should().HaveCount(3);
        result.Categories.Should().BeEmpty();
    }

    // ---- HeatmapCategory ----

    [Fact]
    public void HeatmapCategory_Deserialize_WithItems()
    {
        var json = """{"name":"Shipped","cssClass":"ship","emoji":"✅","items":{"Jan":["A","B"],"Feb":["C"]}}""";
        var result = JsonSerializer.Deserialize<HeatmapCategory>(json, Options);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Shipped");
        result.CssClass.Should().Be("ship");
        result.Emoji.Should().Be("✅");
        result.Items.Should().HaveCount(2);
        result.Items["Jan"].Should().BeEquivalentTo(new[] { "A", "B" });
        result.Items["Feb"].Should().BeEquivalentTo(new[] { "C" });
    }

    [Fact]
    public void HeatmapCategory_Deserialize_EmptyItems()
    {
        var json = """{"name":"X","cssClass":"ship","emoji":"✅","items":{}}""";
        var result = JsonSerializer.Deserialize<HeatmapCategory>(json, Options);

        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
    }

    // ---- Full DashboardData Round-Trip ----

    [Fact]
    public void DashboardData_RoundTrip_PreservesAllData()
    {
        var original = new DashboardData(
            new ProjectInfo("Title", "Sub", "http://url", "Apr"),
            new TimelineConfig(new List<string> { "Jan", "Feb" }, 0.5),
            new List<MilestoneTrack>
            {
                new("m1", "Track 1", "#FF0000", new List<Milestone>
                {
                    new("2024-03-15", "poc", 0.5, "PoC")
                })
            },
            new HeatmapData(
                new List<string> { "Jan", "Feb" },
                new List<HeatmapCategory>
                {
                    new("Shipped", "ship", "✅", new Dictionary<string, List<string>>
                    {
                        { "Jan", new List<string> { "Item A" } }
                    })
                })
        );

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json, Options);

        deserialized.Should().NotBeNull();
        deserialized!.Project.Title.Should().Be("Title");
        deserialized.Timeline.Months.Should().HaveCount(2);
        deserialized.Tracks.Should().HaveCount(1);
        deserialized.Tracks[0].Milestones[0].Label.Should().Be("PoC");
        deserialized.Heatmap.Categories[0].Items["Jan"].Should().Contain("Item A");
    }

    // ---- Record Equality ----

    [Fact]
    public void ProjectInfo_RecordEquality_EqualValues()
    {
        var a = new ProjectInfo("T", "S", "http://x", "Jan");
        var b = new ProjectInfo("T", "S", "http://x", "Jan");

        a.Should().Be(b);
    }

    [Fact]
    public void ProjectInfo_RecordEquality_DifferentValues()
    {
        var a = new ProjectInfo("T1", "S", "http://x", "Jan");
        var b = new ProjectInfo("T2", "S", "http://x", "Jan");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Milestone_RecordEquality_NullLabel()
    {
        var a = new Milestone("2024-01-01", "poc", 0.5, null);
        var b = new Milestone("2024-01-01", "poc", 0.5, null);

        a.Should().Be(b);
    }
}