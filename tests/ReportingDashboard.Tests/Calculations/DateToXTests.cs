using ReportingDashboard.Web.Components;

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
        Assert.Equal(expectedX, Math.Round(result, 1));
    }

    [Fact]
    public void DateToX_MidRange_ReturnsApproximateMidpoint()
    {
        // April 1 is roughly day 90 out of 180 total days
        var result = TimelineSection.DateToX("2026-04-01", StartDate, EndDate, SvgWidth);
        Assert.InRange(result, 750, 810);
    }

    [Fact]
    public void DateToX_KnownCheckpoint_ReturnsExpectedPosition()
    {
        // Jan 12 is day 11 out of 180 days => ~95.3
        var result = TimelineSection.DateToX("2026-01-12", StartDate, EndDate, SvgWidth);
        Assert.InRange(result, 90, 110);
    }

    [Fact]
    public void DateToX_KnownPocMilestone_ReturnsExpectedPosition()
    {
        // Mar 26 is day 84 out of 180 => ~728
        var result = TimelineSection.DateToX("2026-03-26", StartDate, EndDate, SvgWidth);
        Assert.InRange(result, 720, 750);
    }

    [Fact]
    public void DateToX_InvalidDate_ReturnsZero()
    {
        var result = TimelineSection.DateToX("not-a-date", StartDate, EndDate, SvgWidth);
        Assert.Equal(0, result);
    }

    [Fact]
    public void DateToX_SameStartAndEnd_ReturnsZero()
    {
        var result = TimelineSection.DateToX("2026-03-15", "2026-01-01", "2026-01-01", SvgWidth);
        Assert.Equal(0, result);
    }
}
