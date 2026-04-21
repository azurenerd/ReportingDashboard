using FluentAssertions;
using ReportingDashboard.Web.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DateToXTests
{
    private const string Start = "2026-01-01";
    private const string End = "2026-07-01";
    private const double SvgWidth = 1560.0;

    [Fact]
    public void StartDate_ReturnsZero()
    {
        var result = TimelineHelper.DateToX(Start, Start, End, SvgWidth);
        result.Should().Be(0.0);
    }

    [Fact]
    public void EndDate_ReturnsSvgWidth()
    {
        var result = TimelineHelper.DateToX(End, Start, End, SvgWidth);
        result.Should().Be(SvgWidth);
    }

    [Fact]
    public void MidpointDate_ReturnsApproximatelyHalfWidth()
    {
        var startDt = DateTime.Parse(Start);
        var endDt = DateTime.Parse(End);
        var totalDays = (endDt - startDt).TotalDays;
        var midDate = startDt.AddDays(totalDays / 2).ToString("yyyy-MM-dd");

        var result = TimelineHelper.DateToX(midDate, Start, End, SvgWidth);

        result.Should().BeApproximately(SvgWidth / 2, 5.0);
    }

    [Fact]
    public void EqualStartAndEnd_ReturnsZero()
    {
        var sameDate = "2026-03-01";
        var result = TimelineHelper.DateToX(sameDate, sameDate, sameDate, SvgWidth);
        result.Should().Be(0.0);
    }

    [Fact]
    public void KnownDate_ReturnsCorrectLinearInterpolation()
    {
        var date = "2026-04-10";
        var startDt = DateTime.Parse(Start);
        var endDt = DateTime.Parse(End);
        var dateDt = DateTime.Parse(date);
        var totalDays = (endDt - startDt).TotalDays;
        var elapsed = (dateDt - startDt).TotalDays;
        var expected = elapsed / totalDays * SvgWidth;

        var result = TimelineHelper.DateToX(date, Start, End, SvgWidth);

        result.Should().BeApproximately(expected, 0.01);
    }
}