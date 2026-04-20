namespace ReportingDashboard.Web.Tests;

public class SmokeTests
{
    [Fact]
    public void EmptyTimelineViewModel_HasNoLanes()
    {
        TimelineViewModel.Empty.Lanes.Should().BeEmpty();
        TimelineViewModel.Empty.Gridlines.Should().BeEmpty();
        TimelineViewModel.Empty.Now.InRange.Should().BeFalse();
    }

    [Fact]
    public void EmptyHeatmapViewModel_HasNegativeCurrentMonth()
    {
        HeatmapViewModel.Empty.Rows.Should().BeEmpty();
        HeatmapViewModel.Empty.Months.Should().BeEmpty();
        HeatmapViewModel.Empty.CurrentMonthIndex.Should().Be(-1);
    }

    [Fact]
    public void ProjectPlaceholder_HasErrorTitle()
    {
        Project.Placeholder.Title.Should().Contain("error");
    }
}
