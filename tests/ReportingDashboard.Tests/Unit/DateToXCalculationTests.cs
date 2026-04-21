using FluentAssertions;
using ReportingDashboard.Web.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class DateToXCalculationTests
{
    private static readonly DateTime Start = new(2026, 1, 1);
    private static readonly DateTime End = new(2026, 6, 30);

    [Fact]
    [Trait("Category", "Unit")]
    public void StartDate_ReturnsZero()
    {
        var result = TimelineSection.CalculateDateToX(Start, Start, End);
        result.Should().Be(0.0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EndDate_Returns1560()
    {
        var result = TimelineSection.CalculateDateToX(End, Start, End);
        result.Should().Be(1560.0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MidpointDate_ReturnsApprox780()
    {
        var totalDays = (End - Start).TotalDays;
        var mid = Start.AddDays(totalDays / 2.0);
        var result = TimelineSection.CalculateDateToX(mid, Start, End);
        result.Should().BeApproximately(780.0, 1.0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SameStartAndEnd_ReturnsZero()
    {
        var same = new DateTime(2026, 3, 15);
        var result = TimelineSection.CalculateDateToX(same, same, same);
        result.Should().Be(0.0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DateBeforeStart_ReturnsNegative()
    {
        var before = Start.AddDays(-10);
        var result = TimelineSection.CalculateDateToX(before, Start, End);
        // The method does not clamp; it returns negative via linear interpolation
        result.Should().BeLessThan(0.0);
    }
}