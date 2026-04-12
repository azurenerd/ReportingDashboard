using NodaTime;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Interface for date calculations and timeline positioning.
/// </summary>
public interface IDateCalculationService
{
    /// <summary>
    /// Calculates the 4-month display window for the heatmap grid.
    /// </summary>
    /// <param name="currentDate">Current system date.</param>
    /// <returns>List of 4 MonthInfo objects representing the display window.</returns>
    List<MonthInfo> GetDisplayMonths(DateTime currentDate);

    /// <summary>
    /// Converts a milestone date to its SVG x-coordinate position.
    /// </summary>
    /// <param name="milestoneDate">Date of the milestone.</param>
    /// <param name="baselineDate">Reference date (typically January 1 of the timeline year).</param>
    /// <returns>X-coordinate in pixels (0-1560), clamped to timeline bounds.</returns>
    int GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate);

    /// <summary>
    /// Calculates the SVG x-coordinate for the "Now" marker (current date indicator).
    /// </summary>
    /// <param name="currentDate">Current system date.</param>
    /// <param name="baselineDate">Reference date (typically January 1 of the timeline year).</param>
    /// <returns>X-coordinate in pixels (0-1560).</returns>
    int GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate);

    /// <summary>
    /// Determines if a given year-month matches the current month.
    /// </summary>
    /// <param name="year">Year to check.</param>
    /// <param name="month">Month to check (1-12).</param>
    /// <param name="currentDate">Current system date.</param>
    /// <returns>True if the month contains the current date.</returns>
    bool IsCurrentMonth(int year, int month, DateTime currentDate);

    /// <summary>
    /// Gets the start and end x-coordinates for a month column in the SVG timeline.
    /// </summary>
    /// <param name="monthIndex">Zero-based month index (0 = Jan, 5 = Jun).</param>
    /// <returns>Tuple of (startX, endX) pixel coordinates.</returns>
    (int startX, int endX) GetMonthBounds(int monthIndex);
}