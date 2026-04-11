using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class DashboardDataTests
{
    [Fact]
    public void DashboardData_CanSetTitle()
    {
        var data = new DashboardData { Title = "My Dashboard" };
        Assert.Equal("My Dashboard", data.Title);
    }

    [Fact]
    public void DashboardData_CanSetSubtitle()
    {
        var data = new DashboardData { Subtitle = "Team X - April 2026" };
        Assert.Equal("Team X - April 2026", data.Subtitle);
    }

    [Fact]
    public void DashboardData_CanSetBacklogLink()
    {
        var data = new DashboardData { BacklogLink = "https://dev.azure.com/backlog" };
        Assert.Equal("https://dev.azure.com/backlog", data.BacklogLink);
    }

    [Fact]
    public void DashboardData_CanSetCurrentMonth()
    {
        var data = new DashboardData { CurrentMonth = "April" };
        Assert.Equal("April", data.CurrentMonth);
    }

    [Fact]
    public void DashboardData_CanSetMonths()
    {
        var data = new DashboardData
        {
            Months = new List<string> { "Jan", "Feb", "Mar", "Apr" }
        };
        Assert.Equal(4, data.Months.Count);
    }

    [Fact]
    public void DashboardData_CanSetTimeline()
    {
        var data = new DashboardData
        {
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-07-01"
            }
        };
        Assert.NotNull(data.Timeline);
        Assert.Equal("2026-01-01", data.Timeline.StartDate);
    }

    [Fact]
    public void DashboardData_CanSetHeatmap()
    {
        var data = new DashboardData
        {
            Heatmap = new HeatmapData
            {
                Shipped = new Dictionary<string, List<string>>(),
                InProgress = new Dictionary<string, List<string>>(),
                Carryover = new Dictionary<string, List<string>>(),
                Blockers = new Dictionary<string, List<string>>()
            }
        };
        Assert.NotNull(data.Heatmap);
    }
}