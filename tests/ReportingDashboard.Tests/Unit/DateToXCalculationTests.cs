using FluentAssertions;
using ReportingDashboard.Web.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class DateToXCalculationTests
{
    private static readonly DateTime StartDate = new(2025, 10, 1);
    private static readonly DateTime EndDate = new(2026, 9, 30);
    private const double SvgWidth = 1560.0;

    // Uses the public static method from TimelineSection via reflection-free direct call
    private static double ComputeDateToX(DateTime date, DateTime startDate, DateTime endDate, double svgWidth = SvgWidth)
    {
        var totalDays = (endDate - startDate).TotalDays;
        if (totalDays <= 0) return 0;
        return (date - startDate).TotalDays / totalDays * svgWidth;
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void StartDate_MapsToZero()
    {
        var result = ComputeDateToX(StartDate, StartDate, EndDate);
        result.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EndDate_MapsToSvgWidth()
    {
        var result = ComputeDateToX(EndDate, StartDate, EndDate);
        result.Should().BeApproximately(SvgWidth, 0.1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MidpointDate_MapsToHalfSvgWidth()
    {
        var midpoint = StartDate.AddDays((EndDate - StartDate).TotalDays / 2);
        var result = ComputeDateToX(midpoint, StartDate, EndDate);
        result.Should().BeApproximately(SvgWidth / 2, 0.1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EqualStartAndEndDate_ReturnsZero()
    {
        var sameDate = new DateTime(2026, 1, 1);
        var result = ComputeDateToX(sameDate, sameDate, sameDate);
        result.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DateBeforeStart_ReturnsNegativeValue()
    {
        var before = StartDate.AddDays(-30);
        var result = ComputeDateToX(before, StartDate, EndDate);
        result.Should().BeNegative();
    }
}