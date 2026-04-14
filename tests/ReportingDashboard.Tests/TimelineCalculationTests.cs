using ReportingDashboard.Helpers;
using Xunit;

namespace ReportingDashboard.Tests;

public class TimelineCalculationTests
{
    private static readonly DateOnly Start = new(2026, 1, 1);
    private static readonly DateOnly End = new(2026, 7, 1);

    [Fact]
    public void CalculateX_StartDate_ReturnsZero()
    {
        var x = TimelineHelper.CalculateX("2026-01-01", Start, End);
        Assert.Equal(0.0, x, precision: 1);
    }

    [Fact]
    public void CalculateX_EndDate_ReturnsSvgWidth()
    {
        var x = TimelineHelper.CalculateX("2026-07-01", Start, End);
        Assert.Equal(1560.0, x, precision: 1);
    }

    [Fact]
    public void CalculateX_MidRange_ReturnsProportionalX()
    {
        var x = TimelineHelper.CalculateX("2026-04-01", Start, End);
        var expected = 90.0 / 181.0 * 1560.0;
        Assert.Equal(expected, x, precision: 1);
    }

    [Fact]
    public void CalculateX_BeforeStart_ReturnsNegative()
    {
        var x = TimelineHelper.CalculateX("2025-12-01", Start, End);
        Assert.True(x < 0);
    }

    [Fact]
    public void CalculateX_AfterEnd_ExceedsSvgWidth()
    {
        var x = TimelineHelper.CalculateX("2026-08-01", Start, End);
        Assert.True(x > 1560.0);
    }

    [Fact]
    public void CalculateXClamped_BeforeStart_ReturnsZero()
    {
        var x = TimelineHelper.CalculateXClamped(new DateOnly(2025, 12, 1), Start, End);
        Assert.Equal(0.0, x, precision: 1);
    }

    [Fact]
    public void CalculateXClamped_AfterEnd_ReturnsSvgWidth()
    {
        var x = TimelineHelper.CalculateXClamped(new DateOnly(2026, 8, 1), Start, End);
        Assert.Equal(1560.0, x, precision: 1);
    }

    [Fact]
    public void GetMonthGridlines_SixMonthRange_ReturnsSixEntries()
    {
        var gridlines = TimelineHelper.GetMonthGridlines(Start, End);
        Assert.Equal(6, gridlines.Count);
    }

    [Fact]
    public void GetMonthGridlines_LabelsAreAbbreviated()
    {
        var gridlines = TimelineHelper.GetMonthGridlines(Start, End);
        Assert.Equal("Feb", gridlines[0].Label);
        Assert.Equal("Mar", gridlines[1].Label);
        Assert.Equal("Apr", gridlines[2].Label);
        Assert.Equal("May", gridlines[3].Label);
        Assert.Equal("Jun", gridlines[4].Label);
        Assert.Equal("Jul", gridlines[5].Label);
    }

    [Fact]
    public void GetMonthGridlines_FirstGridlinePositionIsCorrect()
    {
        var gridlines = TimelineHelper.GetMonthGridlines(Start, End);
        var expected = 31.0 / 181.0 * 1560.0;
        Assert.Equal(expected, gridlines[0].X, precision: 1);
    }

    [Fact]
    public void GetYLane_ThreeWorkstreams_Returns42_98_154()
    {
        Assert.Equal(42, TimelineHelper.GetYLane(0, 3));
        Assert.Equal(98, TimelineHelper.GetYLane(1, 3));
        Assert.Equal(154, TimelineHelper.GetYLane(2, 3));
    }

    [Fact]
    public void GetYLane_OneWorkstream_ReturnsCentered()
    {
        Assert.Equal(98, TimelineHelper.GetYLane(0, 1));
    }

    [Fact]
    public void GetYLane_TwoWorkstreams_ReturnsDistributed()
    {
        Assert.Equal(70, TimelineHelper.GetYLane(0, 2));
        Assert.Equal(126, TimelineHelper.GetYLane(1, 2));
    }

    [Fact]
    public void GetYLane_FourWorkstreams_DistributesEvenly()
    {
        var y0 = TimelineHelper.GetYLane(0, 4);
        var y3 = TimelineHelper.GetYLane(3, 4);
        Assert.Equal(30, y0, precision: 1);
        Assert.Equal(170, y3, precision: 1);
    }

    [Fact]
    public void GetLabelAbove_OverrideAbove_ReturnsTrue()
    {
        Assert.True(TimelineHelper.GetLabelAbove("above", 1));
    }

    [Fact]
    public void GetLabelAbove_OverrideBelow_ReturnsFalse()
    {
        Assert.False(TimelineHelper.GetLabelAbove("below", 0));
    }

    [Fact]
    public void GetLabelAbove_NoOverride_AlternatesByIndex()
    {
        Assert.True(TimelineHelper.GetLabelAbove(null, 0));
        Assert.False(TimelineHelper.GetLabelAbove(null, 1));
        Assert.True(TimelineHelper.GetLabelAbove(null, 2));
    }

    [Fact]
    public void DiamondPoints_ReturnsCorrectFormat()
    {
        var pts = TimelineHelper.DiamondPoints(100, 50, 11);
        Assert.Contains("100.0,39.0", pts);
        Assert.Contains("111.0,50.0", pts);
        Assert.Contains("100.0,61.0", pts);
        Assert.Contains("89.0,50.0", pts);
    }
}