using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using NodaTime;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests.Unit.Services;

public class DateCalculationServiceTests
{
    [Fact]
    public void GetDisplayMonths_Returns4Months()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(mockLogger.Object);
        var currentDate = new DateTime(2026, 4, 15);

        // Act
        var result = service.GetDisplayMonths(currentDate);

        // Assert
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void GetDisplayMonths_IncludesCurrentMonth()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(mockLogger.Object);
        var currentDate = new DateTime(2026, 4, 15);

        // Act
        var result = service.GetDisplayMonths(currentDate);

        // Assert
        var currentMonthInfo = result.FirstOrDefault(m => m.IsCurrentMonth);
        Assert.NotNull(currentMonthInfo);
        Assert.Equal("April", currentMonthInfo.Name);
    }

    [Fact]
    public void GetDisplayMonths_StartsWithPreviousMonth()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(mockLogger.Object);
        var currentDate = new DateTime(2026, 4, 15);

        // Act
        var result = service.GetDisplayMonths(currentDate);

        // Assert
        Assert.Equal("March", result[0].Name);
        Assert.Equal("April", result[1].Name);
        Assert.Equal("May", result[2].Name);
        Assert.Equal("June", result[3].Name);
    }

    [Fact]
    public void GetMilestoneXPosition_Returns0_ForJanuary1st()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(mockLogger.Object);
        var milestoneDate = new DateTime(2026, 1, 1);
        var baselineDate = new DateTime(2026, 1, 1);

        // Act
        var result = service.GetMilestoneXPosition(milestoneDate, baselineDate);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetMilestoneXPosition_ThrowsArgumentOutOfRangeException_WhenYearDiffers()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(mockLogger.Object);
        var milestoneDate = new DateTime(2027, 4, 15);
        var baselineDate = new DateTime(2026, 1, 1);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            service.GetMilestoneXPosition(milestoneDate, baselineDate));
    }

    [Fact]
    public void GetNowMarkerXPosition_ReturnsSameAsGetMilestoneXPosition()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(mockLogger.Object);
        var currentDate = new DateTime(2026, 4, 15);
        var baselineDate = new DateTime(2026, 1, 1);

        // Act
        var milestoneResult = service.GetMilestoneXPosition(currentDate, baselineDate);
        var nowMarkerResult = service.GetNowMarkerXPosition(currentDate, baselineDate);

        // Assert
        Assert.Equal(milestoneResult, nowMarkerResult);
    }

    [Fact]
    public void IsCurrentMonth_ReturnsTrue_WhenMonthIsCurrentMonth()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(mockLogger.Object);
        var currentDate = new DateTime(2026, 4, 15);
        var month = new YearMonth(2026, 4);

        // Act
        var result = service.IsCurrentMonth(month, currentDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsCurrentMonth_ReturnsFalse_WhenMonthIsNotCurrentMonth()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(mockLogger.Object);
        var currentDate = new DateTime(2026, 4, 15);
        var month = new YearMonth(2026, 3);

        // Act
        var result = service.IsCurrentMonth(month, currentDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCurrentMonth_ReturnsFalse_WhenYearDiffers()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(mockLogger.Object);
        var currentDate = new DateTime(2026, 4, 15);
        var month = new YearMonth(2025, 4);

        // Act
        var result = service.IsCurrentMonth(month, currentDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetMonthBounds_ReturnsCorrectBounds_ForFirstMonth()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(mockLogger.Object);

        // Act
        var (startX, endX) = service.GetMonthBounds(0);

        // Assert
        Assert.Equal(0, startX);
        Assert.Equal(260, endX);
    }

    [Fact]
    public void GetMonthBounds_ReturnsCorrectBounds_ForSecondMonth()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(mockLogger.Object);

        // Act
        var (startX, endX) = service.GetMonthBounds(1);

        // Assert
        Assert.Equal(260, startX);
        Assert.Equal(520, endX);
    }

    [Fact]
    public void GetMonthBounds_ThrowsArgumentOutOfRangeException_WhenIndexNegative()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(mockLogger.Object);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => service.GetMonthBounds(-1));
    }

    [Fact]
    public void GetMonthBounds_ThrowsArgumentOutOfRangeException_WhenIndexTooLarge()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DateCalculationService>>();
        var service = new DateCalculationService(mockLogger.Object);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => service.GetMonthBounds(6));
    }
}