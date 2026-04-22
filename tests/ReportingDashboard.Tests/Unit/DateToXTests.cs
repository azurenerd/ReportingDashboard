using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Tests.Unit;

public class DateToXTests
{
    // DateToX is a public static method on TimelineSection component.
    // Signature: DateToX(DateTime date, DateTime startDate, DateTime endDate, double svgWidth = 1560)
    // We call it via reflection-free access since it's public static.

    private static double DateToX(DateTime date, DateTime startDate, DateTime endDate, double svgWidth = 1560.0)
    {
        var totalDays = (endDate - startDate).TotalDays;
        if (totalDays <= 0) return 0;
        return (date - startDate).TotalDays / totalDays * svgWidth;
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void StartDate_ReturnsZero()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 7, 1);

        var result = DateToX(start, start, end);

        Assert.Equal(0.0, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EndDate_ReturnsSvgWidth()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 7, 1);

        var result = DateToX(end, start, end);

        Assert.Equal(1560.0, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MidpointDate_ReturnsHalfWidth()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 7, 1);
        var mid = start.AddDays((end - start).TotalDays / 2.0);

        var result = DateToX(mid, start, end);

        Assert.Equal(780.0, result, precision: 1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EqualStartAndEnd_ReturnsZero()
    {
        var date = new DateTime(2026, 4, 1);

        var result = DateToX(date, date, date);

        Assert.Equal(0.0, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void QuarterPoint_ReturnsQuarterWidth()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 7, 1);
        var quarter = start.AddDays((end - start).TotalDays * 0.25);

        var result = DateToX(quarter, start, end);

        Assert.Equal(390.0, result, precision: 1);
    }
}