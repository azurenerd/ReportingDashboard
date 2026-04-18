using ReportingDashboard.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class TimelineCalculatorTests
{
    [Fact]
    public void DateToX_StartDate_ReturnsZero()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 12, 31);
        var result = TimelineCalculator.DateToX(start, start, end, 1560);
        Assert.Equal(0, result);
    }

    [Fact]
    public void DateToX_EndDate_ReturnsSvgWidth()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 12, 31);
        var result = TimelineCalculator.DateToX(end, start, end, 1560);
        Assert.Equal(1560, result);
    }

    [Fact]
    public void DateToX_BeforeStart_ClampsToZero()
    {
        var start = new DateOnly(2026, 6, 1);
        var end = new DateOnly(2026, 12, 31);
        var result = TimelineCalculator.DateToX(new DateOnly(2026, 1, 1), start, end, 1560);
        Assert.Equal(0, result);
    }

    [Fact]
    public void DistributeYPositions_ThreeMilestones_EvenlySpaced()
    {
        var positions = TimelineCalculator.DistributeYPositions(3, 185);
        Assert.Equal(3, positions.Length);
        Assert.Equal(positions[0], 185.0 / 4, precision: 5);
        Assert.Equal(positions[1], 185.0 / 4 * 2, precision: 5);
        Assert.Equal(positions[2], 185.0 / 4 * 3, precision: 5);
    }

    [Fact]
    public void DiamondPoints_ReturnsCorrectFormat()
    {
        var result = TimelineCalculator.DiamondPoints(100, 50, 11);
        Assert.Contains("100.00", result);
        Assert.Contains("39.00", result); // cy - halfSize = 50 - 11
        Assert.Contains("111.00", result); // cx + halfSize
    }
}