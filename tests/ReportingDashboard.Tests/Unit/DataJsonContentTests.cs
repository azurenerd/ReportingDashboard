using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class DataJsonContentTests
{
    private static DashboardData LoadData()
    {
        var json = File.ReadAllText(Path.Combine(FindProjectRoot(), "ReportingDashboard", "data.json"));
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<DashboardData>(json, options)!;
    }

    private static string FindProjectRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "ReportingDashboard.sln")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new InvalidOperationException("Could not find solution root");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DataJson_CanBeDeserialized()
    {
        var data = LoadData();
        data.Should().NotBeNull();
        data.Title.Should().NotBeNullOrWhiteSpace();
        data.Subtitle.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DataJson_Timeline_HasTracksAndMilestones()
    {
        var data = LoadData();
        data.Timeline.Should().NotBeNull();
        data.Timeline.Tracks.Should().NotBeEmpty();
        data.Timeline.StartMonth.Should().NotBeNullOrWhiteSpace();
        data.Timeline.EndMonth.Should().NotBeNullOrWhiteSpace();

        foreach (var track in data.Timeline.Tracks)
        {
            track.Id.Should().NotBeNullOrWhiteSpace();
            track.Label.Should().NotBeNullOrWhiteSpace();
            track.Color.Should().NotBeNullOrWhiteSpace();
            track.Milestones.Should().NotBeEmpty();
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DataJson_Heatmap_HasCorrectMonthsAndCategories()
    {
        var data = LoadData();
        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Months.Should().NotBeEmpty();
        data.Heatmap.CurrentMonth.Should().NotBeNullOrWhiteSpace();
        data.Heatmap.Months.Should().Contain(data.Heatmap.CurrentMonth);

        data.Heatmap.Categories.Should().HaveCount(4);

        var categoryNames = data.Heatmap.Categories.Select(c => c.Name).ToList();
        categoryNames.Should().Contain("Shipped");
        categoryNames.Should().Contain("In Progress");
        categoryNames.Should().Contain("Carryover");
        categoryNames.Should().Contain("Blockers");

        foreach (var cat in data.Heatmap.Categories)
        {
            cat.CssClass.Should().NotBeNullOrWhiteSpace();
            cat.Items.Should().NotBeNull();
            // Each category's item keys should be a subset of the defined months
            cat.Items.Keys.Should().BeSubsetOf(data.Heatmap.Months,
                because: $"'{cat.Name}' item keys should only reference defined months");
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DataJson_HeatmapItems_HaveCorrectDensity()
    {
        var data = LoadData();

        foreach (var cat in data.Heatmap.Categories)
        {
            // Each category should have items for at least one month
            cat.Items.Should().NotBeEmpty(because: $"'{cat.Name}' should have at least some month entries");

            // Items that exist should have reasonable content
            foreach (var (month, items) in cat.Items)
            {
                items.Should().NotBeNull(because: $"'{cat.Name}' items for '{month}' should not be null");
                items.Count.Should().BeGreaterThan(0,
                    because: $"'{cat.Name}' for '{month}' should have at least one item if the key exists");
                items.Count.Should().BeLessOrEqualTo(6,
                    because: $"'{cat.Name}' for '{month}' should not have excessive items");
            }
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DataJson_HasValidBacklogUrl()
    {
        var data = LoadData();
        data.BacklogUrl.Should().NotBeNullOrWhiteSpace();
    }
}