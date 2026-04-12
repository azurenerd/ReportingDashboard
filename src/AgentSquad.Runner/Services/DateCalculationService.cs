using NodaTime;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for date calculations and timeline positioning.
/// Converts dates to SVG pixel coordinates and manages month boundaries for the dashboard timeline.
/// </summary>
public class DateCalculationService : IDateCalculationService
{
    /// <summary>
    /// Pixels per month on the SVG timeline (6 months × 260px = 1560px total).
    /// </summary>
    public const int PixelsPerMonth = 260;

    /// <summary>
    /// Total SVG timeline width in pixels (6 months: Jan-Jun).
    /// </summary>
    public const int SvgWidth = 1560;

    /// <summary>
    /// Number of months displayed in the timeline window.
    /// </summary>
    private const int MonthsInTimeline = 6;

    private readonly ILogger<DateCalculationService> _logger;

    public DateCalculationService(ILogger<DateCalculationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculates the 4-month display window for the heatmap grid.
    /// Window starts from current month - 1 and extends 3 months forward.
    /// </summary>
    /// <param name="currentDate">Current system date.</param>
    /// <returns>List of 4 MonthInfo objects representing the display window.</returns>
    public List<MonthInfo> GetDisplayMonths(DateTime currentDate)
    {
        var months = new List<MonthInfo>();
        var currentMonth = currentDate.Month;
        var currentYear = currentDate.Year;

        var startMonth = currentMonth - 1;
        var startYear = currentYear;

        if (startMonth < 1)
        {
            startMonth += 12;
            startYear -= 1;
        }

        for (int i = 0; i < 4; i++)
        {
            var month = startMonth + i;
            var year = startYear;

            if (month > 12)
            {
                month -= 12;
                year += 1;
            }

            var monthStartDate = new LocalDate(year, month, 1);
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var monthEndDate = new LocalDate(year, month, daysInMonth);

            var monthInfo = new MonthInfo
            {
                Name = monthStartDate.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture),
                Year = year,
                StartDate = monthStartDate.ToDateTimeUnspecified(),
                EndDate = monthEndDate.ToDateTimeUnspecified(),
                GridColumnIndex = i,
                IsCurrentMonth = year == currentYear && month == currentMonth
            };

            months.Add(monthInfo);
        }

        return months;
    }

    /// <summary>
    /// Converts a milestone date to its SVG x-coordinate position.
    /// Calculates position based on days elapsed since January 1 of the timeline year.
    /// </summary>
    /// <param name="milestoneDate">Date of the milestone.</param>
    /// <param name="baselineDate">Reference date (typically January 1 of the timeline year).</param>
    /// <returns>X-coordinate in pixels (0-1560), clamped to timeline bounds.</returns>
    public int GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate)
    {
        var baseline = LocalDate.FromDateTime(baselineDate);
        var milestone = LocalDate.FromDateTime(milestoneDate);

        var period = Period.Between(baseline, milestone, PeriodUnits.Days);
        var daysFromBaseline = period.Days;

        const int daysInTimeline = 182;

        if (daysFromBaseline < 0 || daysFromBaseline > daysInTimeline)
        {
            _logger.LogWarning(
                "Milestone date {MilestoneDate} is outside expected timeline range. Days from baseline: {Days}",
                milestoneDate.ToString("yyyy-MM-dd"),
                daysFromBaseline);
        }

        var xPosition = (daysFromBaseline / (double)daysInTimeline) * SvgWidth;

        return (int)Math.Max(0, Math.Min(SvgWidth, xPosition));
    }

    /// <summary>
    /// Calculates the SVG x-coordinate for the "Now" marker (current date indicator).
    /// </summary>
    /// <param name="currentDate">Current system date.</param>
    /// <param name="baselineDate">Reference date (typically January 1 of the timeline year).</param>
    /// <returns>X-coordinate in pixels (0-1560).</returns>
    public int GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate)
    {
        return GetMilestoneXPosition(currentDate, baselineDate);
    }

    /// <summary>
    /// Determines if a given year-month matches the current month.
    /// </summary>
    /// <param name="year">Year to check.</param>
    /// <param name="month">Month to check (1-12).</param>
    /// <param name="currentDate">Current system date.</param>
    /// <returns>True if the month contains the current date.</returns>
    public bool IsCurrentMonth(int year, int month, DateTime currentDate)
    {
        return year == currentDate.Year && month == currentDate.Month;
    }

    /// <summary>
    /// Gets the start and end x-coordinates for a month column in the SVG timeline.
    /// </summary>
    /// <param name="monthIndex">Zero-based month index (0 = Jan, 5 = Jun).</param>
    /// <returns>Tuple of (startX, endX) pixel coordinates.</returns>
    public (int startX, int endX) GetMonthBounds(int monthIndex)
    {
        if (monthIndex < 0 || monthIndex >= MonthsInTimeline)
        {
            throw new ArgumentOutOfRangeException(
                nameof(monthIndex),
                $"Month index must be between 0 and {MonthsInTimeline - 1}");
        }

        var startX = monthIndex * PixelsPerMonth;
        var endX = startX + PixelsPerMonth;

        return (startX, endX);
    }
}