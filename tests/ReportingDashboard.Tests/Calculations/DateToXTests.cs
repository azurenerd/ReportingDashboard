using FluentAssertions;
using ReportingDashboard.Web.Components;
using Xunit;

namespace ReportingDashboard.Tests.Calculations;

public class DateToXTests
{
    private static readonly DateTime StartDate = new(2026, 1, 1);
    private static readonly DateTime EndDate = new(2026, 6, 30);
    private const double SvgWidth = 1560.0;

    [Theory]
    [InlineData("2026-01-01", 0.0)]
    [InlineData("2026-06-30", 1560.0)]
    public void DateToX_BoundaryDates_ReturnsExactBounds(string dateStr, double expectedX)
    {
        var date = DateTime.Parse(dateStr);
        var result = TimelineSection.DateToX(date, StartDate, EndDate, SvgWidth);
        result.Should().BeApproximately(expectedX, 0.1);
    }

    [Fact]
    public void DateToX_MidpointDate_ReturnsApproximateMidpoint()
    {
        // April 1 is roughly 50% through Jan 1 - Jun 30 (90 of 180 days)
        var date = new DateTime(2026, 4, 1);
        var result = TimelineSection.DateToX(date, StartDate, EndDate, SvgWidth);
        result.Should().BeApproximately(780.0, 1.0);
    }

    [Fact]
    public void DateToX_KnownCheckpoint_Jan12()
    {
        // Jan 12 = 11 days from start, 11/180 * 1560 ≈ 95.3
        var date = new DateTime(2026, 1, 12);
        var result = TimelineSection.DateToX(date, StartDate, EndDate, SvgWidth);
        result.Should().BeApproximately(95.3, 10.0);
    }

    [Fact]
    public void DateToX_KnownPoCMilestone_Mar26()
    {
        // Mar 26 = 84 days from start, 84/180 * 1560 ≈ 728.0
        var date = new DateTime(2026, 3, 26);
        var result = TimelineSection.DateToX(date, StartDate, EndDate, SvgWidth);
        result.Should().BeApproximately(728.0, 20.0);
    }

    [Fact]
    public void DateToX_EqualStartAndEnd_ReturnsZero()
    {
        var date = new DateTime(2026, 3, 15);
        var sameDate = new DateTime(2026, 3, 15);
        var result = TimelineSection.DateToX(date, sameDate, sameDate, SvgWidth);
        result.Should().Be(0.0);
    }

    [Fact]
    public void DateToX_DateBeforeStart_ReturnsNegative()
    {
        var date = new DateTime(2025, 12, 15);
        var result = TimelineSection.DateToX(date, StartDate, EndDate, SvgWidth);
        result.Should().BeLessThan(0);
    }

    [Fact]
    public void DateToX_DateAfterEnd_ReturnsGreaterThanWidth()
    {
        var date = new DateTime(2026, 7, 15);
        var result = TimelineSection.DateToX(date, StartDate, EndDate, SvgWidth);
        result.Should().BeGreaterThan(SvgWidth);
    }
}
