using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgentSquad.Runner.Tests;

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
    public void GetDisplayMonths_WithCurrentDateInApril_Returns4MonthsStartingInMarch()
    {
        // Arrange
        var currentDate = new DateTime(2026, 4, 12);

        // Act
        var months = _service.GetDisplayMonths(currentDate);

        // Assert
        Assert.Equal(4, months.Count);
        Assert.Equal("March", months[0].Name);
        Assert.Equal(2026, months[0].Year);
        Assert.False(months[0].IsCurrentMonth);

        Assert.Equal("April", months[1].Name);
        Assert.Equal(2026, months[1].Year);
        Assert.True(months[1].IsCurrentMonth);

        Assert.Equal("May", months[2].Name);
        Assert.Equal(2026, months[2].Year);
        Assert.False(months[2].IsCurrentMonth);

        Assert.Equal("June", months[3].Name);
        Assert.Equal(2026, months[3].Year);
        Assert.False(months[3].IsCurrentMonth);
    }

    [Fact]
    public void GetDisplayMonths_WithCurrentDateInJanuary_Returns4MonthsStartingInJanuary()
    {
        // Arrange
        var currentDate = new DateTime(2026, 1, 15);

        // Act
        var months = _service.GetDisplayMonths(currentDate);

        // Assert
        Assert.Equal(4, months.Count);
        Assert.Equal("January", months[0].Name);
        Assert.True(months[0].IsCurrentMonth);
        Assert.Equal("February", months[1].Name);
        Assert.Equal("March", months[2].Name);
        Assert.Equal("April", months[3].Name);
    }

    [Fact]
    public void GetDisplayMonths_SetCorrectDateRanges()
    {
        // Arrange
        var currentDate = new DateTime(2026, 4, 12);

        // Act
        var months = _service.GetDisplayMonths(currentDate);

        // Assert - March
        Assert.Equal(new DateTime(2026, 3, 1), months[0].StartDate);
        Assert.Equal(new DateTime(2026, 3, 31), months[0].EndDate);

        // Assert - April
        Assert.Equal(new DateTime(2026, 4, 1), months[1].StartDate);
        Assert.Equal(new DateTime(2026, 4, 30), months[1].EndDate);

        // Assert - May
        Assert.Equal(new DateTime(2026, 5, 1), months[2].StartDate);
        Assert.Equal(new DateTime(2026, 5, 31), months[2].EndDate);

        // Assert - June
        Assert.Equal(new DateTime(2026, 6, 1), months[3].StartDate);
        Assert.Equal(new DateTime(2026, 6, 30), months[3].EndDate);
    }

    [Fact]
    public void GetMilestoneXPosition_WithJanuaryDate_ReturnsLowPixelValue()
    {
        // Arrange
        var milestone = new DateTime(2026, 1, 15);
        var baseline = new DateTime(2026, 1, 1);

        // Act
        var position = _service.GetMilestoneXPosition(milestone, baseline);

        // Assert - Should be roughly 14 days into the timeline
        Assert.True(position > 0 && position < 150, $"Expected position between 0-150, got {position}");
    }

    [Fact]
    public void GetMilestoneXPosition_WithMarchDate_ReturnsMiddlePixelValue()
    {
        // Arrange
        var milestone = new DateTime(2026, 3, 26);
        var baseline = new DateTime(2026, 1, 1);

        // Act
        var position = _service.GetMilestoneXPosition(milestone, baseline);

        // Assert - March 26 should be around 85 days from Jan 1
        // 1560 / 184 * 85 ≈ 723
        Assert.True(position > 600 && position < 800, $"Expected position between 600-800, got {position}");
    }

    [Fact]
    public void GetMilestoneXPosition_WithJuneDate_ReturnsHighPixelValue()
    {
        // Arrange
        var milestone = new DateTime(2026, 6, 30);
        var baseline = new DateTime(2026, 1, 1);

        // Act
        var position = _service.GetMilestoneXPosition(milestone, baseline);

        // Assert - June 30 should be at or near the end of timeline (1560px)
        Assert.True(position > 1400, $"Expected position > 1400, got {position}");
    }

    [Fact]
    public void GetNowMarkerXPosition_WithAprilDate_ReturnsCorrectPosition()
    {
        // Arrange
        var currentDate = new DateTime(2026, 4, 12);
        var baseline = new DateTime(2026, 1, 1);

        // Act
        var position = _service.GetNowMarkerXPosition(currentDate, baseline);

        // Assert - April 12 should be around 102 days from Jan 1
        // 1560 / 184 * 102 ≈ 863
        Assert.True(position > 750 && position < 950, $"Expected position between 750-950, got {position}");
    }

    [Fact]
    public void IsCurrentMonth_WithMatchingMonth_ReturnsTrue()
    {
        // Arrange
        var currentDate = new DateTime(2026, 4, 12);

        // Act
        var isCurrentMonth = _service.IsCurrentMonth("April", 2026, currentDate);

        // Assert
        Assert.True(isCurrentMonth);
    }

    [Fact]
    public void IsCurrentMonth_WithDifferentMonth_ReturnsFalse()
    {
        // Arrange
        var currentDate = new DateTime(2026, 4, 12);

        // Act
        var isCurrentMonth = _service.IsCurrentMonth("March", 2026, currentDate);

        // Assert
        Assert.False(isCurrentMonth);
    }

    [Fact]
    public void IsCurrentMonth_WithDifferentYear_ReturnsFalse()
    {
        // Arrange
        var currentDate = new DateTime(2026, 4, 12);

        // Act
        var isCurrentMonth = _service.IsCurrentMonth("April", 2027, currentDate);

        // Assert
        Assert.False(isCurrentMonth);
    }

    [Fact]
    public void IsCurrentMonth_CaseInsensitive()
    {
        // Arrange
        var currentDate = new DateTime(2026, 4, 12);

        // Act
        var isCurrentMonth = _service.IsCurrentMonth("april", 2026, currentDate);

        // Assert
        Assert.True(isCurrentMonth);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 260)]
    [InlineData(2, 520)]
    [InlineData(3, 780)]
    [InlineData(4, 1040)]
    [InlineData(5, 1300)]
    public void GetMonthGridLinePosition_ReturnsCorrectPixelPositions(int monthIndex, int expectedPosition)
    {
        // Act
        var position = _service.GetMonthGridLinePosition(monthIndex);

        // Assert
        Assert.Equal(expectedPosition, position);
    }

    [Theory]
    [InlineData(0, 0, 260)]
    [InlineData(1, 260, 520)]
    [InlineData(2, 520, 780)]
    [InlineData(3, 780, 1040)]
    [InlineData(4, 1040, 1300)]
    [InlineData(5, 1300, 1560)]
    public void GetMonthBounds_ReturnsCorrectStartAndEndPositions(int monthIndex, int expectedStart, int expectedEnd)
    {
        // Act
        var bounds = _service.GetMonthBounds(monthIndex);

        // Assert
        Assert.Equal(expectedStart, bounds.StartX);
        Assert.Equal(expectedEnd, bounds.EndX);
    }

    [Fact]
    public void GetMonthBounds_WithInvalidIndex_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetMonthBounds(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetMonthBounds(6));
    }

    [Fact]
    public void GetMonthGridLinePosition_WithInvalidIndex_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetMonthGridLinePosition(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetMonthGridLinePosition(6));
    }
}