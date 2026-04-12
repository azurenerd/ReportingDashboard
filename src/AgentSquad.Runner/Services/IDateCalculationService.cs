using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for date calculations, month boundaries, and timeline positioning
/// </summary>
public interface IDateCalculationService
{
    /// <summary>
    /// Calculate the 4-month display window based on current date
    /// </summary>
    /// <param name="currentDate">Reference date (typically DateTime.UtcNow)</param>
    /// <returns>List of MonthInfo objects for display months</returns>
    List<MonthInfo> GetDisplayMonths(DateTime currentDate);

    /// <summary>
    /// Convert a milestone date to SVG x-coordinate (pixels)
    /// </summary>
    /// <param name="milestoneDate">Date of milestone</param>
    /// <param name="baselineDate">Reference baseline (typically Jan 1 of year)</param>
    /// <returns>X position in pixels (0-1560)</returns>
    int GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate);

    /// <summary>
    /// Calculate x-coordinate of "Now" marker (red dashed line)
    /// </summary>
    /// <param name="currentDate">Current date</param>
    /// <param name="baselineDate">Reference baseline</param>
    /// <returns>X position in pixels</returns>
    int GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate);

    /// <summary>
    /// Determine if a given month is the current month
    /// </summary>
    /// <param name="monthName">Month name (January, February, etc.)</param>
    /// <param name="year">Year</param>
    /// <param name="currentDate">Current date for comparison</param>
    /// <returns>True if month/year matches current month</returns>
    bool IsCurrentMonth(string monthName, int year, DateTime currentDate);

    /// <summary>
    /// Get the start and end x-coordinates for a month grid column
    /// </summary>
    /// <param name="monthIndex">Index of month in display (0-3)</param>
    /// <returns>Tuple of (startX, endX) pixel coordinates</returns>
    (int startX, int endX) GetMonthBounds(int monthIndex);
}