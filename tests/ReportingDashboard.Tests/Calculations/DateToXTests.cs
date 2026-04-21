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
    public void DateToX_BoundaryDates_ReturnsCorrectPosition(string dateStr, double expectedX)
    {
        var date = DateTime.Parse(dateStr);
        var result = TimelineSection.CalculateDateToX(date, StartDate, EndDate);
        Assert.Equal(expectedX, Math.Round(result, 1));
    }

    [Fact]
    public void DateToX_MidRange_ReturnsApproximateMidpoint()
    {
        // April 1 is roughly in the middle of Jan 1 - Jun 30
        var date = new DateTime(2026, 4, 1);
        var result = TimelineSection.CalculateDateToX(date, StartDate, EndDate);
        // 90 days out of 180 = exactly half = 780
        Assert.Equal(780.0, Math.Round(result, 1));
    }

    [Fact]
    public void DateToX_KnownCheckpoint_Jan12()
    {
        // Jan 12 = 11 days from start. 11/180*1560 ≈ 95.3
        var date = new DateTime(2026, 1, 12);
        var result = TimelineSection.CalculateDateToX(date, StartDate, EndDate);
        var rounded = Math.Round(result, 1);
        Assert.InRange(rounded, 90.0, 110.0);
    }

    [Fact]
    public void DateToX_KnownPocMilestone_Mar26()
    {
        // Mar 26 = 84 days from start. 84/180*1560 = 728
        var date = new DateTime(2026, 3, 26);
        var result = TimelineSection.CalculateDateToX(date, StartDate, EndDate);
        var rounded = Math.Round(result, 1);
        Assert.InRange(rounded, 720.0, 750.0);
    }

    [Fact]
    public void DateToX_SameStartAndEnd_ReturnsZero()
    {
        var date = new DateTime(2026, 3, 15);
        var same = new DateTime(2026, 3, 15);
        var result = TimelineSection.CalculateDateToX(date, same, same);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void DateToX_QuarterPoint()
    {
        // Jan 1 to Jun 30 = 180 days. 45 days = quarter = 390
        var date = new DateTime(2026, 2, 15); // 45 days from Jan 1
        var result = TimelineSection.CalculateDateToX(date, StartDate, EndDate);
        Assert.Equal(390.0, Math.Round(result, 1));
    }
}
