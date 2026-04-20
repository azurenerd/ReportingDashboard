using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Tests;

public class SmokeTests
{
    [Fact]
    public void DashboardState_Initial_IsNotNull()
    {
        DashboardState.Initial.Should().NotBeNull();
        DashboardState.Initial.Model.Should().NotBeNull();
        DashboardState.Initial.Error.Should().BeNull();
    }

    [Fact]
    public void DashboardModel_Empty_HasDefaults()
    {
        DashboardModel.Empty.Title.Should().BeEmpty();
        DashboardModel.Empty.Timeline.Tracks.Should().BeEmpty();
        DashboardModel.Empty.Heatmap.Months.Should().HaveCount(4);
    }

    [Fact]
    public void TimelineLayout_ComputeTrackY_FirstTrack_Returns42()
    {
        TimelineLayout.ComputeTrackY(0).Should().Be(42);
        TimelineLayout.ComputeTrackY(1).Should().Be(98);
        TimelineLayout.ComputeTrackY(2).Should().Be(154);
    }

    [Fact]
    public void TimelineLayout_ComputeX_AtRangeStart_ReturnsZero()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 6, 30);
        TimelineLayout.ComputeX(start, start, end, 1560).Should().Be(0);
    }
}