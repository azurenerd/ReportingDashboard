using FluentAssertions;
using ReportingDashboard.Web.Components;
using Xunit;

namespace ReportingDashboard.Tests.Calculations;

public class DateToXTests
{
    private readonly DateTime _startDate = new(2026, 1, 1);
    private readonly DateTime _endDate = new(2026, 6, 30);

    [Theory]
    [InlineData("2026-01-01", 0.0)]
    [InlineData("2026-06-30", 1560.0)]
    public void DateToX_BoundaryDates_ReturnsExactPosition(string dateStr, double expectedX)
    {
        var date = DateTime.Parse(dateStr);
        var result = TimelineSection.DateToX(date, _startDate, _endDate);
        result.Should().BeApproximately(expectedX, 0.1);
    }

    [Fact]
    public void DateToX_MidRange_ReturnsApproximateMiddle()
    {
        var date = new DateTime(2026, 4, 1);
        var result = TimelineSection.DateToX(date, _startDate, _endDate);
        // Apr 1 is 90 days into 180-day range = 50%
        result.Should().BeApproximately(780.0, 1.0);
    }

    [Fact]
    public void DateToX_Jan12Checkpoint_ReturnsExpectedPosition()
    {
        var date = new DateTime(2026, 1, 12);
        var result = TimelineSection.DateToX(date, _startDate, _endDate);
        // 11 days into 180 days = ~95.3
        result.Should().BeApproximately(95.3, 10.0);
    }

    [Fact]
    public void DateToX_Mar26PoC_ReturnsExpectedPosition()
    {
        var date = new DateTime(2026, 3, 26);
        var result = TimelineSection.DateToX(date, _startDate, _endDate);
        // 84 days into 180 days = ~728
        result.Should().BeApproximately(728.0, 20.0);
    }

    [Fact]
    public void DateToX_BeforeStart_ReturnsNegative()
    {
        var date = new DateTime(2025, 12, 1);
        var result = TimelineSection.DateToX(date, _startDate, _endDate);
        result.Should().BeLessThan(0);
    }

    [Fact]
    public void DateToX_AfterEnd_ReturnsGreaterThanWidth()
    {
        var date = new DateTime(2026, 8, 1);
        var result = TimelineSection.DateToX(date, _startDate, _endDate);
        result.Should().BeGreaterThan(1560.0);
    }

    [Fact]
    public void DateToX_SameStartAndEnd_ReturnsZero()
    {
        var date = new DateTime(2026, 1, 1);
        var result = TimelineSection.DateToX(date, date, date);
        result.Should().Be(0);
    }
}
