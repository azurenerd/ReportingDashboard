using ReportingDashboard.Web.Components;

namespace ReportingDashboard.Tests.Calculations;

public class DateToXTests
{
    private static readonly DateTime StartDate = new(2025, 12, 19);
    private static readonly DateTime EndDate = new(2026, 6, 30);
    private const double SvgWidth = 1560.0;

    private double DateToX(string dateStr)
    {
        var date = DateTime.Parse(dateStr);
        return TimelineSection.DateToX(date, StartDate, EndDate, SvgWidth);
    }

    [Fact]
    public void StartDate_ReturnsZero()
    {
        var result = DateToX("2025-12-19");
        Assert.Equal(0.0, result, precision: 1);
    }

    [Fact]
    public void EndDate_ReturnsSvgWidth()
    {
        var result = DateToX("2026-06-30");
        Assert.Equal(SvgWidth, result, precision: 1);
    }

    [Fact]
    public void Jan12_ReturnsExpectedPosition()
    {
        // Jan 12 is 24 days from Dec 19, total span is 193 days
        var result = DateToX("2026-01-12");
        var expected = 24.0 / 193.0 * SvgWidth;
        Assert.Equal(expected, result, precision: 1);
    }

    [Fact]
    public void Mar26_ReturnsExpectedPosition()
    {
        // Mar 26 is 97 days from Dec 19
        var result = DateToX("2026-03-26");
        var expected = 97.0 / 193.0 * SvgWidth;
        Assert.Equal(expected, result, precision: 1);
    }

    [Fact]
    public void Apr1_ReturnsMidRangeApprox()
    {
        var result = DateToX("2026-04-01");
        // 103 days from start out of 193 total
        var expected = 103.0 / 193.0 * SvgWidth;
        Assert.Equal(expected, result, precision: 1);
    }

    [Fact]
    public void MidPoint_ReturnsHalfSvgWidth()
    {
        // Midpoint date: Dec 19 + 96.5 days ≈ Mar 25
        var midDate = StartDate.AddDays(193.0 / 2);
        var result = TimelineSection.DateToX(midDate, StartDate, EndDate, SvgWidth);
        Assert.Equal(SvgWidth / 2, result, precision: 1);
    }

    [Fact]
    public void SameDates_ReturnsZero()
    {
        var sameDate = new DateTime(2026, 1, 1);
        var result = TimelineSection.DateToX(sameDate, sameDate, sameDate, SvgWidth);
        Assert.Equal(0.0, result, precision: 1);
    }
}
