using Xunit;
using ReportingDashboard.Web.Components;

namespace ReportingDashboard.Tests.Calculations;

public class DateToXTests
{
    private static readonly DateTime StartDate = new(2026, 1, 1);
    private static readonly DateTime EndDate = new(2026, 6, 30);

    [Theory]
    [InlineData("2026-01-01", 0.0)]
    [InlineData("2026-06-30", 1560.0)]
    public void DateToX_BoundaryDates_ReturnsExactBoundary(string dateStr, double expectedX)
    {
        var date = DateTime.Parse(dateStr);
        var result = TimelineSection.CalculateDateToX(date, StartDate, EndDate);
        Assert.Equal(expectedX, Math.Round(result, 1));
    }

    [Fact]
    public void DateToX_MidpointDate_ReturnsApproximateMidpoint()
    {
        // April 1 is roughly midpoint of Jan 1 - Jun 30
        var date = new DateTime(2026, 4, 1);
        var result = TimelineSection.CalculateDateToX(date, StartDate, EndDate);
        // 90 days out of 180 total = 50% = 780
        Assert.Equal(780.0, Math.Round(result, 1));
    }

    [Fact]
    public void DateToX_Jan12Checkpoint_ReturnsExpectedPosition()
    {
        var date = new DateTime(2026, 1, 12);
        var result = TimelineSection.CalculateDateToX(date, StartDate, EndDate);
        // 11 days / 180 days * 1560 ≈ 95.3
        var rounded = Math.Round(result, 0);
        Assert.InRange(rounded, 90, 100);
    }

    [Fact]
    public void DateToX_Mar26Poc_ReturnsExpectedPosition()
    {
        var date = new DateTime(2026, 3, 26);
        var result = TimelineSection.CalculateDateToX(date, StartDate, EndDate);
        // 84 days / 180 days * 1560 ≈ 728
        var rounded = Math.Round(result, 0);
        Assert.InRange(rounded, 720, 740);
    }

    [Fact]
    public void DateToX_EqualStartAndEnd_ReturnsZero()
    {
        var date = new DateTime(2026, 3, 15);
        var sameDate = new DateTime(2026, 3, 15);
        var result = TimelineSection.CalculateDateToX(date, sameDate, sameDate);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void DateToX_BeforeStartDate_ReturnsNegative()
    {
        var date = new DateTime(2025, 12, 15);
        var result = TimelineSection.CalculateDateToX(date, StartDate, EndDate);
        Assert.True(result < 0);
    }

    [Fact]
    public void DateToX_AfterEndDate_ReturnsAboveSvgWidth()
    {
        var date = new DateTime(2026, 7, 15);
        var result = TimelineSection.CalculateDateToX(date, StartDate, EndDate);
        Assert.True(result > 1560.0);
    }
}
