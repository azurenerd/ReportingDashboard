using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// Validates that data.json deserializes correctly and meets all acceptance criteria
/// for the example data file.
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
        // Title must contain meaningful project name text
        data.Title.Should().MatchRegex(".{5,}",
            "title should be a meaningful project name of at least 5 characters");
        data.Subtitle.Should().NotBeNullOrWhiteSpace();
        data.BacklogUrl.Should().StartWith("https://");
    }

    [Fact]
    public void DataJson_Timeline_HasThreeTracksWithCorrectStructure()
    {
        var data = LoadDataJson();

        data.Timeline.Should().NotBeNull();
        data.Timeline.StartMonth.Should().NotBeNullOrWhiteSpace();
        data.Timeline.EndMonth.Should().NotBeNullOrWhiteSpace();
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
        allTypes.Should().Contain("poc");
        allTypes.Should().Contain("production");
    }

    [Fact]
    public void DataJson_Heatmap_HasCorrectMonthsAndCategories()
    {
        var data = LoadDataJson();

        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Months.Should().HaveCountGreaterThanOrEqualTo(2,
            "heatmap should have at least 2 months");
        data.Heatmap.CurrentMonth.Should().NotBeNullOrWhiteSpace();
        data.Heatmap.Months.Should().Contain(data.Heatmap.CurrentMonth,
            "months list should include the current month");
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
            // Each category's items keys should match the months list
            cat.Items.Keys.Should().BeEquivalentTo(data.Heatmap.Months);
        }
    }

    [Fact]
    public void DataJson_HeatmapItems_HaveCorrectDensity()
    {
        var data = LoadDataJson();

        var months = data.Heatmap.Months;

        foreach (var cat in data.Heatmap.Categories)
        {
            // Every category must have an entry for each month
            cat.Items.Keys.Should().BeEquivalentTo(months,
                $"'{cat.Name}' should have entries for all months");

            // At least one month per category should have items (non-empty data)
            cat.Items.Values.SelectMany(v => v).Should().NotBeEmpty(
                $"'{cat.Name}' should have at least some items across all months");
        }

        // Verify that at least one category has items in the current month
        var currentMonth = data.Heatmap.CurrentMonth;
        data.Heatmap.Categories
            .Any(c => c.Items.ContainsKey(currentMonth) && c.Items[currentMonth].Count > 0)
            .Should().BeTrue("at least one category should have items in the current month");
    }
}