using Xunit;
using FluentAssertions;
using ReportingDashboard.Web.Components;

namespace ReportingDashboard.Tests.Calculations;

public class DateToXTests
{
    private readonly DateTime _start = new(2026, 1, 1);
    private readonly DateTime _end = new(2026, 6, 30);
    private const double SvgWidth = 1560.0;

    [Fact]
    public void StartDate_ReturnsZero()
    {
        var result = TimelineSection.ComputeDateToX(_start, _start, _end, SvgWidth);
        result.Should().BeApproximately(0.0, 0.1);
    }

    [Fact]
    public void EndDate_ReturnsSvgWidth()
    {
        var result = TimelineSection.ComputeDateToX(_end, _start, _end, SvgWidth);
        result.Should().BeApproximately(SvgWidth, 0.1);
    }

    [Fact]
    public void MidRange_ReturnsApproximateMiddle()
    {
        var midDate = new DateTime(2026, 4, 1);
        var result = TimelineSection.ComputeDateToX(midDate, _start, _end, SvgWidth);
        // 90 days / 180 days * 1560 = 780
        result.Should().BeApproximately(780.0, 1.0);
    }

    [Fact]
    public void Jan12Checkpoint_ReturnsExpectedPosition()
    {
        var date = new DateTime(2026, 1, 12);
        var result = TimelineSection.ComputeDateToX(date, _start, _end, SvgWidth);
        // 11 days / 180 days * 1560 ≈ 95.3
        result.Should().BeInRange(90.0, 110.0);
    }

    [Fact]
    public void Mar26PoC_ReturnsExpectedPosition()
    {
        var date = new DateTime(2026, 3, 26);
        var result = TimelineSection.ComputeDateToX(date, _start, _end, SvgWidth);
        // 84 days / 180 days * 1560 ≈ 728
        result.Should().BeInRange(710.0, 760.0);
    }

    [Fact]
    public void BeforeStartDate_ReturnsNegative()
    {
        var date = new DateTime(2025, 12, 19);
        var result = TimelineSection.ComputeDateToX(date, _start, _end, SvgWidth);
        result.Should().BeLessThan(0);
    }

    [Fact]
    public void SameStartAndEnd_ReturnsZero()
    {
        var same = new DateTime(2026, 1, 1);
        var result = TimelineSection.ComputeDateToX(same, same, same, SvgWidth);
        result.Should().Be(0);
    }
}
