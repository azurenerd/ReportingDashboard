using FluentAssertions;
using ReportingDashboard.Web.Components;

namespace ReportingDashboard.Tests.Unit;

public class DateToXTests
{
    private const string StartDate = "2026-01-01";
    private const string EndDate = "2026-07-01";
    private const double SvgWidth = 1560.0;

    [Fact]
    [Trait("Category", "Unit")]
    public void StartDate_MapsToZero()
    {
        var result = TimelineSection.DateToX(StartDate, StartDate, EndDate, SvgWidth);
        result.Should().Be(0.0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EndDate_MapsToSvgWidth()
    {
        var result = TimelineSection.DateToX(EndDate, StartDate, EndDate, SvgWidth);
        result.Should().Be(SvgWidth);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MidpointDate_MapsToApproximatelyHalfWidth()
    {
        var result = TimelineSection.DateToX("2026-04-01", StartDate, EndDate, SvgWidth);
        result.Should().BeApproximately(SvgWidth / 2.0, 30.0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SameStartAndEndDate_ReturnsZero()
    {
        var result = TimelineSection.DateToX("2026-01-01", "2026-01-01", "2026-01-01", SvgWidth);
        result.Should().Be(0.0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InvalidDateString_ReturnsZero()
    {
        var result = TimelineSection.DateToX("not-a-date", StartDate, EndDate, SvgWidth);
        result.Should().Be(0.0);
    }
}