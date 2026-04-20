using ReportingDashboard.Web.ViewModels;

namespace ReportingDashboard.Web.Tests.Unit;

public class ViewModelEmptyTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void TimelineViewModel_Empty_IsNotNullAndHasZeroGeometry()
    {
        TimelineViewModel.Empty.Should().NotBeNull();
        TimelineViewModel.Empty.Gridlines.Should().BeEmpty();
        TimelineViewModel.Empty.Lanes.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TimelineViewModel_Empty_NowMarkerIsOutOfRangeAtZero()
    {
        TimelineViewModel.Empty.Now.Should().NotBeNull();
        TimelineViewModel.Empty.Now.X.Should().Be(0);
        TimelineViewModel.Empty.Now.InRange.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapViewModel_Empty_IsNotNullWithNoMonthsOrRows()
    {
        HeatmapViewModel.Empty.Should().NotBeNull();
        HeatmapViewModel.Empty.Months.Should().BeEmpty();
        HeatmapViewModel.Empty.Rows.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapViewModel_Empty_CurrentMonthIndexIsNegativeOne()
    {
        HeatmapViewModel.Empty.CurrentMonthIndex.Should().Be(-1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TimelineViewModel_Empty_IsSameInstanceOnRepeatedAccess()
    {
        ReferenceEquals(TimelineViewModel.Empty, TimelineViewModel.Empty).Should().BeTrue();
    }
}