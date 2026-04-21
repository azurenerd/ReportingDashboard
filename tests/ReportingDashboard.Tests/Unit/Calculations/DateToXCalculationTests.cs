using FluentAssertions;
using ReportingDashboard.Web.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Calculations;

public class DateToXCalculationTests
{
    private const double SvgWidth = 1560.0;

    [Fact]
    [Trait("Category", "Unit")]
    public void StartDate_MapsToZero()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);

        var result = TimelineSection.CalculateDateToX(start, start, end);

        result.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EndDate_MapsToSvgWidth()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);

        var result = TimelineSection.CalculateDateToX(end, start, end);

        result.Should().BeApproximately(SvgWidth, 0.1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MidpointDate_MapsToHalfWidth()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 7, 1);
        var mid = new DateTime(2026, 4, 1);

        var totalDays = (end - start).TotalDays;
        var midDays = (mid - start).TotalDays;
        var expected = midDays / totalDays * SvgWidth;

        var result = TimelineSection.CalculateDateToX(mid, start, end);

        result.Should().BeApproximately(expected, 0.1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SameDayRange_ReturnsZero()
    {
        var date = new DateTime(2026, 3, 15);

        var result = TimelineSection.CalculateDateToX(date, date, date);

        result.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void KnownDate_ComputesCorrectLinearInterpolation()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 12, 31);
        var target = new DateTime(2026, 4, 15);

        var totalDays = (end - start).TotalDays;
        var dayOffset = (target - start).TotalDays;
        var expected = dayOffset / totalDays * SvgWidth;

        var result = TimelineSection.CalculateDateToX(target, start, end);

        result.Should().BeApproximately(expected, 0.01);
    }
}