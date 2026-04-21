using FluentAssertions;
using ReportingDashboard.Web.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DateToXCalculationTests
{
    private static readonly DateTime StartDate = new(2025, 10, 1);
    private static readonly DateTime EndDate = new(2026, 9, 30);
    private const double SvgWidth = 1560.0;

    [Fact]
    public void ComputeDateToX_StartDate_ReturnsZero()
    {
        var result = TimelineSection.ComputeDateToX(StartDate, StartDate, EndDate, SvgWidth);

        result.Should().Be(0);
    }

    [Fact]
    public void ComputeDateToX_EndDate_ReturnsSvgWidth()
    {
        var result = TimelineSection.ComputeDateToX(EndDate, StartDate, EndDate, SvgWidth);

        result.Should().BeApproximately(SvgWidth, 0.1);
    }

    [Fact]
    public void ComputeDateToX_MidpointDate_ReturnsApproximatelyHalfWidth()
    {
        var midpoint = StartDate.AddDays((EndDate - StartDate).TotalDays / 2.0);

        var result = TimelineSection.ComputeDateToX(midpoint, StartDate, EndDate, SvgWidth);

        result.Should().BeApproximately(SvgWidth / 2.0, 1.0);
    }

    [Fact]
    public void ComputeDateToX_DateBeforeStart_ReturnsNegative()
    {
        var beforeStart = StartDate.AddDays(-30);

        var result = TimelineSection.ComputeDateToX(beforeStart, StartDate, EndDate, SvgWidth);

        result.Should().BeLessThan(0);
    }

    [Fact]
    public void ComputeDateToX_EqualStartAndEnd_ReturnsZero()
    {
        var sameDate = new DateTime(2026, 1, 1);

        var result = TimelineSection.ComputeDateToX(sameDate, sameDate, sameDate, SvgWidth);

        result.Should().Be(0);
    }
}