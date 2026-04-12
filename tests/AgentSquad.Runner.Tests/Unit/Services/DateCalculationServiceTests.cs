#nullable enable

using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Services;

public class DateCalculationServiceTests
{
    private readonly ILogger<DateCalculationService> mockLogger;
    private readonly DateCalculationService service;

    public DateCalculationServiceTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        mockLogger = loggerFactory.CreateLogger<DateCalculationService>();
        service = new DateCalculationService(mockLogger);
    }

    [Fact]
    public void GetDisplayMonths_ReturnsThreeMonthsAroundCurrent()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = service.GetDisplayMonths(currentDate);

        Assert.Equal(4, result.Count);
        Assert.All(result, m => Assert.NotNull(m.Name));
        Assert.All(result, m => Assert.True(m.Year > 0));
    }

    [Fact]
    public void GetDisplayMonths_IncludesCurrentMonth()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = service.GetDisplayMonths(currentDate);

        var currentMonth = result.FirstOrDefault(m => m.IsCurrentMonth);
        Assert.NotNull(currentMonth);
        Assert.Equal("April", currentMonth.Name);
        Assert.Equal(2026, currentMonth.Year);
    }

    [Fact]
    public void GetDisplayMonths_CurrentMonthIsHighlighted()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = service.GetDisplayMonths(currentDate);

        Assert.Single(result, m => m.IsCurrentMonth);
    }

    [Fact]
    public void GetDisplayMonths_ReturnsCorrectMonthOrder()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = service.GetDisplayMonths(currentDate);

        // Should have 4 months total with current month in the middle
        Assert.NotNull(result[0]); // Previous month
        Assert.NotNull(result[1]); // Previous month (or current)
        Assert.NotNull(result[2]); // Current or next
        Assert.NotNull(result[3]); // Next month

        var currentMonthIndex = result.FindIndex(m => m.IsCurrentMonth);
        Assert.True(currentMonthIndex >= 1 && currentMonthIndex <= 2);
    }

    [Fact]
    public void GetMilestoneXPosition_CalculatesPositionForMilestoneInJanuary()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 1, 15);

        var result = service.GetMilestoneXPosition(milestoneDate, baselineDate);

        Assert.True(result >= 0);
        Assert.True(result <= 260);
    }

    [Fact]
    public void GetMilestoneXPosition_CalculatesPositionForMilestoneInFebruary()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 2, 15);

        var result = service.GetMilestoneXPosition(milestoneDate, baselineDate);

        Assert.True(result > 260);
        Assert.True(result <= 520);
    }

    [Fact]
    public void GetMilestoneXPosition_CalculatesPositionForMilestoneInMarch()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 3, 15);

        var result = service.GetMilestoneXPosition(milestoneDate, baselineDate);

        Assert.True(result > 520);
        Assert.True(result <= 780);
    }

    [Fact]
    public void GetMilestoneXPosition_StartOfMonth_IsAtMonthBoundary()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2026, 2, 1);

        var result = service.GetMilestoneXPosition(milestoneDate, baselineDate);

        Assert.Equal(260, result);
    }

    [Fact]
    public void GetNowMarkerXPosition_CalculatesCurrentDayPosition()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var currentDate = new DateTime(2026, 4, 12);

        var result = service.GetNowMarkerXPosition(currentDate, baselineDate);

        // April is month 4 (index 3), so approximately 3*260 + partial days = ~780 + days into April
        Assert.True(result > 780);
        Assert.True(result < 1040);
    }

    [Fact]
    public void GetNowMarkerXPosition_FirstDayOfMonth_IsNearMonthStart()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var currentDate = new DateTime(2026, 4, 1);

        var result = service.GetNowMarkerXPosition(currentDate, baselineDate);

        // April starts around 780
        Assert.True(result >= 780);
        Assert.True(result < 850);
    }

    [Fact]
    public void GetNowMarkerXPosition_LastDayOfMonth_IsNearMonthEnd()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var currentDate = new DateTime(2026, 4, 30);

        var result = service.GetNowMarkerXPosition(currentDate, baselineDate);

        // April ends around 1040
        Assert.True(result > 990);
        Assert.True(result <= 1040);
    }

    [Theory]
    [InlineData("January", 2026, true)]
    [InlineData("January", 2025, false)]
    [InlineData("February", 2026, false)]
    public void IsCurrentMonth_ComparesMonthAndYear(string monthName, int year, bool expected)
    {
        var currentDate = new DateTime(2026, 1, 15);

        var result = service.IsCurrentMonth(monthName, year, currentDate);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsCurrentMonth_CaseInsensitive()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var resultLower = service.IsCurrentMonth("april", 2026, currentDate);
        var resultUpper = service.IsCurrentMonth("APRIL", 2026, currentDate);
        var resultMixed = service.IsCurrentMonth("ApRiL", 2026, currentDate);

        Assert.True(resultLower);
        Assert.True(resultUpper);
        Assert.True(resultMixed);
    }

    [Theory]
    [InlineData(2026, 1, "January")]
    [InlineData(2026, 2, "February")]
    [InlineData(2026, 3, "March")]
    [InlineData(2026, 4, "April")]
    [InlineData(2026, 5, "May")]
    [InlineData(2026, 6, "June")]
    public void GetDisplayMonths_IncludesExpectedMonthNames(int year, int month, string expectedName)
    {
        var currentDate = new DateTime(year, month, 15);

        var result = service.GetDisplayMonths(currentDate);

        Assert.Contains(result, m => m.Name == expectedName);
    }

    [Fact]
    public void GetDisplayMonths_MonthsHaveCorrectStartAndEndDates()
    {
        var currentDate = new DateTime(2026, 4, 15);

        var result = service.GetDisplayMonths(currentDate);

        var april = result.First(m => m.Name == "April");
        Assert.Equal(new DateTime(2026, 4, 1), april.StartDate);
        Assert.Equal(new DateTime(2026, 4, 30), april.EndDate);
    }

    [Fact]
    public void GetMilestoneXPosition_HandlesPreviousYearMilestone()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2025, 12, 15);

        var result = service.GetMilestoneXPosition(milestoneDate, baselineDate);

        Assert.True(result < 0);
    }

    [Fact]
    public void GetMilestoneXPosition_HandlesNextYearMilestone()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var milestoneDate = new DateTime(2027, 6, 15);

        var result = service.GetMilestoneXPosition(milestoneDate, baselineDate);

        Assert.True(result > 1560);
    }

    [Fact]
    public void GetNowMarkerXPosition_MiddleOfMonth_ReturnsMidpointValue()
    {
        var baselineDate = new DateTime(2026, 1, 1);
        var currentDate = new DateTime(2026, 4, 15);

        var result = service.GetNowMarkerXPosition(currentDate, baselineDate);

        // Approximately 3 months (780) + half of April = 780 + 130 = ~910
        Assert.True(result > 870);
        Assert.True(result < 950);
    }
}