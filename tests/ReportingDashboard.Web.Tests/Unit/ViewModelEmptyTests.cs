using System;

namespace ReportingDashboard.Web.Tests.Unit;

public class ViewModelEmptyTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void TimelineViewModel_Empty_HasEmptyCollectionsAndNotInRange()
    {
        var vm = TimelineViewModel.Empty;

        vm.Should().NotBeNull();
        vm.Gridlines.Should().NotBeNull().And.BeEmpty();
        vm.Lanes.Should().NotBeNull().And.BeEmpty();
        vm.Now.Should().NotBeNull();
        vm.Now.InRange.Should().BeFalse();
        vm.Now.X.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapViewModel_Empty_HasEmptyMonthsAndNoCurrentIndex()
    {
        var vm = HeatmapViewModel.Empty;

        vm.Should().NotBeNull();
        vm.Months.Should().NotBeNull().And.BeEmpty();
        vm.Rows.Should().NotBeNull().And.BeEmpty();
        vm.CurrentMonthIndex.Should().Be(-1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapCellView_EmptyCell_IsMarkedEmpty()
    {
        var cell = HeatmapCellView.EmptyCell;

        cell.Should().NotBeNull();
        cell.Items.Should().NotBeNull().And.BeEmpty();
        cell.OverflowCount.Should().Be(0);
        cell.IsEmpty.Should().BeTrue();
    }
}