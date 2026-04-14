using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataTests
{
    [Fact]
    public void EffectiveTimelineStart_WhenTimelineStartIsNull_ReturnsJanFirstOfCurrentDateYear()
    {
        var data = new DashboardData(
            Title: "Test",
            Subtitle: null,
            BacklogUrl: null,
            CurrentDate: new DateOnly(2026, 4, 14),
            Months: new[] { "Jan" },
            CurrentMonthIndex: 0,
            TimelineStart: null,
            TimelineEnd: null,
            MilestoneTracks: Array.Empty<MilestoneTrack>(),
            Heatmap: new HeatmapData(
                new HeatmapRow(new Dictionary<string, string[]>()),
                new HeatmapRow(new Dictionary<string, string[]>()),
                new HeatmapRow(new Dictionary<string, string[]>()),
                new HeatmapRow(new Dictionary<string, string[]>()))
        );

        data.EffectiveTimelineStart.Should().Be(new DateOnly(2026, 1, 1));
    }

    [Fact]
    public void EffectiveTimelineEnd_WhenTimelineEndIsNull_ReturnsJuneThirtiethOfCurrentDateYear()
    {
        var data = new DashboardData(
            Title: "Test",
            Subtitle: null,
            BacklogUrl: null,
            CurrentDate: new DateOnly(2026, 4, 14),
            Months: new[] { "Jan" },
            CurrentMonthIndex: 0,
            TimelineStart: null,
            TimelineEnd: null,
            MilestoneTracks: Array.Empty<MilestoneTrack>(),
            Heatmap: new HeatmapData(
                new HeatmapRow(new Dictionary<string, string[]>()),
                new HeatmapRow(new Dictionary<string, string[]>()),
                new HeatmapRow(new Dictionary<string, string[]>()),
                new HeatmapRow(new Dictionary<string, string[]>()))
        );

        data.EffectiveTimelineEnd.Should().Be(new DateOnly(2026, 6, 30));
    }

    [Fact]
    public void EffectiveTimelineStart_WhenTimelineStartIsSet_ReturnsExplicitValue()
    {
        var explicitStart = new DateOnly(2026, 3, 1);
        var data = new DashboardData(
            Title: "Test",
            Subtitle: null,
            BacklogUrl: null,
            CurrentDate: new DateOnly(2026, 4, 14),
            Months: new[] { "Mar" },
            CurrentMonthIndex: 0,
            TimelineStart: explicitStart,
            TimelineEnd: new DateOnly(2026, 9, 30),
            MilestoneTracks: Array.Empty<MilestoneTrack>(),
            Heatmap: new HeatmapData(
                new HeatmapRow(new Dictionary<string, string[]>()),
                new HeatmapRow(new Dictionary<string, string[]>()),
                new HeatmapRow(new Dictionary<string, string[]>()),
                new HeatmapRow(new Dictionary<string, string[]>()))
        );

        data.EffectiveTimelineStart.Should().Be(explicitStart);
    }

    [Fact]
    public void Title_WhenNull_DefaultsToUntitledDashboard()
    {
        var data = new DashboardData(
            Title: null!,
            Subtitle: null,
            BacklogUrl: null,
            CurrentDate: new DateOnly(2026, 1, 1),
            Months: null!,
            CurrentMonthIndex: 0,
            TimelineStart: null,
            TimelineEnd: null,
            MilestoneTracks: null!,
            Heatmap: null!
        );

        data.Title.Should().Be("Untitled Dashboard");
    }

    [Fact]
    public void NullCollections_DefaultToEmpty()
    {
        var data = new DashboardData(
            Title: "Test",
            Subtitle: null,
            BacklogUrl: null,
            CurrentDate: new DateOnly(2026, 1, 1),
            Months: null!,
            CurrentMonthIndex: 0,
            TimelineStart: null,
            TimelineEnd: null,
            MilestoneTracks: null!,
            Heatmap: null!
        );

        data.Months.Should().BeEmpty();
        data.MilestoneTracks.Should().BeEmpty();
        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Shipped.Items.Should().BeEmpty();
    }
}

[Trait("Category", "Unit")]
public class HeatmapRowTests
{
    [Fact]
    public void GetItems_ExistingMonth_ReturnsItems()
    {
        var row = new HeatmapRow(new Dictionary<string, string[]>
        {
            ["Jan"] = new[] { "Item A", "Item B" },
            ["Feb"] = new[] { "Item C" }
        });

        row.GetItems("Jan").Should().BeEquivalentTo(new[] { "Item A", "Item B" });
    }

    [Fact]
    public void GetItems_NonexistentMonth_ReturnsEmptyArray()
    {
        var row = new HeatmapRow(new Dictionary<string, string[]>
        {
            ["Jan"] = new[] { "Item A" }
        });

        row.GetItems("NonexistentMonth").Should().BeEmpty();
    }

    [Fact]
    public void TotalItemCount_SumsAllArrayLengths()
    {
        var row = new HeatmapRow(new Dictionary<string, string[]>
        {
            ["Jan"] = new[] { "A", "B" },
            ["Feb"] = new[] { "C" },
            ["Mar"] = new[] { "D", "E", "F" }
        });

        row.TotalItemCount.Should().Be(6);
    }

    [Fact]
    public void TotalItemCount_EmptyDictionary_ReturnsZero()
    {
        var row = new HeatmapRow(new Dictionary<string, string[]>());

        row.TotalItemCount.Should().Be(0);
    }

    [Fact]
    public void Items_WhenNull_DefaultsToEmptyDictionary()
    {
        var row = new HeatmapRow(null!);

        row.Items.Should().NotBeNull();
        row.Items.Should().BeEmpty();
        row.TotalItemCount.Should().Be(0);
    }
}