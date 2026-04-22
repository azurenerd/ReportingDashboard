using ReportingDashboard.Web.Components;
using Xunit;
using FluentAssertions;

namespace ReportingDashboard.Tests.Calculations;

public class DateToXTests
{
    private const string StartDate = "2026-01-01";
    private const string EndDate = "2026-06-30";
    private const double SvgWidth = 1560.0;

    [Theory]
    [InlineData("2026-01-01", 0.0)]
    [InlineData("2026-06-30", 1560.0)]
    public void DateToX_BoundaryDates_ReturnsCorrectPosition(string date, double expectedX)
    {
        var result = TimelineSection.DateToX(date, StartDate, EndDate, SvgWidth);
        result.Should().BeApproximately(expectedX, 0.5);
    }

    [Fact]
    public void DateToX_MidRange_ReturnsApproximateMidpoint()
    {
        // April 1 is roughly 50% through the Jan 1 - Jun 30 range (90/181 days)
        var result = TimelineSection.DateToX("2026-04-01", StartDate, EndDate, SvgWidth);
        result.Should().BeInRange(700, 850);
    }

    [Fact]
    public void DateToX_KnownCheckpoint_Jan12()
    {
        // Jan 12 = 11 days from start. Total = 181 days. x = 11/181 * 1560 ≈ 94.8
        var result = TimelineSection.DateToX("2026-01-12", StartDate, EndDate, SvgWidth);
        result.Should().BeApproximately(94.8, 10);
    }

    [Fact]
    public void DateToX_KnownPoC_Mar26()
    {
        // Mar 26 = 84 days from Jan 1. x = 84/181 * 1560 ≈ 724
        var result = TimelineSection.DateToX("2026-03-26", StartDate, EndDate, SvgWidth);
        result.Should().BeApproximately(724, 30);
    }

    [Fact]
    public void DateToX_InvalidDate_ReturnsNegative()
    {
        var result = TimelineSection.DateToX("not-a-date", StartDate, EndDate, SvgWidth);
        result.Should().BeLessThan(0);
    }

    [Fact]
    public void DateToX_InvalidStartDate_ReturnsNegative()
    {
        var result = TimelineSection.DateToX("2026-03-01", "bad", EndDate, SvgWidth);
        result.Should().BeLessThan(0);
    }

    [Fact]
    public void DateToX_SameStartAndEnd_ReturnsNegative()
    {
        var result = TimelineSection.DateToX("2026-01-01", "2026-01-01", "2026-01-01", SvgWidth);
        result.Should().BeLessThan(0);
    }
}
