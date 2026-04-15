namespace ReportingDashboard.Tests;

public class TimelineCalculationTests
{
    private const double SvgWidth = 1560.0;

    private static double CalculateX(string dateStr, string startStr, string endStr)
    {
        var date = DateOnly.ParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var start = DateOnly.ParseExact(startStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var end = DateOnly.ParseExact(endStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var totalDays = (double)(end.DayNumber - start.DayNumber);
        var elapsed = date.DayNumber - start.DayNumber;
        return (double)elapsed / totalDays * SvgWidth;
    }

    [Fact]
    public void CalculateX_StartDate_ReturnsZero()
    {
        var x = CalculateX("2026-01-01", "2026-01-01", "2026-07-01");
        Assert.Equal(0.0, x);
    }

    [Fact]
    public void CalculateX_EndDate_ReturnsSvgWidth()
    {
        var x = CalculateX("2026-07-01", "2026-01-01", "2026-07-01");
        Assert.Equal(SvgWidth, x, 2);
    }

    [Fact]
    public void CalculateX_MidRange_ReturnsApproximateHalf()
    {
        var x = CalculateX("2026-04-02", "2026-01-01", "2026-07-01");
        var expected = 91.0 / 181.0 * SvgWidth;
        Assert.Equal(expected, x, 2);
    }

    [Fact]
    public void CalculateX_BeforeStart_ReturnsNegative()
    {
        var x = CalculateX("2025-12-01", "2026-01-01", "2026-07-01");
        Assert.True(x < 0);
    }

    [Fact]
    public void CalculateX_AfterEnd_ReturnsGreaterThanWidth()
    {
        var x = CalculateX("2026-08-01", "2026-01-01", "2026-07-01");
        Assert.True(x > SvgWidth);
    }

    [Fact]
    public void MonthGridlines_CorrectCount()
    {
        var start = DateOnly.ParseExact("2026-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var end = DateOnly.ParseExact("2026-07-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var totalDays = (double)(end.DayNumber - start.DayNumber);

        var gridlines = new List<(string Label, double X)>();
        var cursor = new DateOnly(start.Year, start.Month, 1);
        if (cursor < start) cursor = cursor.AddMonths(1);
        while (cursor <= end)
        {
            var x = (cursor.DayNumber - start.DayNumber) / totalDays * SvgWidth;
            gridlines.Add((cursor.ToString("MMM", CultureInfo.InvariantCulture), x));
            cursor = cursor.AddMonths(1);
        }

        Assert.Equal(7, gridlines.Count);
        Assert.Equal("Jan", gridlines[0].Label);
        Assert.Equal(0.0, gridlines[0].X);
        Assert.Equal("Jul", gridlines[6].Label);
        Assert.Equal(SvgWidth, gridlines[6].X, 2);
    }

    [Fact]
    public void CurrentMonthName_FormatsCorrectly()
    {
        var date = new DateOnly(2026, 4, 15);
        Assert.Equal("Apr", date.ToString("MMM", CultureInfo.InvariantCulture));
    }
}