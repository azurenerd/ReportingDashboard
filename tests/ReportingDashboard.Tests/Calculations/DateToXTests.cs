using ReportingDashboard.Web.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Calculations;

public class DateToXTests
{
    [Fact]
    public void DateToX_StartDate_ReturnsZero()
    {
        var result = TimelineHelper.DateToX("2026-01-01", "2026-01-01", "2026-12-31", 1000);
        Assert.Equal(0, result, precision: 1);
    }

    [Fact]
    public void DateToX_EndDate_ReturnsTotalWidth()
    {
        var result = TimelineHelper.DateToX("2026-12-31", "2026-01-01", "2026-12-31", 1000);
        Assert.Equal(1000, result, precision: 1);
    }

    [Fact]
    public void DateToX_MidDate_ReturnsMidpoint()
    {
        var result = TimelineHelper.DateToX("2026-07-02", "2026-01-01", "2027-01-01", 365);
        Assert.InRange(result, 175, 190);
    }

    [Fact]
    public void DateToX_InvalidDate_ReturnsZero()
    {
        var result = TimelineHelper.DateToX("not-a-date", "2026-01-01", "2026-12-31", 1000);
        Assert.Equal(0, result);
    }

    [Fact]
    public void DateToX_InvalidStart_ReturnsZero()
    {
        var result = TimelineHelper.DateToX("2026-06-15", "bad", "2026-12-31", 1000);
        Assert.Equal(0, result);
    }

    [Fact]
    public void DateToX_EndBeforeStart_ReturnsZero()
    {
        var result = TimelineHelper.DateToX("2026-06-15", "2026-12-31", "2026-01-01", 1000);
        Assert.Equal(0, result);
    }

    [Fact]
    public void DateToX_BeforeStart_ClampedToZero()
    {
        var result = TimelineHelper.DateToX("2025-01-01", "2026-01-01", "2026-12-31", 1000);
        Assert.Equal(0, result);
    }

    [Fact]
    public void DateToX_AfterEnd_ClampedToWidth()
    {
        var result = TimelineHelper.DateToX("2028-01-01", "2026-01-01", "2026-12-31", 1000);
        Assert.Equal(1000, result);
    }
}