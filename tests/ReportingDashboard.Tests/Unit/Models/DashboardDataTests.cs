using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class DashboardDataTests
{
    [Fact]
    public void DashboardData_DefaultValues_AllEmpty()
    {
        var data = new DashboardData();

        data.Title.Should().BeEmpty();
        data.Subtitle.Should().BeEmpty();
        data.BacklogLink.Should().BeEmpty();
        data.CurrentMonth.Should().BeEmpty();
        data.Months.Should().NotBeNull().And.BeEmpty();
        data.Timeline.Should().NotBeNull();
        data.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void DashboardData_SetAllProperties_ShouldRetainValues()
    {
        var data = new DashboardData
        {
            Title = "Executive Dashboard",
            Subtitle = "Q2 2026",
            BacklogLink = "https://dev.azure.com/project",
            CurrentMonth = "Apr",
            Months = new List<string> { "Jan", "Feb", "Mar", "Apr" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-06-30",
                NowDate = "2026-04-10"
            },
            Heatmap = new HeatmapData()
        };

        data.Title.Should().Be("Executive Dashboard");
        data.Subtitle.Should().Be("Q2 2026");
        data.BacklogLink.Should().Be("https://dev.azure.com/project");
        data.CurrentMonth.Should().Be("Apr");
        data.Months.Should().HaveCount(4);
        data.Timeline.StartDate.Should().Be("2026-01-01");
    }

    [Fact]
    public void DashboardData_TimelineIsNeverNull_ByDefault()
    {
        var data = new DashboardData();
        data.Timeline.Should().NotBeNull();
        data.Timeline.Tracks.Should().NotBeNull();
    }

    [Fact]
    public void DashboardData_HeatmapIsNeverNull_ByDefault()
    {
        var data = new DashboardData();
        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Shipped.Should().NotBeNull();
        data.Heatmap.InProgress.Should().NotBeNull();
        data.Heatmap.Carryover.Should().NotBeNull();
        data.Heatmap.Blockers.Should().NotBeNull();
    }
}

[Trait("Category", "Unit")]
public class HeatmapDataTests
{
    [Fact]
    public void HeatmapData_DefaultValues_AllEmptyDictionaries()
    {
        var data = new HeatmapData();

        data.Shipped.Should().NotBeNull().And.BeEmpty();
        data.InProgress.Should().NotBeNull().And.BeEmpty();
        data.Carryover.Should().NotBeNull().And.BeEmpty();
        data.Blockers.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void HeatmapData_PopulateDictionaries_ShouldRetainValues()
    {
        var data = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                { "Jan", new List<string> { "Feature A", "Feature B" } },
                { "Feb", new List<string> { "Feature C" } }
            },
            InProgress = new Dictionary<string, List<string>>
            {
                { "Mar", new List<string> { "Feature D" } }
            }
        };

        data.Shipped.Should().HaveCount(2);
        data.Shipped["Jan"].Should().HaveCount(2);
        data.InProgress.Should().HaveCount(1);
    }
}

[Trait("Category", "Unit")]
public class MonthSummaryTests
{
    [Fact]
    public void MonthSummary_DefaultValues_ShouldHaveDefaults()
    {
        var summary = new MonthSummary();

        summary.Month.Should().BeNull();
        summary.TotalItems.Should().Be(0);
        summary.CompletedItems.Should().Be(0);
        summary.CarriedItems.Should().Be(0);
        summary.OverallHealth.Should().Be("Unknown");
    }

    [Fact]
    public void MonthSummary_SetProperties_ShouldRetainValues()
    {
        var summary = new MonthSummary
        {
            Month = "April",
            TotalItems = 10,
            CompletedItems = 7,
            CarriedItems = 3,
            OverallHealth = "Good"
        };

        summary.Month.Should().Be("April");
        summary.TotalItems.Should().Be(10);
        summary.CompletedItems.Should().Be(7);
        summary.CarriedItems.Should().Be(3);
        summary.OverallHealth.Should().Be("Good");
    }

    [Fact]
    public void MonthSummary_IsRecord_SupportsEquality()
    {
        var a = new MonthSummary { Month = "Jan", TotalItems = 5 };
        var b = new MonthSummary { Month = "Jan", TotalItems = 5 };

        a.Should().Be(b);
    }

    [Fact]
    public void MonthSummary_IsRecord_SupportsWith()
    {
        var original = new MonthSummary { Month = "Jan", TotalItems = 5, OverallHealth = "Good" };
        var modified = original with { TotalItems = 10 };

        modified.TotalItems.Should().Be(10);
        modified.Month.Should().Be("Jan");
        modified.OverallHealth.Should().Be("Good");
        original.TotalItems.Should().Be(5);
    }
}