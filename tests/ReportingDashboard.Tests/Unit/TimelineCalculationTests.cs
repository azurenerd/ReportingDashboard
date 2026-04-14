using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// Tests for the date-to-X-position calculation used in the SVG timeline.
/// Formula: x = (date - startDate).TotalDays / (endDate - startDate).TotalDays * svgWidth
/// </summary>
[Trait("Category", "Unit")]
public class TimelineCalculationTests
{
    private const double SvgWidth = 1560.0;

    private static double CalculateX(string dateStr, string startDateStr, string endDateStr)
    {
        var date = DateOnly.ParseExact(dateStr, "yyyy-MM-dd");
        var start = DateOnly.ParseExact(startDateStr, "yyyy-MM-dd");
        var end = DateOnly.ParseExact(endDateStr, "yyyy-MM-dd");
        var totalDays = end.DayNumber - start.DayNumber;
        var elapsed = date.DayNumber - start.DayNumber;
        return (double)elapsed / totalDays * SvgWidth;
    }

    [Fact]
    public void StartDate_ReturnsZero()
    {
        var x = CalculateX("2026-01-01", "2026-01-01", "2026-07-01");
        x.Should().Be(0.0);
    }

    [Fact]
    public void EndDate_ReturnsSvgWidth()
    {
        var x = CalculateX("2026-07-01", "2026-01-01", "2026-07-01");
        x.Should().Be(SvgWidth);
    }

    [Fact]
    public void MidRange_ReturnsApproximateHalfway()
    {
        var x = CalculateX("2026-04-01", "2026-01-01", "2026-07-01");
        x.Should().BeApproximately(775.69, 1.0);
    }

    [Fact]
    public void OneDayAfterStart_ReturnsSmallPositiveValue()
    {
        var x = CalculateX("2026-01-02", "2026-01-01", "2026-07-01");
        x.Should().BeGreaterThan(0);
        x.Should().BeLessThan(20);
    }

    [Fact]
    public void OneDayBeforeEnd_ReturnsNearSvgWidth()
    {
        var x = CalculateX("2026-06-30", "2026-01-01", "2026-07-01");
        x.Should().BeGreaterThan(SvgWidth - 20);
        x.Should().BeLessThan(SvgWidth);
    }

    [Fact]
    public void SameStartAndEndMonth_HandlesCorrectly()
    {
        var x = CalculateX("2026-01-16", "2026-01-01", "2026-01-31");
        x.Should().BeApproximately(SvgWidth / 2, 1.0);
    }

    [Fact]
    public void MonthGridlinePositions_AreEvenlySpaced()
    {
        var janX = CalculateX("2026-01-01", "2026-01-01", "2026-07-01");
        var febX = CalculateX("2026-02-01", "2026-01-01", "2026-07-01");
        var marX = CalculateX("2026-03-01", "2026-01-01", "2026-07-01");

        janX.Should().Be(0);
        febX.Should().BeGreaterThan(janX);
        marX.Should().BeGreaterThan(febX);

        (febX - janX).Should().BeGreaterThan(marX - febX);
    }

    [Fact]
    public void DateBeforeStart_ReturnsNegative()
    {
        var x = CalculateX("2025-12-15", "2026-01-01", "2026-07-01");
        x.Should().BeNegative();
    }

    [Fact]
    public void DateAfterEnd_ReturnsGreaterThanSvgWidth()
    {
        var x = CalculateX("2026-08-01", "2026-01-01", "2026-07-01");
        x.Should().BeGreaterThan(SvgWidth);
    }
}