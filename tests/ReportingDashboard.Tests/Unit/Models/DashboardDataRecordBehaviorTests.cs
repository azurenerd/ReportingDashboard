using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

/// <summary>
/// Tests C# record behavior: immutability, equality, with-expressions, deconstruction,
/// and edge cases for all 8 model types. Complements DashboardDataModelsTests which
/// focuses on JSON serialization.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardDataRecordBehaviorTests
{
    // ---- Record Immutability via With-Expressions ----

    [Fact]
    public void ProjectInfo_WithExpression_CreatesNewInstance()
    {
        var original = new ProjectInfo("Title", "Sub", "http://url", "Apr");
        var modified = original with { Title = "New Title" };

        modified.Title.Should().Be("New Title");
        modified.Subtitle.Should().Be("Sub");
        original.Title.Should().Be("Title", "original should be unchanged");
    }

    [Fact]
    public void TimelineConfig_WithExpression_CreatesNewInstance()
    {
        var original = new TimelineConfig(new List<string> { "Jan" }, 0.5);
        var modified = original with { NowPosition = 0.8 };

        modified.NowPosition.Should().Be(0.8);
        original.NowPosition.Should().Be(0.5);
    }

    [Fact]
    public void Milestone_WithExpression_CanSetLabelToNull()
    {
        var original = new Milestone("2024-01-01", "poc", 0.5, "PoC Ready");
        var modified = original with { Label = null };

        modified.Label.Should().BeNull();
        original.Label.Should().Be("PoC Ready");
    }

    [Fact]
    public void MilestoneTrack_WithExpression_PreservesOtherFields()
    {
        var milestones = new List<Milestone> { new("2024-01-01", "checkpoint", 0.1, null) };
        var original = new MilestoneTrack("m1", "Track 1", "#FF0000", milestones);
        var modified = original with { Label = "Renamed Track" };

        modified.Id.Should().Be("m1");
        modified.Color.Should().Be("#FF0000");
        modified.Label.Should().Be("Renamed Track");
        modified.Milestones.Should().BeSameAs(milestones, "with-expression shallow copies references");
    }

    [Fact]
    public void HeatmapCategory_WithExpression_PreservesItems()
    {
        var items = new Dictionary<string, List<string>> { { "Jan", new List<string> { "A" } } };
        var original = new HeatmapCategory("Shipped", "ship", "✅", items);
        var modified = original with { Name = "Delivered" };

        modified.Name.Should().Be("Delivered");
        modified.Items.Should().BeSameAs(items);
    }

    // ---- Record Equality ----

    [Fact]
    public void Milestone_EqualValues_AreEqual()
    {
        var a = new Milestone("2024-01-01", "poc", 0.5, "PoC");
        var b = new Milestone("2024-01-01", "poc", 0.5, "PoC");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Milestone_DifferentValues_AreNotEqual()
    {
        var a = new Milestone("2024-01-01", "poc", 0.5, "PoC");
        var b = new Milestone("2024-01-01", "production", 0.5, "PoC");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Milestone_NullLabelVsNonNull_AreNotEqual()
    {
        var a = new Milestone("2024-01-01", "poc", 0.5, null);
        var b = new Milestone("2024-01-01", "poc", 0.5, "Label");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Milestone_BothNullLabels_AreEqual()
    {
        var a = new Milestone("2024-01-01", "poc", 0.5, null);
        var b = new Milestone("2024-01-01", "poc", 0.5, null);

        a.Should().Be(b);
    }

    [Fact]
    public void TimelineConfig_EqualValues_AreEqual()
    {
        // Note: List reference equality means two different lists are NOT equal
        var months = new List<string> { "Jan", "Feb" };
        var a = new TimelineConfig(months, 0.5);
        var b = new TimelineConfig(months, 0.5);

        a.Should().Be(b);
    }

    [Fact]
    public void TimelineConfig_DifferentListInstances_AreNotEqual()
    {
        var a = new TimelineConfig(new List<string> { "Jan" }, 0.5);
        var b = new TimelineConfig(new List<string> { "Jan" }, 0.5);

        // Records use reference equality for collections
        a.Should().NotBe(b);
    }

    [Fact]
    public void MilestoneTrack_EqualReferences_AreEqual()
    {
        var milestones = new List<Milestone>();
        var a = new MilestoneTrack("m1", "Track", "#FFF", milestones);
        var b = new MilestoneTrack("m1", "Track", "#FFF", milestones);

        a.Should().Be(b);
    }

    // ---- ToString ----

    [Fact]
    public void ProjectInfo_ToString_ContainsPropertyValues()
    {
        var info = new ProjectInfo("My Project", "Sub", "http://url", "Apr");
        var str = info.ToString();

        str.Should().Contain("My Project");
        str.Should().Contain("Sub");
        str.Should().Contain("Apr");
    }

    [Fact]
    public void Milestone_ToString_ContainsNullLabelGracefully()
    {
        var milestone = new Milestone("2024-01-01", "poc", 0.5, null);
        var str = milestone.ToString();

        str.Should().NotBeNull();
        str.Should().Contain("poc");
    }

    // ---- Positional Deconstruction ----

    [Fact]
    public void ProjectInfo_Deconstruct_AllFields()
    {
        var info = new ProjectInfo("T", "S", "http://x", "Jan");
        var (title, subtitle, backlogUrl, currentMonth) = info;

        title.Should().Be("T");
        subtitle.Should().Be("S");
        backlogUrl.Should().Be("http://x");
        currentMonth.Should().Be("Jan");
    }

    [Fact]
    public void Milestone_Deconstruct_AllFields()
    {
        var m = new Milestone("2024-03-15", "production", 0.7, "GA Release");
        var (date, type, position, label) = m;

        date.Should().Be("2024-03-15");
        type.Should().Be("production");
        position.Should().Be(0.7);
        label.Should().Be("GA Release");
    }

    [Fact]
    public void DashboardData_Deconstruct_AllTopLevelFields()
    {
        var data = new DashboardData(
            new ProjectInfo("T", "S", "", "Jan"),
            new TimelineConfig(new List<string> { "Jan" }, 0.5),
            new List<MilestoneTrack>(),
            new HeatmapData(new List<string> { "Jan" }, new List<HeatmapCategory>())
        );

        var (project, timeline, tracks, heatmap) = data;

        project.Title.Should().Be("T");
        timeline.NowPosition.Should().Be(0.5);
        tracks.Should().BeEmpty();
        heatmap.Categories.Should().BeEmpty();
    }

    // ---- Edge Cases for Milestone Position ----

    [Fact]
    public void Milestone_PositionZero_IsValid()
    {
        var m = new Milestone("2024-01-01", "checkpoint", 0.0, null);
        m.Position.Should().Be(0.0);
    }

    [Fact]
    public void Milestone_PositionOne_IsValid()
    {
        var m = new Milestone("2024-12-31", "production", 1.0, "End");
        m.Position.Should().Be(1.0);
    }

    // ---- HeatmapCategory Items Dictionary Edge Cases ----

    [Fact]
    public void HeatmapCategory_ItemsWithEmptyLists_Deserializes()
    {
        var json = """{"name":"X","cssClass":"ship","emoji":"✅","items":{"Jan":[],"Feb":[]}}""";
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<HeatmapCategory>(json, options);

        result!.Items.Should().HaveCount(2);
        result.Items["Jan"].Should().BeEmpty();
        result.Items["Feb"].Should().BeEmpty();
    }

    [Fact]
    public void HeatmapCategory_ItemsWithSpecialCharacters_Deserializes()
    {
        var json = """{"name":"X","cssClass":"ship","emoji":"✅","items":{"Jan":["Item with \"quotes\"","Item <html>"]}}""";
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<HeatmapCategory>(json, options);

        result!.Items["Jan"].Should().Contain("Item with \"quotes\"");
        result.Items["Jan"].Should().Contain("Item <html>");
    }

    [Fact]
    public void HeatmapCategory_ItemsWithManyEntries_Deserializes()
    {
        var items = new Dictionary<string, List<string>>();
        for (int i = 0; i < 12; i++)
        {
            items[$"Month{i}"] = Enumerable.Range(0, 10).Select(j => $"Item {j}").ToList();
        }
        var category = new HeatmapCategory("Shipped", "ship", "✅", items);

        var json = JsonSerializer.Serialize(category);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var roundTripped = JsonSerializer.Deserialize<HeatmapCategory>(json, options);

        roundTripped!.Items.Should().HaveCount(12);
        roundTripped.Items["Month0"].Should().HaveCount(10);
    }

    // ---- Full DashboardData Construction Edge Cases ----

    [Fact]
    public void DashboardData_WithEmptyCollections_IsValid()
    {
        var data = new DashboardData(
            new ProjectInfo("T", "S", "", "Jan"),
            new TimelineConfig(new List<string>(), 0.0),
            new List<MilestoneTrack>(),
            new HeatmapData(new List<string>(), new List<HeatmapCategory>())
        );

        data.Tracks.Should().BeEmpty();
        data.Heatmap.Categories.Should().BeEmpty();
        data.Timeline.Months.Should().BeEmpty();
    }

    [Fact]
    public void DashboardData_WithMultipleTracks_PreservesOrder()
    {
        var tracks = new List<MilestoneTrack>
        {
            new("m1", "First", "#AAA", new List<Milestone>()),
            new("m2", "Second", "#BBB", new List<Milestone>()),
            new("m3", "Third", "#CCC", new List<Milestone>())
        };
        var data = new DashboardData(
            new ProjectInfo("T", "S", "", "Jan"),
            new TimelineConfig(new List<string> { "Jan" }, 0.5),
            tracks,
            new HeatmapData(new List<string> { "Jan" }, new List<HeatmapCategory>())
        );

        data.Tracks[0].Id.Should().Be("m1");
        data.Tracks[1].Id.Should().Be("m2");
        data.Tracks[2].Id.Should().Be("m3");
    }

    // ---- JSON Deserialization with Missing Optional Fields ----

    [Fact]
    public void Milestone_Deserialize_MissingLabelField_DefaultsToNull()
    {
        var json = """{"date":"2024-01-01","type":"checkpoint","position":0.3}""";
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<Milestone>(json, options);

        result!.Label.Should().BeNull();
    }

    [Fact]
    public void Milestone_Deserialize_ExplicitNullLabel_IsNull()
    {
        var json = """{"date":"2024-01-01","type":"checkpoint","position":0.3,"label":null}""";
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<Milestone>(json, options);

        result!.Label.Should().BeNull();
    }

    [Fact]
    public void Milestone_Deserialize_EmptyStringLabel_IsEmptyString()
    {
        var json = """{"date":"2024-01-01","type":"checkpoint","position":0.3,"label":""}""";
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<Milestone>(json, options);

        result!.Label.Should().BeEmpty();
    }

    // ---- ProjectInfo with various BacklogUrl values ----

    [Fact]
    public void ProjectInfo_EmptyBacklogUrl_IsValid()
    {
        var info = new ProjectInfo("T", "S", "", "Jan");
        info.BacklogUrl.Should().BeEmpty();
    }

    [Fact]
    public void ProjectInfo_LongBacklogUrl_PreservesValue()
    {
        var longUrl = "https://dev.azure.com/org/project/_backlogs/backlog/" + new string('x', 500);
        var info = new ProjectInfo("T", "S", longUrl, "Jan");
        info.BacklogUrl.Should().Be(longUrl);
    }
}