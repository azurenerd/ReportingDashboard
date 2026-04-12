using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Service interface for date calculations, month boundary calculations, and timeline positioning.
    /// Handles all date-driven coordinate calculations for SVG rendering and heatmap display.
    /// </summary>
    public interface IDateCalculationService
    {
        /// <summary>
        /// Get 4-month display window (current month + 3 future months) based on current date.
        /// </summary>
        /// <param name="currentDate">Reference date for calculating display window.</param>
        /// <returns>List of MonthInfo objects for the 4-month window.</returns>
        List<MonthInfo> GetDisplayMonths(DateTime currentDate);

        /// <summary>
        /// Convert milestone date to SVG x-coordinate (pixels).
        /// </summary>
        /// <param name="milestoneDate">Date of the milestone to position.</param>
        /// <param name="baselineDate">Baseline date (typically January 1) for coordinate calculation.</param>
        /// <returns>X-coordinate in pixels (0-1560) for SVG rendering.</returns>
        int GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate);

        /// <summary>
        /// Get x-coordinate for "Now" marker (red dashed line) based on current date.
        /// </summary>
        /// <param name="currentDate">Current date to position the marker.</param>
        /// <param name="baselineDate">Baseline date for coordinate calculation.</param>
        /// <returns>X-coordinate in pixels for SVG rendering.</returns>
        int GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate);

        /// <summary>
        /// Determine if a given month/year matches the current month for highlighting.
        /// </summary>
        /// <param name="month">Month name string (e.g., "April").</param>
        /// <param name="year">Year as integer (e.g., 2026).</param>
        /// <param name="currentDate">Current date to compare.</param>
        /// <returns>True if the month/year matches current month; false otherwise.</returns>
        bool IsCurrentMonth(string month, int year, DateTime currentDate);

        /// <summary>
        /// Get the start and end x-positions for a month column in the SVG timeline.
        /// </summary>
        /// <param name="monthIndex">Month index (0=January, 5=June).</param>
        /// <returns>Tuple of (startX, endX) pixel coordinates.</returns>
        (int startX, int endX) GetMonthBounds(int monthIndex);
    }
}