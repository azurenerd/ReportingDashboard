using Xunit;

namespace ReportingDashboard.Tests;

public class TimelineCalculationTests
{
    private const double SvgWidth = 1560.0;

    private static double CalculateX(string dateStr, string startStr, string endStr)
    {
        var date = DateOnly.ParseExact(dateStr, "yyyy-MM-dd");
        var start = DateOnly.ParseExact(startStr, "yyyy-MM-dd");
        var end = DateOnly.ParseExact(endStr, "yyyy-MM-dd");
        var totalDays = (double)(end.DayNumber - start.DayNumber);
        var elapsed = (double)(date.DayNumber - start.DayNumber);
        return elapsed / totalDays * SvgWidth;
    }

    [Fact]
    public void StartDate_ReturnsZero()
    {
        var x = CalculateX("2026-01-01", "2026-01-01", "2026-07-01");
        Assert.Equal(0.0, x, 1);
    }

    [Fact]
    public void EndDate_ReturnsSvgWidth()
    {
        var x = CalculateX("2026-07-01", "2026-01-01", "2026-07-01");
        Assert.Equal(SvgWidth, x, 1);
    }

    [Fact]
    public void MidRange_ReturnsApproximateHalf()
    {
        var x = CalculateX("2026-04-01", "2026-01-01", "2026-07-01");
        var expected = 90.0 / 181.0 * SvgWidth;
        Assert.Equal(expected, x, 1);
    }

    [Fact]
    public void BeforeStart_ReturnsNegative()
    {
        var x = CalculateX("2025-12-01", "2026-01-01", "2026-07-01");
        Assert.True(x < 0);
    }

    [Fact]
    public void AfterEnd_ReturnsGreaterThanWidth()
    {
        var x = CalculateX("2026-08-01", "2026-01-01", "2026-07-01");
        Assert.True(x > SvgWidth);
    }

    [Fact]
    public void MonthBoundary_Feb1()
    {
        var x = CalculateX("2026-02-01", "2026-01-01", "2026-07-01");
        var expected = 31.0 / 181.0 * SvgWidth;
        Assert.Equal(expected, x, 1);
    }

    [Fact]
    public void SameDayMilestones_ReturnSameX()
    {
        var x1 = CalculateX("2026-03-15", "2026-01-01", "2026-07-01");
        var x2 = CalculateX("2026-03-15", "2026-01-01", "2026-07-01");
        Assert.Equal(x1, x2);
    }
}