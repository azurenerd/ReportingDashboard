using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DateCalculationServiceTests
{
    private readonly DateCalculationService _service;
    private readonly Mock<ILogger<DateCalculationService>> _mockLogger;

    public DateCalculationServiceTests()
    {
        _mockLogger = new Mock<ILogger<DateCalculationService>>();
        _service = new DateCalculationService(_mockLogger.Object);
    }

    [Fact]
    public void GetDisplayMonths_ReturnsListWithFourMonths()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.GetDisplayMonths(currentDate);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void GetDisplayMonths_IncludesCurrentMonth()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.GetDisplayMonths(currentDate);

        result.Should().Contain(m => m.IsCurrentMonth && m.Name == "April");
    }

    [Fact]
    public void GetDisplayMonths_MarksPreviousMonthAsNotCurrent()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.GetDisplayMonths(currentDate);

        result.First().IsCurrentMonth.Should().BeFalse();
    }

    [Fact]
    public void GetDisplayMonths_ContainsCorrectMonthNames()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.GetDisplayMonths(currentDate);

        result.Select(m => m.Name).Should().Contain("March", "April", "May", "June");
    }

    [Fact]
    public void GetDisplayMonths_SetsGridColumnIndexCorrectly()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.GetDisplayMonths(currentDate);

        result[0].GridColumnIndex.Should().Be(0);
        result[1].GridColumnIndex.Should().Be(1);
    }

    [Fact]
    public void GetMilestoneXPosition_ReturnsPositionWithinSvgWidth()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 3, 15);

        var result = _service.GetMilestoneXPosition(milestoneDate, baselineDate);

        result.Should().BeGreaterThanOrEqualTo(0);
        result.Should().BeLessThanOrEqualTo(1560);
    }

    [Fact]
    public void GetMilestoneXPosition_IncreasesDatesInOrder()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var earlierDate = new DateTime(2026, 1, 15);
        var laterDate = new DateTime(2026, 2, 15);

        var earlierPos = _service.GetMilestoneXPosition(earlierDate, baselineDate);
        var laterPos = _service.GetMilestoneXPosition(laterDate, baselineDate);

        laterPos.Should().BeGreaterThan(earlierPos);
    }

    [Fact]
    public void GetMilestoneXPosition_ClampsNegativeToZero()
    {
        var baselineDate = new DateTime(2026, 6, 1);
        var pastDate = new DateTime(2026, 1, 1);

        var result = _service.GetMilestoneXPosition(pastDate, baselineDate);

        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetNowMarkerXPosition_ReturnsValidSvgPosition()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var currentDate = new DateTime(2026, 4, 12);

        var result = _service.GetNowMarkerXPosition(currentDate, baselineDate);

        result.Should().BeGreaterThan(0);
        result.Should().BeLessThanOrEqualTo(1560);
    }

    [Fact]
    public void IsCurrentMonth_ReturnsTrueForCurrentMonth()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.IsCurrentMonth("April", 2026, currentDate);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentMonth_ReturnsFalseForDifferentMonth()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.IsCurrentMonth("March", 2026, currentDate);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrentMonth_IsCaseInsensitive()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.IsCurrentMonth("april", 2026, currentDate);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentMonth_ReturnsFalseForDifferentYear()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.IsCurrentMonth("April", 2025, currentDate);

        result.Should().BeFalse();
    }

    [Fact]
    public void GetMonthBounds_ReturnsValidRange()
    {
        var (startX, endX) = _service.GetMonthBounds(0);

        startX.Should().BeLessThan(endX);
        startX.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetMonthBounds_ThrowsForInvalidIndex()
    {
        var action = () => _service.GetMonthBounds(6);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetMonthBounds_CalculatesConsistentWidths()
    {
        var (startX1, endX1) = _service.GetMonthBounds(0);
        var (startX2, endX2) = _service.GetMonthBounds(1);

        var width1 = endX1 - startX1;
        var width2 = endX2 - startX2;

        width1.Should().Be(width2);
    }

    [Fact]
    public void GetDisplayMonths_SetsStartAndEndDateCorrectly()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.GetDisplayMonths(currentDate);

        var aprilMonth = result.First(m => m.Name == "April");
        aprilMonth.StartDate.Should().Be(new DateTime(2026, 4, 1));
        aprilMonth.EndDate.Should().Be(new DateTime(2026, 4, 30));
    }

    [Fact]
    public void GetDisplayMonths_ContainsCorrectYear()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.GetDisplayMonths(currentDate);

        result.Should().AllSatisfy(m => m.Year.Should().Be(2026));
    }
}