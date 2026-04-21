using FluentAssertions;
using ReportingDashboard.Web.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DateToXTests
{
    private static readonly DateTime Start = new(2026, 1, 1);
    private static readonly DateTime End = new(2026, 7, 1);
    private const double SvgWidth = 1560.0;

    [Fact]
    public void StartDate_ReturnsZero()
    {
        var result = TimelineSection.DateToX(Start, Start, End, SvgWidth);
        result.Should().Be(0.0);
    }

    [Fact]
    public void EndDate_ReturnsSvgWidth()
    {
        var result = TimelineSection.DateToX(End, Start, End, SvgWidth);
        result.Should().Be(SvgWidth);
    }

    [Fact]
    public void MidpointDate_ReturnsApproximatelyHalfWidth()
    {
        var totalDays = (End - Start).TotalDays;
        var midDate = Start.AddDays(totalDays / 2);

        var result = TimelineSection.DateToX(midDate, Start, End, SvgWidth);

        result.Should().BeApproximately(SvgWidth / 2, 0.01);
    }

    [Fact]
    public void EqualStartAndEnd_ReturnsZero()
    {
        var sameDate = new DateTime(2026, 3, 1);
        var result = TimelineSection.DateToX(sameDate, sameDate, sameDate, SvgWidth);
        result.Should().Be(0.0);
    }

    [Fact]
    public void KnownDate_ReturnsCorrectLinearInterpolation()
    {
        // Apr 10 in a Jan 1 – Jul 1 range (181 days total, 99 days elapsed)
        var date = new DateTime(2026, 4, 10);
        var totalDays = (End - Start).TotalDays; // 181
        var elapsed = (date - Start).TotalDays;  // 99
        var expected = elapsed / totalDays * SvgWidth;

        var result = TimelineSection.DateToX(date, Start, End, SvgWidth);

        result.Should().BeApproximately(expected, 0.01);
    }
}