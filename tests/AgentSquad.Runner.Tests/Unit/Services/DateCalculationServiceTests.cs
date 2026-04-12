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

    public DateCalculationServiceTests()
    {
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        _service = new DateCalculationService(mockLogger.Object);
    }

    [Fact]
    public void GetDisplayMonths_ReturnsMonthsStartingFromCurrentMonth()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var months = _service.GetDisplayMonths(currentDate);

        months.Should().NotBeEmpty();
        months.Should().HaveCountGreaterThanOrEqualTo(4);
        months[0].Name.Should().Be("April");
        months[0].Year.Should().Be(2026);
        months[0].IsCurrentMonth.Should().BeTrue();
    }

    [Fact]
    public void GetDisplayMonths_MarksCurrent_MonthCorrectly()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var months = _service.GetDisplayMonths(currentDate);

        var currentMonthResult = months.FirstOrDefault(m => m.IsCurrentMonth);
        currentMonthResult.Should().NotBeNull();
        currentMonthResult!.Name.Should().Be("April");
    }

    [Fact]
    public void GetDisplayMonths_AssignsGridColumnIndices()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var months = _service.GetDisplayMonths(currentDate);

        for (int i = 0; i < months.Count; i++)
        {
            months[i].GridColumnIndex.Should().Be(i);
        }
    }

    [Fact]
    public void GetDisplayMonths_CalculatesMonthBounds()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var months = _service.GetDisplayMonths(currentDate);

        months.ForEach(m =>
        {
            m.StartDate.Day.Should().Be(1);
            m.EndDate.Month.Should().Be(m.StartDate.Month);
            m.EndDate.Should().BeOnOrAfter(m.StartDate);
        });
    }

    [Fact]
    public void IsCurrentMonth_ReturnsTrueForCurrentMonth()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.IsCurrentMonth("April", 2026, currentDate);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentMonth_ReturnsFalseForPastMonth()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.IsCurrentMonth("March", 2026, currentDate);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrentMonth_ReturnsFalseForFutureMonth()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = _service.IsCurrentMonth("May", 2026, currentDate);

        result.Should().BeFalse();
    }

    [Fact]
    public void GetMonthBounds_ReturnsValidXCoordinates()
    {
        var bounds = _service.GetMonthBounds(0);

        bounds.startX.Should().BeGreaterThanOrEqualTo(0);
        bounds.endX.Should().BeGreaterThan(bounds.startX);
    }

    [Fact]
    public void GetMonthBounds_IncreasingIndexesProduceRightwardBounds()
    {
        var bounds0 = _service.GetMonthBounds(0);
        var bounds1 = _service.GetMonthBounds(1);

        bounds1.startX.Should().BeGreaterThan(bounds0.startX);
        bounds1.endX.Should().BeGreaterThan(bounds0.endX);
    }

    [Fact]
    public void GetMilestoneXPosition_CalculatesPositionForMilestoneDate()
    {
        var baselineDate = new DateTime(2026, 3, 1);
        var milestoneDate = new DateTime(2026, 4, 15);

        var position = _service.GetMilestoneXPosition(milestoneDate, baselineDate);

        position.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetMilestoneXPosition_ReturnsEqualPositionForSameDates()
    {
        var date = new DateTime(2026, 4, 15);

        var position = _service.GetMilestoneXPosition(date, date);

        position.Should().Be(0);
    }

    [Fact]
    public void GetNowMarkerXPosition_CalculatesCurrentDatePosition()
    {
        var baselineDate = new DateTime(2026, 3, 1);
        var currentDate = new DateTime(2026, 4, 15);

        var position = _service.GetNowMarkerXPosition(currentDate, baselineDate);

        position.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetNowMarkerXPosition_WithEarlyApril_ReturnsValidPosition()
    {
        var baselineDate = new DateTime(2026, 3, 1);
        var currentDate = new DateTime(2026, 4, 1);

        var position = _service.GetNowMarkerXPosition(currentDate, baselineDate);

        position.Should().BeGreaterThanOrEqualTo(0);
    }
}