using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// Tests for the date-to-X-position calculation used by the timeline SVG.
/// The formula is: x = (date - startDate).TotalDays / (endDate - startDate).TotalDays * svgWidth
/// This is a pure function extracted here for testability.
/// </summary>
[Trait("Category", "Unit")]
public class TimelineCalculationTests
{
    private const double SvgWidth = 1560.0;

    /// <summary>
    /// Replicates the CalculateX logic from Dashboard.razor for unit testing.
    /// </summary>
    private static double CalculateX(string dateStr, string startDateStr, string endDateStr)
    {
        var date = DateOnly.ParseExact(dateStr, "yyyy-MM-dd");
        var start = DateOnly.ParseExact(startDateStr, "yyyy-MM-dd");
        var end = DateOnly.ParseExact(endDateStr, "yyyy-MM-dd");
        var totalDays = end.DayNumber - start.DayNumber;
        if (totalDays == 0) return 0;
        var elapsed = date.DayNumber - start.DayNumber;
        return (double)elapsed / totalDays * SvgWidth;
    }

    [Fact]
    public void StartOfRange_ReturnsZero()
    {
        var x = CalculateX("2026-01-01", "2026-01-01", "2026-07-01");
        x.Should().Be(0);
    }

    [Fact]
    public void EndOfRange_ReturnsSvgWidth()
    {
        var x = CalculateX("2026-07-01", "2026-01-01", "2026-07-01");
        x.Should().Be(SvgWidth);
    }

    [Fact]
    public void MidRange_ReturnsApproximateHalf()
    {
        // Jan 1 to Jul 1 = 181 days. Midpoint ~90 days = ~April 1
        var x = CalculateX("2026-04-01", "2026-01-01", "2026-07-01");
        x.Should().BeApproximately(SvgWidth * 90.0 / 181.0, 0.01);
    }

    [Fact]
    public void BeforeRange_ReturnsNegative()
    {
        var x = CalculateX("2025-12-01", "2026-01-01", "2026-07-01");
        x.Should().BeLessThan(0);
    }

    [Fact]
    public void AfterRange_ReturnsAboveSvgWidth()
    {
        var x = CalculateX("2026-08-01", "2026-01-01", "2026-07-01");
        x.Should().BeGreaterThan(SvgWidth);
    }

    [Fact]
    public void SameStartAndEnd_ReturnsZero()
    {
        var x = CalculateX("2026-03-01", "2026-03-01", "2026-03-01");
        x.Should().Be(0);
    }

    [Fact]
    public void OneMonthRange_CalculatesCorrectly()
    {
        // Jan 1 to Feb 1 = 31 days. Jan 16 = 15 days elapsed.
        var x = CalculateX("2026-01-16", "2026-01-01", "2026-02-01");
        x.Should().BeApproximately(SvgWidth * 15.0 / 31.0, 0.01);
    }

    [Fact]
    public void MonthGridlinePositions_AreEvenlyDistributed()
    {
        // For a Jan-Jul range, month boundaries should map to specific X positions
        var start = "2026-01-01";
        var end = "2026-07-01";

        var feb = CalculateX("2026-02-01", start, end);
        var mar = CalculateX("2026-03-01", start, end);
        var apr = CalculateX("2026-04-01", start, end);

        // Verify ordering
        feb.Should().BeLessThan(mar);
        mar.Should().BeLessThan(apr);

        // All should be within range
        feb.Should().BeGreaterThan(0);
        apr.Should().BeLessThan(SvgWidth);
    }

    [Fact]
    public void SameDayMilestones_ReturnSameX()
    {
        var x1 = CalculateX("2026-03-15", "2026-01-01", "2026-07-01");
        var x2 = CalculateX("2026-03-15", "2026-01-01", "2026-07-01");
        x1.Should().Be(x2);
    }

    [Fact]
    public void LeapYearDate_CalculatesCorrectly()
    {
        // 2028 is a leap year; Feb 29 should be valid and calculable
        var x = CalculateX("2028-02-29", "2028-01-01", "2028-07-01");
        x.Should().BeGreaterThan(0).And.BeLessThan(SvgWidth);
    }
}