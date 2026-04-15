using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// Validates that data.json deserializes correctly and meets all acceptance criteria
/// for the example data file (PR #1313).
/// </summary>
[Trait("Category", "Unit")]
public class DataJsonContentTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private static DashboardData LoadDataJson()
    {
        // Walk up from bin output to find the ReportingDashboard/data.json
        var dir = AppContext.BaseDirectory;
        string? filePath = null;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "ReportingDashboard", "data.json");
            if (File.Exists(candidate))
            {
                filePath = candidate;
                break;
            }
            // Also check if data.json is in the output directory (CopyToOutputDirectory)
            candidate = Path.Combine(dir, "data.json");
            if (File.Exists(candidate))
            {
                filePath = candidate;
                break;
            }
            dir = Directory.GetParent(dir)?.FullName;
        }

        filePath.Should().NotBeNull("data.json must exist in the project tree");

        var json = File.ReadAllText(filePath!);
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);
        data.Should().NotBeNull("data.json must deserialize to a valid DashboardData instance");
        return data!;
    }

    [Fact]
    public void DataJson_Deserializes_WithCorrectProjectMetadata()
    {
        var data = LoadDataJson();

        data.Title.Should().NotBeNullOrWhiteSpace();
        data.Title.Should().Contain("Project Phoenix");
        data.Subtitle.Should().Contain("April 2026");
        data.BacklogUrl.Should().StartWith("https://");
        data.NowDate.Should().NotBeNull();
        data.NowDate.Should().StartWith("2026-04");
    }

    [Fact]
    public void DataJson_Timeline_HasThreeTracksWithCorrectStructure()
    {
        var data = LoadDataJson();

        data.Timeline.Should().NotBeNull();
        data.Timeline.StartMonth.Should().Be("2026-01");
        data.Timeline.EndMonth.Should().Be("2026-06");
        data.Timeline.Tracks.Should().HaveCount(3);

        var trackIds = data.Timeline.Tracks.Select(t => t.Id).ToList();
        trackIds.Should().BeEquivalentTo(new[] { "M1", "M2", "M3" });

        foreach (var track in data.Timeline.Tracks)
        {
            track.Label.Should().NotBeNullOrWhiteSpace();
            track.Color.Should().MatchRegex("^#[0-9A-Fa-f]{6}$");
            track.Milestones.Should().HaveCountGreaterThanOrEqualTo(3)
                .And.HaveCountLessThanOrEqualTo(5);
        }
    }

    [Fact]
    public void DataJson_Milestones_ContainAllRequiredTypes()
    {
        var data = LoadDataJson();

        var allTypes = data.Timeline.Tracks
            .SelectMany(t => t.Milestones)
            .Select(m => m.Type)
            .Distinct()
            .ToList();

        allTypes.Should().Contain("checkpoint");
        allTypes.Should().Contain("checkpoint-small");
        allTypes.Should().Contain("poc");
        allTypes.Should().Contain("production");
    }

    [Fact]
    public void DataJson_Heatmap_HasCorrectMonthsAndCategories()
    {
        var data = LoadDataJson();

        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Months.Should().BeEquivalentTo(
            new[] { "March", "April", "May", "June" },
            options => options.WithStrictOrdering());
        data.Heatmap.CurrentMonth.Should().Be("April");
        data.Heatmap.Categories.Should().HaveCount(4);

        var expectedPairs = new Dictionary<string, string>
        {
            ["Shipped"] = "ship",
            ["In Progress"] = "prog",
            ["Carryover"] = "carry",
            ["Blockers"] = "block"
        };

        foreach (var cat in data.Heatmap.Categories)
        {
            expectedPairs.Should().ContainKey(cat.Name);
            cat.CssClass.Should().Be(expectedPairs[cat.Name]);
            cat.Items.Keys.Should().BeEquivalentTo(new[] { "March", "April", "May", "June" });
        }
    }

    [Fact]
    public void DataJson_HeatmapItems_HaveCorrectDensity()
    {
        var data = LoadDataJson();

        foreach (var cat in data.Heatmap.Categories)
        {
            // Active months (March, April) should have 1-4 items
            cat.Items["March"].Should().HaveCountGreaterThanOrEqualTo(1,
                $"'{cat.Name}' March should have items");
            cat.Items["April"].Should().HaveCountGreaterThanOrEqualTo(1,
                $"'{cat.Name}' April should have items");

            // June cells must be empty
            cat.Items["June"].Should().BeEmpty(
                $"'{cat.Name}' June should be empty");
        }
    }
}