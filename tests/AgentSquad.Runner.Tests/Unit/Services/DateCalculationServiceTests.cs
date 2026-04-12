using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests.Unit.Services;

/// <summary>
/// Unit tests for DateCalculationService coordinate calculation methods.
/// Verifies pixel-to-date conversions and month boundary calculations.
/// </summary>
public class DateCalculationServiceTests
{
    private readonly DateCalculationService _service;
    private readonly Mock<ILogger<DateCalculationService>> _loggerMock;

    public DateCalculationServiceTests()
    {
        _loggerMock = new Mock<ILogger<DateCalculationService>>();
        _service = new DateCalculationService(_loggerMock.Object);
    }

    #region GetDisplayMonths Tests

    [Fact]
    public void GetDisplayMonths_WithAprilCurrentDate_ReturnsMarchToJuneWindow()
    {
        // Arrange
        var currentDate = new DateTime(2026, 4, 12);

        // Act
        var months = _service.GetDisplayMonths(currentDate);

        // Assert
        Assert.Equal(4, months.Count);
        Assert.Equal("March", months[0].Name);
        Assert.Equal("April", months[1].Name);
        Assert.Equal("May", months[2].Name);
        Assert.Equal("June", months[3].Name);

        Assert.False(months[0].IsCurrentMonth);
        Assert.True(months[1].IsCurrentMonth);
        Assert.False(months[2].IsCurrentMonth);
        Assert.False(months[3].IsCurrentMonth);
    }

    [Fact]
    public void GetDisplayMonths_WithJanuaryCurrentDate_ReturnsDecemberToMarchWindow()
    {
        // Arrange
        var currentDate = new DateTime(2026, 1, 15);

        // Act
        var months = _service.GetDisplayMonths(currentDate);

        // Assert
        Assert.Equal(4, months.Count);
        Assert.Equal("December", months[0].Name);
        Assert.Equal(2025, months[0].Year);
        Assert.Equal("January", months[1].Name);
        Assert.Equal(2026, months[1].Year);
        Assert.True(months[1].IsCurrentMonth);
    }

    [Fact]
    public void GetDisplayMonths_ReturnsCorrectStartAndEndDates()
    {
        // Arrange
        var currentDate = new DateTime(2026, 4, 12);

        // Act
        var months = _service.GetDisplayMonths(currentDate);

        // Assert
        Assert.Equal(new DateTime(2026, 3, 1), months[0].StartDate);
        Assert.Equal(new DateTime(2026, 3, 31), months[0].EndDate);

        Assert.Equal(new DateTime(2026, 4, 1), months[1].StartDate);
        Assert.Equal(new DateTime(2026, 4, 30), months[1].EndDate);
    }

    #endregion

    #region GetMilestoneXPosition Tests

    [Fact]
    public void GetMilestoneXPosition_WithJanuaryFirstDate_ReturnsZero()
    {
        // Arrange
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 1, 1);

        // Act
        var xPosition = _service.GetMilestoneXPosition(milestoneDate, baselineDate);

        // Assert
        Assert.Equal(0, xPosition);
    }

    [Fact]
    public void GetMilestoneXPosition_WithFebruaryFirstDate_ReturnsApproximately260Pixels()
    {
        // Arrange
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 2, 1);

        // Act
        var xPosition = _service.GetMilestoneXPosition(milestoneDate, baselineDate);

        // Assert
        // February 1 is 31 days from Jan 1
        // (31 / 182) * 1560 ≈ 265 pixels
        Assert.InRange(xPosition, 260, 270);
    }

    [Fact]
    public void GetMilestoneXPosition_WithMidAprilDate_ReturnsApproximately890Pixels()
    {
        // Arrange
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 4, 15);

        // Act
        var xPosition = _service.GetMilestoneXPosition(milestoneDate, baselineDate);

        // Assert
        // April 15 is approximately 105 days from Jan 1
        // (105 / 182) * 1560 ≈ 897 pixels
        Assert.InRange(xPosition, 890, 910);
    }

    [Fact]
    public void GetMilestoneXPosition_WithJuneTwentiethDate_ReturnsApproximately1500Pixels()
    {
        // Arrange
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 6, 20);

        // Act
        var xPosition = _service.GetMilestoneXPosition(milestoneDate, baselineDate);

        // Assert
        // June 20 is approximately 170 days from Jan 1
        // Should be near or at the upper bound of 1560
        Assert.InRange(xPosition, 1500, 1560);
    }

    [Fact]
    public void GetMilestoneXPosition_ClampsNegativeValuesToZero()
    {
        // Arrange
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2025, 12, 1);

        // Act
        var xPosition = _service.GetMilestoneXPosition(milestoneDate, baselineDate);

        // Assert
        Assert.Equal(0, xPosition);
    }

    [Fact]
    public void GetMilestoneXPosition_LogsWarningForOutOfRangeDates()
    {
        // Arrange
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 12, 31);
        var loggerMock = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(loggerMock.Object);

        // Act
        var xPosition = service.GetMilestoneXPosition(milestoneDate, baselineDate);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetNowMarkerXPosition Tests

    [Fact]
    public void GetNowMarkerXPosition_WithApril12Date_ReturnsExpectedPixelPosition()
    {
        // Arrange
        var baselineDate = new DateTime(2026, 1, 1);
        var currentDate = new DateTime(2026, 4, 12);

        // Act
        var xPosition = _service.GetNowMarkerXPosition(currentDate, baselineDate);

        // Assert
        // Should position marker in April (between 780 and 1040 pixels)
        Assert.InRange(xPosition, 780, 1040);
    }

    [Fact]
    public void GetNowMarkerXPosition_MatchesGetMilestoneXPosition()
    {
        // Arrange
        var baselineDate = new DateTime(2026, 1, 1);
        var date = new DateTime(2026, 3, 26);

        // Act
        var nowPosition = _service.GetNowMarkerXPosition(date, baselineDate);
        var milestonePosition = _service.GetMilestoneXPosition(date, baselineDate);

        // Assert
        Assert.Equal(milestonePosition, nowPosition);
    }

    #endregion

    #region GetMonthBounds Tests

    [Fact]
    public void GetMonthBounds_WithZeroIndex_ReturnsJanuaryBounds()
    {
        // Act
        var (startX, endX) = _service.GetMonthBounds(0);

        // Assert
        Assert.Equal(0, startX);
        Assert.Equal(260, endX);
    }

    [Fact]
    public void GetMonthBounds_WithIndexThree_ReturnsAprilBounds()
    {
        // Act
        var (startX, endX) = _service.GetMonthBounds(3);

        // Assert
        Assert.Equal(780, startX);
        Assert.Equal(1040, endX);
    }

    [Fact]
    public void GetMonthBounds_WithIndexFive_ReturnsJuneBounds()
    {
        // Act
        var (startX, endX) = _service.GetMonthBounds(5);

        // Assert
        Assert.Equal(1300, startX);
        Assert.Equal(1560, endX);
    }

    [Fact]
    public void GetMonthBounds_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetMonthBounds(-1));
    }

    [Fact]
    public void GetMonthBounds_WithIndexGreaterThanFive_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetMonthBounds(6));
    }

    [Fact]
    public void GetMonthBounds_AllMonthsCoverFullTimeline()
    {
        // Arrange & Act
        var month0 = _service.GetMonthBounds(0);
        var month5 = _service.GetMonthBounds(5);

        // Assert
        Assert.Equal(0, month0.startX);
        Assert.Equal(1560, month5.endX);
    }

    #endregion

    #region IsCurrentMonth Tests

    [Fact]
    public void IsCurrentMonth_WithCurrentMonth_ReturnsTrue()
    {
        // Arrange
        var currentDate = new DateTime(2026, 4, 15);

        // Act
        var result = _service.IsCurrentMonth(2026, 4, currentDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsCurrentMonth_WithPastMonth_ReturnsFalse()
    {
        // Arrange
        var currentDate = new DateTime(2026, 4, 15);

        // Act
        var result = _service.IsCurrentMonth(2026, 3, currentDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCurrentMonth_WithFutureMonth_ReturnsFalse()
    {
        // Arrange
        var currentDate = new DateTime(2026, 4, 15);

        // Act
        var result = _service.IsCurrentMonth(2026, 5, currentDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCurrentMonth_WithDifferentYear_ReturnsFalse()
    {
        // Arrange
        var currentDate = new DateTime(2026, 4, 15);

        // Act
        var result = _service.IsCurrentMonth(2025, 4, currentDate);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Constants Tests

    [Fact]
    public void Constants_PixelsPerMonthEquals260()
    {
        Assert.Equal(260, DateCalculationService.PixelsPerMonth);
    }

    [Fact]
    public void Constants_SvgWidthEquals1560()
    {
        Assert.Equal(1560, DateCalculationService.SvgWidth);
    }

    #endregion
}