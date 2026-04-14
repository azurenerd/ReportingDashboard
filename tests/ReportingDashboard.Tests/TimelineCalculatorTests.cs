using Xunit;
using ReportingDashboard.Helpers;

namespace ReportingDashboard.Tests;

public class TimelineCalculatorTests
{
    private static readonly DateTime Jan1 = new(2026, 1, 1);
    private static readonly DateTime Jul1 = new(2026, 7, 1);

    // --- GetXPosition Tests ---

    [Fact]
    public void GetXPosition_AtTimelineStart_ReturnsZero()
    {
        var result = TimelineCalculator.GetXPosition(Jan1, Jan1, Jul1);
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetXPosition_AtTimelineEnd_ReturnsSvgWidth()
    {
        var result = TimelineCalculator.GetXPosition(Jul1, Jan1, Jul1);
        Assert.Equal(TimelineCalculator.SvgWidth, result);
    }

    [Fact]
    public void GetXPosition_AtMidpoint_ReturnsHalfWidth()
    {
        var mid = Jan1.AddDays((Jul1 - Jan1).TotalDays / 2);
        var result = TimelineCalculator.GetXPosition(mid, Jan1, Jul1);
        Assert.Equal(TimelineCalculator.SvgWidth / 2, result, 2);
    }

    [Fact]
    public void GetXPosition_BeforeStart_ClampsToZero()
    {
        var before = new DateTime(2025, 12, 1);
        var result = TimelineCalculator.GetXPosition(before, Jan1, Jul1);
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetXPosition_AfterEnd_ClampsToSvgWidth()
    {
        var after = new DateTime(2026, 8, 1);
        var result = TimelineCalculator.GetXPosition(after, Jan1, Jul1);
        Assert.Equal(TimelineCalculator.SvgWidth, result);
    }

    [Fact]
    public void GetXPosition_ZeroDuration_ReturnsZero()
    {
        var result = TimelineCalculator.GetXPosition(Jan1, Jan1, Jan1);
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetXPosition_QuarterWayThrough_ReturnsQuarterWidth()
    {
        var totalDays = (Jul1 - Jan1).TotalDays;
        var quarter = Jan1.AddDays(totalDays / 4);
        var result = TimelineCalculator.GetXPosition(quarter, Jan1, Jul1);
        Assert.Equal(TimelineCalculator.SvgWidth / 4, result, 2);
    }

    // --- GetTrackY Tests ---

    [Fact]
    public void GetTrackY_SingleTrack_ReturnsCenterOfRange()
    {
        var result = TimelineCalculator.GetTrackY(0, 1);
        Assert.Equal(98, result);
    }

    [Fact]
    public void GetTrackY_ThreeTracks_FirstTrackAt42()
    {
        var result = TimelineCalculator.GetTrackY(0, 3);
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetTrackY_ThreeTracks_MiddleTrackAt98()
    {
        var result = TimelineCalculator.GetTrackY(1, 3);
        Assert.Equal(98, result);
    }

    [Fact]
    public void GetTrackY_ThreeTracks_LastTrackAt154()
    {
        var result = TimelineCalculator.GetTrackY(2, 3);
        Assert.Equal(154, result);
    }

    [Fact]
    public void GetTrackY_FiveTracks_EvenlySpaced()
    {
        var y0 = TimelineCalculator.GetTrackY(0, 5);
        var y1 = TimelineCalculator.GetTrackY(1, 5);
        var y2 = TimelineCalculator.GetTrackY(2, 5);
        var y3 = TimelineCalculator.GetTrackY(3, 5);
        var y4 = TimelineCalculator.GetTrackY(4, 5);

        var spacing = y1 - y0;
        Assert.Equal(spacing, y2 - y1, 2);
        Assert.Equal(spacing, y3 - y2, 2);
        Assert.Equal(spacing, y4 - y3, 2);
    }

    [Fact]
    public void GetTrackY_FiveTracks_FirstAt42_LastAt154()
    {
        Assert.Equal(42, TimelineCalculator.GetTrackY(0, 5));
        Assert.Equal(154, TimelineCalculator.GetTrackY(4, 5));
    }

    [Fact]
    public void GetTrackY_TwoTracks_FirstAt42_LastAt154()
    {
        Assert.Equal(42, TimelineCalculator.GetTrackY(0, 2));
        Assert.Equal(154, TimelineCalculator.GetTrackY(1, 2));
    }

    // --- DiamondPoints Tests ---

    [Fact]
    public void DiamondPoints_ReturnsCorrectPolygonString()
    {
        var result = TimelineCalculator.DiamondPoints(100, 50, 11);
        Assert.Equal("100,39 111,50 100,61 89,50", result);
    }

    [Fact]
    public void DiamondPoints_DefaultRadius_Uses11()
    {
        var result = TimelineCalculator.DiamondPoints(200, 100);
        Assert.Equal("200,89 211,100 200,111 189,100", result);
    }

    [Fact]
    public void DiamondPoints_WithDecimalCoordinates_FormatsCorrectly()
    {
        var result = TimelineCalculator.DiamondPoints(100.5, 50.5, 11);
        Assert.Equal("100.5,39.5 111.5,50.5 100.5,61.5 89.5,50.5", result);
    }

    // --- F (Format) Tests ---

    [Fact]
    public void F_IntegerValue_NoDecimalPoint()
    {
        Assert.Equal("42", TimelineCalculator.F(42.0));
    }

    [Fact]
    public void F_SingleDecimal_ShowsOnePlace()
    {
        Assert.Equal("42.5", TimelineCalculator.F(42.5));
    }

    [Fact]
    public void F_TwoDecimals_ShowsTwoPlaces()
    {
        Assert.Equal("42.12", TimelineCalculator.F(42.12));
    }

    [Fact]
    public void F_Zero_ReturnsZeroString()
    {
        Assert.Equal("0", TimelineCalculator.F(0.0));
    }

    [Fact]
    public void F_LargeValue_FormatsCorrectly()
    {
        Assert.Equal("1560", TimelineCalculator.F(1560.0));
    }

    // --- GetMonthGridlines Tests ---

    [Fact]
    public void GetMonthGridlines_JanToJun_ReturnsSixMonths()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);
        var gridlines = TimelineCalculator.GetMonthGridlines(start, end);

        Assert.Equal(6, gridlines.Count);
        Assert.Equal("Jan", gridlines[0].Label);
        Assert.Equal("Feb", gridlines[1].Label);
        Assert.Equal("Mar", gridlines[2].Label);
        Assert.Equal("Apr", gridlines[3].Label);
        Assert.Equal("May", gridlines[4].Label);
        Assert.Equal("Jun", gridlines[5].Label);
    }

    [Fact]
    public void GetMonthGridlines_MidMonthStart_SkipsPartialMonth()
    {
        var start = new DateTime(2026, 1, 15);
        var end = new DateTime(2026, 6, 30);
        var gridlines = TimelineCalculator.GetMonthGridlines(start, end);

        Assert.Equal(5, gridlines.Count);
        Assert.Equal("Feb", gridlines[0].Label);
    }

    [Fact]
    public void GetMonthGridlines_ExactMonthBoundary_IncludesStartMonth()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 3, 31);
        var gridlines = TimelineCalculator.GetMonthGridlines(start, end);

        Assert.Equal(3, gridlines.Count);
        Assert.Equal("Jan", gridlines[0].Label);
    }

    [Fact]
    public void GetMonthGridlines_SingleMonth_ReturnsOneGridline()
    {
        var start = new DateTime(2026, 3, 1);
        var end = new DateTime(2026, 3, 31);
        var gridlines = TimelineCalculator.GetMonthGridlines(start, end);

        Assert.Single(gridlines);
        Assert.Equal("Mar", gridlines[0].Label);
    }

    [Fact]
    public void GetMonthGridlines_CrossesYearBoundary_HandlesCorrectly()
    {
        var start = new DateTime(2025, 11, 1);
        var end = new DateTime(2026, 2, 28);
        var gridlines = TimelineCalculator.GetMonthGridlines(start, end);

        Assert.Equal(4, gridlines.Count);
        Assert.Equal("Nov", gridlines[0].Label);
        Assert.Equal("Dec", gridlines[1].Label);
        Assert.Equal("Jan", gridlines[2].Label);
        Assert.Equal("Feb", gridlines[3].Label);
    }

    [Fact]
    public void GetMonthGridlines_DatesAreFirstOfMonth()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);
        var gridlines = TimelineCalculator.GetMonthGridlines(start, end);

        foreach (var gl in gridlines)
        {
            Assert.Equal(1, gl.Date.Day);
        }
    }

    // --- Integration: X positions for gridlines are within SVG bounds ---

    [Fact]
    public void MonthGridlines_AllXPositions_WithinSvgBounds()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 7, 1);
        var gridlines = TimelineCalculator.GetMonthGridlines(start, end);

        foreach (var gl in gridlines)
        {
            var x = TimelineCalculator.GetXPosition(gl.Date, start, end);
            Assert.InRange(x, 0, TimelineCalculator.SvgWidth);
        }
    }

    // --- Integration: Track Y positions within SVG bounds ---

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void GetTrackY_AllTracks_WithinSvgHeight(int trackCount)
    {
        for (var i = 0; i < trackCount; i++)
        {
            var y = TimelineCalculator.GetTrackY(i, trackCount);
            Assert.InRange(y, 0, TimelineCalculator.SvgHeight);
        }
    }
}