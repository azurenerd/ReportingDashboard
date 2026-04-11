using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class DashboardDataDefaultsTests
{
    [Fact]
    public void DashboardData_DefaultConstruction_TitleIsEmptyString()
    {
        var data = new DashboardData();
        data.Title.Should().BeEmpty();
    }

    [Fact]
    public void DashboardData_DefaultConstruction_SubtitleIsEmptyString()
    {
        var data = new DashboardData();
        data.Subtitle.Should().BeEmpty();
    }

    [Fact]
    public void DashboardData_DefaultConstruction_BacklogLinkIsEmptyString()
    {
        var data = new DashboardData();
        data.BacklogLink.Should().BeEmpty();
    }

    [Fact]
    public void DashboardData_DefaultConstruction_CurrentMonthIsEmptyString()
    {
        var data = new DashboardData();
        data.CurrentMonth.Should().BeEmpty();
    }

    [Fact]
    public void DashboardData_DefaultConstruction_MonthsIsEmptyList()
    {
        var data = new DashboardData();
        data.Months.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void DashboardData_DefaultConstruction_TimelineIsNotNull()
    {
        var data = new DashboardData();
        data.Timeline.Should().NotBeNull();
    }

    [Fact]
    public void DashboardData_DefaultConstruction_HeatmapIsNotNull()
    {
        var data = new DashboardData();
        data.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void DashboardData_SetProperties_ValuesAreRetained()
    {
        var data = new DashboardData
        {
            Title = "Test Title",
            Subtitle = "Test Subtitle",
            BacklogLink = "https://example.com",
            CurrentMonth = "Apr",
            Months = new List<string> { "Jan", "Feb", "Mar", "Apr" }
        };

        data.Title.Should().Be("Test Title");
        data.Subtitle.Should().Be("Test Subtitle");
        data.BacklogLink.Should().Be("https://example.com");
        data.CurrentMonth.Should().Be("Apr");
        data.Months.Should().HaveCount(4);
    }

    [Fact]
    public void DashboardData_CollectionProperties_AreNotNull_PreventingNullReferenceExceptions()
    {
        var data = new DashboardData();

        data.Months.Should().NotBeNull();
        data.Timeline.Should().NotBeNull();
        data.Timeline.Tracks.Should().NotBeNull();
        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Shipped.Should().NotBeNull();
        data.Heatmap.InProgress.Should().NotBeNull();
        data.Heatmap.Carryover.Should().NotBeNull();
        data.Heatmap.Blockers.Should().NotBeNull();
    }
}