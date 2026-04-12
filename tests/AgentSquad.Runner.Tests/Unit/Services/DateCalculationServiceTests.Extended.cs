using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DateCalculationServiceExtendedTests
{
    private readonly DateCalculationService _service;
    private readonly Mock<ILogger<DateCalculationService>> _mockLogger;

    public DateCalculationServiceExtendedTests()
    {
        _mockLogger = new Mock<ILogger<DateCalculationService>>();
        _service = new DateCalculationService(_mockLogger.Object);
    }

    #region GetDisplayMonths Extended Tests

    [Fact]
    public void GetDisplayMonths_WithJanuaryDate_ReturnsJanuaryThroughApril()
    {
        var currentDate = new DateTime(2026, 1, 5);

        var months = _service.GetDisplayMonths(currentDate);

        months.Should().HaveCount(4);
        months[0].Name.Should().Be("January");
        months[1].Name.Should().Be("February");
        months[2].Name.Should().Be("March");
        months[3].Name.Should().Be("April");
    }

    [Fact]
    public void GetDisplayMonths_WithFebruaryDate_ReturnsFebruaryThroughMay()
    {
        var currentDate = new DateTime(2026, 2, 14);

        var months = _service.GetDisplayMonths(currentDate);

        months.Should().HaveCount(4);
        months[0].Name.Should().Be("February");
        months[0].IsCurrentMonth.Should().BeTrue();
        months[1].Name.Should().Be("March");
        months[2].Name.Should().Be("April");
        months[3].Name.Should().Be("May");
    }

    [Fact]
    public void GetDisplayMonths_WithDecemberDate_WrapsToNextYear()
    {
        var currentDate = new DateTime(2025, 12, 20);

        var months = _service.GetDisplayMonths(currentDate);

        months.Should().HaveCount(4);
        months[0].Name.Should().Be("December");
        months[0].Year.Should().Be(2025);
        months[1].Name.Should().Be("January");
        months[1].Year.Should().Be(2026);
        months[2].Name.Should().Be("February");
        months[2].Year.Should().Be(2026);
        months[3].Name.Should().Be("March");
        months[3].Year.Should().Be(2026);
    }

    [Fact]
    public void GetDisplayMonths_GridColumnIndicesAreSequential()
    {
        var currentDate = new DateTime(2026, 4, 12);

        var months = _service.GetDisplayMonths(currentDate);

        for (int i = 0; i < months.Count; i++)
        {
            months[i].GridColumnIndex.Should().Be(i);
        }
    }

    [Fact]
    public void GetDisplayMonths_OnlyCurrentMonthIsMarked()
    {
        var currentDate = new DateTime(2026, 4, 12);

        var months = _service.GetDisplayMonths(currentDate);

        var currentMonths = months.Where(m => m.IsCurrentMonth).ToList();
        currentMonths.Should().HaveCount(1);
        currentMonths[0].Name.Should().Be("April");
    }

    [Fact]
    public void GetDisplayMonths_EachMonthHasCorrectDayRange()
    {
        var currentDate = new DateTime(2026, 4, 12);

        var months = _service.GetDisplayMonths(currentDate);

        months[1].StartDate.Should().Be(new DateTime(2026, 4, 1));
        months[1].EndDate.Should().Be(new DateTime(2026, 4, 30));

        months[2].StartDate.Should().Be(new DateTime(2026, 5, 1));
        months[2].EndDate.Should().Be(new DateTime(2026, 5, 31));

        months[3].StartDate.Should().Be(new DateTime(2026, 6, 1));
        months[3].EndDate.Should().Be(new DateTime(2026, 6, 30));
    }

    [Fact]
    public void GetDisplayMonths_FebruaryInLeapYear()
    {
        var currentDate = new DateTime(2024, 2, 15);

        var months = _service.GetDisplayMonths(currentDate);

        var februaryMonth = months.First(m => m.Name == "February");
        februaryMonth.EndDate.Day.Should().Be(29);
    }

    [Fact]
    public void GetDisplayMonths_FebruaryInNonLeapYear()
    {
        var currentDate = new DateTime(2026, 2, 15);

        var months = _service.GetDisplayMonths(currentDate);

        var februaryMonth = months.First(m => m.Name == "February");
        februaryMonth.EndDate.Day.Should().Be(28);
    }

    #endregion

    #region GetMilestoneXPosition Extended Tests

    [Fact]
    public void GetMilestoneXPosition_WithJanuaryFirst_ReturnsZero()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 1, 1);

        var position = _service.GetMilestoneXPosition(milestoneDate, baselineDate);

        position.Should().Be(0);
    }

    [Fact]
    public void GetMilestoneXPosition_WithFebruaryFirst_ReturnsApproximately260()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 2, 1);

        var position = _service.GetMilestoneXPosition(milestoneDate, baselineDate);

        position.Should().BeGreaterThan(240);
        position.Should().BeLessThan(280);
    }

    [Fact]
    public void GetMilestoneXPosition_WithMarchFirst_ReturnsApproximately520()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 3, 1);

        var position = _service.GetMilestoneXPosition(milestoneDate, baselineDate);

        position.Should().BeGreaterThan(500);
        position.Should().BeLessThan(540);
    }

    [Fact]
    public void GetMilestoneXPosition_WithAprilFirst_ReturnsApproximately780()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 4, 1);

        var position = _service.GetMilestoneXPosition(milestoneDate, baselineDate);

        position.Should().BeGreaterThan(760);
        position.Should().BeLessThan(800);
    }

    [Fact]
    public void GetMilestoneXPosition_WithJuneThirtieth_ReturnsAtOrNear1560()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 6, 30);

        var position = _service.GetMilestoneXPosition(milestoneDate, baselineDate);

        position.Should().BeGreaterThanOrEqualTo(1500);
        position.Should().BeLessThanOrEqualTo(1560);
    }

    [Fact]
    public void GetMilestoneXPosition_PositionIncreasesWithLaterDates()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var earlierDate = new DateTime(2026, 3, 15);
        var laterDate = new DateTime(2026, 4, 15);

        var positionEarlier = _service.GetMilestoneXPosition(earlierDate, baselineDate);
        var positionLater = _service.GetMilestoneXPosition(laterDate, baselineDate);

        positionLater.Should().BeGreaterThan(positionEarlier);
    }

    [Fact]
    public void GetMilestoneXPosition_WithApril12_ReturnsValidPosition()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 4, 12);

        var position = _service.GetMilestoneXPosition(milestoneDate, baselineDate);

        position.Should().BeGreaterThan(700);
        position.Should().BeLessThan(900);
    }

    #endregion

    #region GetNowMarkerXPosition Extended Tests

    [Fact]
    public void GetNowMarkerXPosition_WithApril12And_JanuaryBaseline_ReturnsValidPosition()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var currentDate = new DateTime(2026, 4, 12);

        var position = _service.GetNowMarkerXPosition(currentDate, baselineDate);

        position.Should().BeGreaterThan(700);
        position.Should().BeLessThan(900);
    }

    [Fact]
    public void GetNowMarkerXPosition_MatchesMilestonePositionForSameDate()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var date = new DateTime(2026, 4, 12);

        var nowPosition = _service.GetNowMarkerXPosition(date, baselineDate);
        var milestonePosition = _service.GetMilestoneXPosition(date, baselineDate);

        nowPosition.Should().Be(milestonePosition);
    }

    [Fact]
    public void GetNowMarkerXPosition_WithEarlyJanuary_ReturnsSmallValue()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var currentDate = new DateTime(2026, 1, 5);

        var position = _service.GetNowMarkerXPosition(currentDate, baselineDate);

        position.Should().BeGreaterThanOrEqualTo(0);
        position.Should().BeLessThan(100);
    }

    [Fact]
    public void GetNowMarkerXPosition_WithLateJune_ReturnsLargeValue()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var currentDate = new DateTime(2026, 6, 25);

        var position = _service.GetNowMarkerXPosition(currentDate, baselineDate);

        position.Should().BeGreaterThan(1400);
    }

    #endregion

    #region IsCurrentMonth Extended Tests

    [Fact]
    public void IsCurrentMonth_WithAprilYearMonth_ReturnsTrueForApril()
    {
        var currentDate = new DateTime(2026, 4, 12);
        var aprilMonth = new YearMonth(2026, 4);

        var result = _service.IsCurrentMonth(aprilMonth, currentDate);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentMonth_WithMarchYearMonth_ReturnsFalseForApril()
    {
        var currentDate = new DateTime(2026, 4, 12);
        var marchMonth = new YearMonth(2026, 3);

        var result = _service.IsCurrentMonth(marchMonth, currentDate);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrentMonth_WithMayYearMonth_ReturnsFalseForApril()
    {
        var currentDate = new DateTime(2026, 4, 12);
        var mayMonth = new YearMonth(2026, 5);

        var result = _service.IsCurrentMonth(mayMonth, currentDate);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrentMonth_WithWrongYear_ReturnsFalse()
    {
        var currentDate = new DateTime(2026, 4, 12);
        var aprilMonth2025 = new YearMonth(2025, 4);

        var result = _service.IsCurrentMonth(aprilMonth2025, currentDate);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsCurrentMonth_WithDayOfMonth_DoesNotAffectResult()
    {
        var aprilMonth = new YearMonth(2026, 4);
        
        var dayOne = _service.IsCurrentMonth(aprilMonth, new DateTime(2026, 4, 1));
        var dayMid = _service.IsCurrentMonth(aprilMonth, new DateTime(2026, 4, 15));
        var dayEnd = _service.IsCurrentMonth(aprilMonth, new DateTime(2026, 4, 30));

        dayOne.Should().Be(dayMid);
        dayMid.Should().Be(dayEnd);
    }

    #endregion

    #region GetMonthBounds Extended Tests

    [Theory]
    [InlineData(0, 0, 260)]
    [InlineData(1, 260, 520)]
    [InlineData(2, 520, 780)]
    [InlineData(3, 780, 1040)]
    [InlineData(4, 1040, 1300)]
    [InlineData(5, 1300, 1560)]
    public void GetMonthBounds_ReturnsCorrectPixelRanges(int monthIndex, int expectedStart, int expectedEnd)
    {
        var (startX, endX) = _service.GetMonthBounds(monthIndex);

        startX.Should().Be(expectedStart);
        endX.Should().Be(expectedEnd);
    }

    [Fact]
    public void GetMonthBounds_EndXMinusStartXAlwaysEquals260()
    {
        for (int i = 0; i <= 5; i++)
        {
            var (startX, endX) = _service.GetMonthBounds(i);
            (endX - startX).Should().Be(260);
        }
    }

    [Fact]
    public void GetMonthBounds_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        var action = () => _service.GetMonthBounds(-1);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetMonthBounds_WithIndexGreaterThan5_ThrowsArgumentOutOfRangeException()
    {
        var action = () => _service.GetMonthBounds(6);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetMonthBounds_ConsecutiveMonthsBoundariesAreAdjacent()
    {
        for (int i = 0; i < 5; i++)
        {
            var (currentStart, currentEnd) = _service.GetMonthBounds(i);
            var (nextStart, nextEnd) = _service.GetMonthBounds(i + 1);

            nextStart.Should().Be(currentEnd);
        }
    }

    #endregion
}