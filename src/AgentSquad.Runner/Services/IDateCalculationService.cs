#nullable enable

using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service interface for date-related calculations for the dashboard.
/// Handles month boundary calculations, milestone positioning, and timeline computations.
/// </summary>
public interface IDateCalculationService
{
    /// <summary>
    /// Calculate the 4-month display window based on the current date.
    /// </summary>
    List<MonthInfo> GetDisplayMonths(DateTime currentDate);

    /// <summary>
    /// Convert a milestone date to an SVG x-coordinate based on a baseline date.
    /// </summary>
    int GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate);

    /// <summary>
    /// Calculate the x-coordinate of the "Now" marker (red dashed line) on the timeline.
    /// </summary>
    int GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate);

    /// <summary>
    /// Determine if a given month/year is the current month.
    /// </summary>
    bool IsCurrentMonth(string monthName, int year, DateTime currentDate);
}